using System;
using System.Collections.Generic;
using System.IO;
using BeatyBit.Binstate;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class TransitionTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_action_on_transition_between_exit_and_enter(RaiseWay raiseWay)
  {
    var onExitInitial = A.Fake<Action>();
    var onTransit     = A.Fake<Action>();
    var onEnterState1 = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial)
           .OnExit(onExitInitial)
           .AddTransition(GoToStateX, StateX, onTransit);

    builder.DefineState(StateX)
           .OnEnter(onEnterState1);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    A.CallTo(() => onExitInitial()).MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onTransit()).MustHaveHappenedOnceExactly())
     .Then(A.CallTo(() => onEnterState1()).MustHaveHappenedOnceExactly());
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_action_w_argument_on_transition_between_exit_and_enter(RaiseWay raiseWay)
  {
    var expected = new MemoryStream();

    var onExitInitial = A.Fake<Action>();
    var onTransit     = A.Fake<Action<IDisposable>>();
    var onEnterState1 = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial)
           .OnEnter<IDisposable>(_ => { })
           .OnExit(onExitInitial)
           .AddTransition(GoToStateX, StateX, onTransit);

    builder.DefineState(StateX)
           .OnEnter(onEnterState1);

    var target = builder.Build(Initial, expected);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    A.CallTo(() => onExitInitial())
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onTransit(expected)).MustHaveHappenedOnceExactly())
     .Then(A.CallTo(() => onEnterState1()).MustHaveHappenedOnceExactly());
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void raise_should_return_false_if_no_transition_found(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);
    builder.DefineState(Initial).AddTransition(StateX, StateX);
    builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

    var target = builder.Build(Initial);

    // --act
    var actual = target.Raise(raiseWay, "WrongEvent");

    // --assert
    actual.Should().BeFalse();
  }

  [Test]
  public void controller_should_return_false_if_no_transition_found()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);

    builder.DefineState(Initial)
           .OnEnter(OnEnterInitialState)
           .AddTransition(StateX, StateX);

    builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

    builder.Build(Initial);

    static void OnEnterInitialState(IStateController<string> stateMachine)
    {
      // --act
      var actual = stateMachine.RaiseAsync("WrongEvent");

      // --assert
      actual.Should().BeFalse();
    }
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_transit_using_dynamic_transition(RaiseWay raiseWay)
  {
    var actual = new List<string>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    var first = true;

    builder
     .DefineState(Initial)
     .AddTransition(
        GoToStateX,
        () =>
        {
          var state = first ? StateX : StateY;
          first = false;

          return state;
        }
      );

    builder
     .DefineState(StateX)
     .AsSubstateOf(Initial)
     .OnEnter(_ => actual.Add(StateX));

    builder
     .DefineState(StateY)
     .OnEnter(_ => actual.Add(StateY));

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToStateX);
    target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeEquivalentTo(StateX, StateY);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_transit_using_dynamic_transition_using_value_type_default(RaiseWay raiseWay)
  {
    const int initialStateId = 1;
    const int stateId1       = 0; // default value
    const int stateId2       = 38;

    var first = true;

    bool DynamicTransition(out int state)
    {
      state = first ? stateId1 : stateId2;
      first = false;

      return true;
    }

    var actual = new List<int>();

    // --arrange
    var builder = new Builder<int, int>(OnException, new Builder.Options{AllowDefaultValueAsStateId = true});

    builder
     .DefineState(initialStateId)
     .AddTransition(GoToStateX, DynamicTransition);

    builder
     .DefineState(stateId1)
     .AsSubstateOf(initialStateId)
     .OnEnter(_ => actual.Add(stateId1));

    builder
     .DefineState(stateId2)
     .OnEnter(_ => actual.Add(stateId2));

    var target = builder.Build(initialStateId);

    // --act
    target.Raise(raiseWay, GoToStateX);
    target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().Equal(stateId1, stateId2);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void raise_should_return_false_if_dynamic_transition_returns_false_value_type(RaiseWay raiseWay)
  {
    const int initialStateId = 1;
    const int stateId        = 2;

    static bool DynamicTransition(out int state)
    {
      state = stateId;
      return false;
    }

    // --arrange
    var builder = new Builder<int, int>(OnException);

    builder.DefineState(initialStateId)
           .AddTransition(GoToStateX, DynamicTransition);

    builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

    var target = builder.Build(initialStateId);

    // --act
    var actual = target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeFalse();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void raise_should_return_false_if_dynamic_transition_returns_false_reference_type(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    static bool DynamicTransition(out string stateId)
    {
      stateId = StateX;

      return false;
    }

    builder.DefineState(Initial)
           .AddTransition(GoToStateX, DynamicTransition);

    builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

    var target = builder.Build(Initial);

    // --act
    var actual = target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeFalse();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void raise_should_return_false_if_dynamic_transition_returns_null(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial)
           .AddTransition(GoToStateX, () => null);

    builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

    var target = builder.Build(Initial);

    // --act
    var actual = target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeFalse();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void raise_should_return_false_if_dynamic_transition_returns_default(RaiseWay raiseWay)
  {
    const int initialStateId = 1;
    const int stateId        = 2;

    // --arrange
    var builder = new Builder<int, int>(OnException);

    builder.DefineState(initialStateId)
           .AddTransition(GoToStateX, () => default!);

    builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

    var target = builder.Build(initialStateId);

    // --act
    var actual = target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeFalse();
  }

  [Test]
  public void controller_should_return_false_if_dynamic_transition_returns_null()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);

    builder.DefineState(Initial)
           .OnEnter(OnEnterInitialState)
           .AddTransition(StateX, () => null);

    builder.DefineState(StateX).OnEnter(() => Assert.Fail("No transition should be performed"));

    builder.Build(Initial);

    static void OnEnterInitialState(IStateController<string> stateMachine)
    {
      // --act
      var actual = stateMachine.RaiseAsync(StateX);

      // --assert
      actual.Should().BeFalse();
    }
  }

  [Test]
  public void controller_should_return_false_if_dynamic_transition_returns_default()
  {
    const int initialStateId = 1;
    const int stateId        = 2;

    // --arrange
    var builder = new Builder<int, int>(OnException);

    builder.DefineState(initialStateId)
           .OnEnter(OnEnterInitialState)
           .AddTransition(GoToStateX, () => default);

    builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

    builder.Build(initialStateId);

    static void OnEnterInitialState(IStateController<int> stateMachine)
    {
      // --act
      var actual = stateMachine.RaiseAsync(GoToStateX);

      // --assert
      actual.Should().BeFalse();
    }
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_use_parent_transition(RaiseWay raiseWay)
  {
    var actual = new List<string>();

    // --arrange
    var builder = new Builder<string, string>(OnException);

    builder.DefineState(Initial).AddTransition(Child, Child);

    builder.DefineState(Parent)
           .AddTransition(StateX, StateX, () => actual.Add(Parent));

    builder.DefineState(Child).AsSubstateOf(Parent);

    builder.DefineState(StateX)
           .OnEnter(_ => actual.Add(StateX));

    var target = builder.Build(Initial);
    target.Raise(raiseWay, Child);

    // --act
    target.Raise(raiseWay, StateX);

    // --assert
    actual.Should().Equal(Parent, StateX);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_catch_user_action_exception_and_report(RaiseWay raiseWay)
  {
    var onException = A.Fake<Action<Exception>>();

    // --arrange
    var builder = new Builder<string, string>(onException);

    builder.DefineState(Initial)
           .AddTransition(StateX, StateX, () => throw new TestException());

    builder.DefineState(StateX);

    var target = builder.Build(Initial);

    // --act
    var actual = target.Raise(raiseWay, StateX);

    // --assert
    actual.Should().BeTrue();
    A.CallTo(() => onException(An<Exception>.That.Matches(exc => exc is TestException))).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void should_throw_exception_if_transitions_to_different_states_by_one_event()
  {
    var builder = new Builder<string, int>(OnException);

    var config = builder
                .DefineState(Initial)
                .AddTransition(GoToStateX, StateX);

    // --act
    Action target = () => config.AddTransition(GoToStateX, StateY);

    // --assert
    target.Should().ThrowExactly<ArgumentException>().WithMessage("An item with the same key has already been added*");
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_not_perform_transition_if_dynamic_transition_throws_exception(RaiseWay raiseWay)
  {
    var onException = A.Fake<Action<Exception>>();

    // --arrange
    var builder = new Builder<string, int>(onException);

    builder
     .DefineState(Initial)
     .AddTransition(GoToStateX, () => throw new TestException());

    var target = builder.Build(Initial);

    // --act
    var result = target.Raise(raiseWay, GoToStateX);

    // --assert
    result.Should().BeFalse();
    A.CallTo(() => onException(An<Exception>.That.Matches(exc => exc is TestException))).MustHaveHappenedOnceExactly();
  }
}