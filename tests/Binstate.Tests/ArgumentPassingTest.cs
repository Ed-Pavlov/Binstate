using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
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
     .AddTransition(Event1, State1);

    builder
     .DefineState(State1)
     .OnEnter<string>((sm, param) => actual = param);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, Event1, expected);

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

    builder.DefineState(Initial).AddTransition(Event1, State1);
    builder.DefineState(State1).OnEnter<IDisposable>(onEnter);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, Event1, expected);

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
    builder.DefineState(Root).OnEnter<IDisposable>(onEnterRoot);
    builder.DefineState(Parent).AsSubstateOf(Root).OnEnter(sm => { });
    builder.DefineState(Child).AsSubstateOf(Parent).OnEnter<Stream>(onEnterChild);

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
     .AddTransition(Event1, State1);

    builder
     .DefineState(State1)
     .OnEnter<string>((sm, value) => { });

    var stateMachine = builder.Build(Initial);

    // --act
    Action target = () => stateMachine.Raise(raiseWay, Event1, 983);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage($"The state '{State1}' requires argument of type '{typeof(string)}' but no argument*");
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_throw_exception_if_no_argument_specified_for_enter_action_with_argument(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder
     .DefineState(Initial)
     .AddTransition(Event1, State1);

    builder
     .DefineState(State1)
     .OnEnter<int>(value => { });

    var stateMachine = builder.Build(Initial);

    // --act
    Action target = () => stateMachine.Raise(raiseWay, Event1);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage($"The state '{State1}' requires argument of type '{typeof(int)}' but no argument*");
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_throw_exception_if_parent_and_child_state_has_not_assignable_arguments_enable_loose_relaying_is_true_and_argument_is_passed(RaiseWay way)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);


    builder.DefineState(Initial).AddTransition(Event1, Child);

    builder.DefineState(Parent)
           .OnEnter<int>((stateMachine, value) => { });

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<string>(value => { });

    // --act
    var sm = builder.Build(Initial, true);

    Action target = () => sm.Raise(Event1, "stringArgument");

    // --assert
    target
     .Should()
     .Throw<TransitionException>()
     .WithMessage($"The state '{Parent}' requires argument of type '{typeof(int)}' but no argument*");
  }
}