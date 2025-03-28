using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BeatyBit.Binstate;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;


namespace Binstate.Tests;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
public class ArgumentPassingTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_argument_to_enter(RaiseWay raiseWay)
  {
    const string expected = "expected";
    var          actual   = expected + "bad";

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder
     .DefineState(Initial)
     .AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter<string>((sm, param) => actual = param);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToX, expected);

    // --assert
    actual.Should().Be(expected);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_argument_if_argument_is_differ_but_assignable_to_enter_action_argument(RaiseWay raiseWay)
  {
    var expected = new MemoryStream();
    var onEnter  = A.Fake<Action<IDisposable>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToX, expected);

    // --assert
    A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_argument_if_parent_and_child_argument_are_differ_but_assignable_and_enter_with_no_argument_on_the_pass(RaiseWay raiseWay)
  {
    var onEnterRoot  = A.Fake<Action<IDisposable>>();
    var onEnterChild = A.Fake<Action<Stream>>();
    var expected     = new MemoryStream();

    // --arrange
    var builder = new Builder<string, string>(OnException);

    builder.DefineState(Initial).AddTransition(Child, Child);
    builder.DefineState(Root).OnEnter(onEnterRoot);
    builder.DefineState(Parent).AsSubstateOf(Root).OnEnter(sm => { });
    builder.DefineState(Child).AsSubstateOf(Parent).OnEnter(onEnterChild);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, Child, expected);

    // --assert
    A.CallTo(() => onEnterRoot(expected)).MustHaveHappenedOnceExactly();
    A.CallTo(() => onEnterChild(expected)).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_throw_exception_if_argument_is_not_assignable_to_enter_action(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder
     .DefineState(Initial)
     .AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter<string>((sm, value) => { });

    var stateMachine = builder.Build(Initial);

    // --act
    Action target = () => stateMachine.Raise(raiseWay, GoToX, 983);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage($"The state '{StateX}' requires argument of type '{typeof(string)}' but no argument*");
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_throw_exception_if_no_argument_specified_for_enter_action_with_argument(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder
     .DefineState(Initial)
     .AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter<int>(value => { });

    var stateMachine = builder.Build(Initial);

    // --act
    Action target = () => stateMachine.Raise(raiseWay, GoToX);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage($"The state '{StateX}' requires argument of type '{typeof(int)}' but no argument*");
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_throw_exception_if_parent_and_child_state_has_not_assignable_arguments_enable_free_mode_and_argument_is_passed(RaiseWay way)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException, new Builder.Options { ArgumentTransferMode = ArgumentTransferMode.Free });

    builder.DefineState(Initial).AddTransition(GoToX, Child);

    builder.DefineState(Parent)
           .OnEnter<int>((stateMachine, value) => { });

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<string>(value => { });

    // --act
    var sm = builder.Build(Initial);

    Action target = () => sm.Raise(GoToX, "stringArgument");

    // --assert
    target
     .Should()
     .Throw<TransitionException>()
     .WithMessage($"The state '{Parent}' requires argument of type '{typeof(int)}' but no argument*");
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_the_same_argument_to_enter_exit_and_transition(RaiseWay raiseWay)
  {
    var expected     = new MemoryStream();
    var onEnter      = A.Fake<Action<IDisposable>>();
    var onExit       = A.Fake<Action<IDisposable>>();
    var onTransition = A.Fake<Action<IDisposable>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AddTransition(GoToX, StateX);
    builder.DefineState(StateY);
    builder.DefineState(StateX)
           .OnEnter(onEnter)
           .OnExit(onExit)
           .AddTransition(GoToY, StateY, onTransition);

    var target = builder.Build(Initial);

    // --act
    target.Raise(GoToX, expected);
    target.Raise(GoToY);

    // --assert
    A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onExit(expected)).MustHaveHappenedOnceExactly())
     .Then(A.CallTo(() => onTransition(expected)).MustHaveHappenedOnceExactly());
  }

  [Test]
  public void null_argument_should_be_passed_test()
  {
    var enter = A.Fake<Action<string?>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder
     .DefineState(Initial)
     .AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter<string?>(enter);

    var target = builder.Build(Initial);

    // --act
    target.Raise<string?>(GoToX, null);

    // --assert
    A.CallTo(() => enter(null)).MustHaveHappenedOnceExactly();
  }

  #region Group 1: Base explicit argument passing

  [Test]
  public void ExplicitArgument_String_IsPassedToTargetState()
  {
    const string expectedValue = "value1";

    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string>>();

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX);

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, expectedValue);

    // --assert
    A.CallTo(() => onEnterStateX(expectedValue)).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void ExplicitArgument_Int_IsPassedToTargetState()
  {
    const int expectedValue = 123;

    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<int>>();

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX); // StateX expects int

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, expectedValue); // Raise<int>

    // --assert
    A.CallTo(() => onEnterStateX(expectedValue)).MustHaveHappenedOnceExactly();
  }

  #endregion

  #region Group 2: Base argument propagating

  [Test]
  public void PropagatedArgument_SameType_IsPassedWhenNoExplicitArgument()
  {
    const string propagatedValue = "value1";

    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string>>();
    var onEnterStateY = A.Fake<Action<string>>();

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX) // StateX expects string and sets the argument
           .AddTransition(GoToY, StateY);

    builder.DefineState(StateY)
           .OnEnter(onEnterStateY); // StateY expects string

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, propagatedValue); // StateX gets "value1"
    stateMachine.Raise(GoToY);                  // Transition to StateY without explicit arg

    // --assert
    A.CallTo(() => onEnterStateX(propagatedValue)).MustHaveHappenedOnceExactly(); // Verify setup
    A.CallTo(() => onEnterStateY(propagatedValue)).MustHaveHappenedOnceExactly(); // Verify propagation
  }

  #endregion

  #region Group 3: Priority of Explicit Passing over Propagation

  [Test]
  public void ExplicitArgument_Overrides_PropagatedArgument()
  {
    const string propagatedValue = "value1";
    const string explicitValue   = "value2";

    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string>>();
    var onEnterStateY = A.Fake<Action<string>>();

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX) // StateX expects string and sets the argument
           .AddTransition(GoToY, StateY);

    builder.DefineState(StateY)
           .OnEnter(onEnterStateY); // StateY expects string

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, propagatedValue); // StateX gets "value1"
    stateMachine.Raise(GoToY, explicitValue);   // Transition to StateY WITH explicit "value2"

    // --assert
    A.CallTo(() => onEnterStateX(propagatedValue)).MustHaveHappenedOnceExactly(); // Verify setup
    A.CallTo(() => onEnterStateY(explicitValue)).MustHaveHappenedOnceExactly();   // Verify explicit override
    A.CallTo(() => onEnterStateY(propagatedValue)).MustNotHaveHappened();
  }

  #endregion

  #region Group 4: Argument Overwrite

  [Test]
  public void Argument_IsOverwritten_OnReentryWithExplicitArgument()
  {
    const string value1 = "value1";
    const string value2 = "value2";

    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string>>();
    var onEnterStateY = A.Fake<Action>(); // StateY doesn't need an argument

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX) // StateX expects string
           .AddTransition(GoToY, StateY);

    builder.DefineState(StateY)
           .OnEnter(onEnterStateY)
           .AddTransition(GoToX, StateX); // Transition back to StateX

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, value1); // StateX enters with "value1"
    stateMachine.Raise(GoToY);         // Go to StateY
    stateMachine.Raise(GoToX, value2); // Go back to StateX with explicit "value2"

    // --assert
    // Check calls in order
    A.CallTo(() => onEnterStateX(value1))
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onEnterStateY()).MustHaveHappenedOnceExactly())
     .Then(A.CallTo(() => onEnterStateX(value2)).MustHaveHappenedOnceExactly());
  }

  [Test]
  public void Argument_IsOverwritten_OnReentryViaPropagation()
  {
    const string valueX1 = "valueX_1";
    const string valueX2 = "valueX_2";

    // --arrange (The simplified version of a scenario focusing on overwriting via propagation)
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string>>();
    var onEnterStateY = A.Fake<Action<string>>();
    var onEnterStateZ = A.Fake<Action>(); // StateZ has no argument

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX) // StateX expects string
           .AddTransition(GoToY, StateY)
           .AddTransition(GoToZ, StateZ); // Need a path away from Y

    builder.DefineState(StateY)
           .OnEnter(onEnterStateY)        // StateY expects string
           .AddTransition(GoToZ, StateZ); // Path away from Y

    builder.DefineState(StateZ)
           .OnEnter(onEnterStateZ)        // No argument
           .AddTransition(GoToX, StateX); // Path back to X, allows setting new value

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, valueX1); // StateX -> "valueX1", OnEnterX(valueX1) called
    stateMachine.Raise(GoToY);          // StateY -> "valueX1" (propagated), OnEnterY(valueX1) called
    stateMachine.Raise(GoToZ);          // Go to StateZ (no arg), OnEnterZ() called
    stateMachine.Raise(GoToX, valueX2); // Go back to StateX explicitly with "valueX2", OnEnterX(valueX2) called
    stateMachine.Raise(GoToY);          // Go to StateY again. Should propagate "valueX2" from StateX. OnEnterY(valueX2) called

    // --assert
    // Verify the sequence and arguments for StateY's OnEnter
    A.CallTo(() => onEnterStateY(valueX1))
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onEnterStateY(valueX2)).MustHaveHappenedOnceExactly());

    // Optionally verify StateX calls too
    A.CallTo(() => onEnterStateX(valueX1))
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onEnterStateX(valueX2)).MustHaveHappenedOnceExactly());

    A.CallTo(() => onEnterStateZ()).MustHaveHappenedOnceExactly();
  }

  #endregion

  #region Group 7: Edge Cases

  [Test]
  public void PropagatedNullArgument_IsPassedToNullableReferenceTypeState()
  {
    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string?>>(); // Expects string?
    var onEnterStateY = A.Fake<Action<string?>>(); // Expects string?

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX) // Attaches a null argument
           .AddTransition(GoToY, StateY);

    builder.DefineState(StateY)
           .OnEnter(onEnterStateY);

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise<string?>(GoToX, null); // StateX gets null
    stateMachine.Raise(GoToY);                // Transition to StateY, should propagate null

    // --assert
    A.CallTo(() => onEnterStateY(null)).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void SelfTransition_Propagates_Argument()
  {
    // --arrange
    var          builder       = new Builder<string, int>(OnException);
    var          onEnterStateX = A.Fake<Action<string>>();
    const string value1        = "value1";

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX)        // StateX expects string
           .AddTransition(GoToX, StateX); // Self-transition

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, value1); // Enter StateX with "value1"
    stateMachine.Raise(GoToX);         // Self-transition, should propagate "value1"

    // --assert
    // OnEnter should be called twice with the same propagated value
    A.CallTo(() => onEnterStateX(value1)).MustHaveHappenedTwiceExactly();
  }

  [Test]
  public void SelfTransition_WithExplicitArgument_Overrides_CurrentArgument()
  {
    const string value1 = "value1";
    const string value2 = "value2";

    // --arrange
    var builder       = new Builder<string, int>(OnException);
    var onEnterStateX = A.Fake<Action<string>>();

    builder.DefineState(Initial)
           .AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(onEnterStateX)        // StateX expects string
           .AddTransition(GoToX, StateX); // Self-transition

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(GoToX, value1); // Enter StateX with "value1"
    stateMachine.Raise(GoToX, value2); // Self-transition with explicit "value2"

    // --assert
    // Check calls in order or by count
    A.CallTo(() => onEnterStateX(value1))
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onEnterStateX(value2)).MustHaveHappenedOnceExactly());
  }

  #endregion
}