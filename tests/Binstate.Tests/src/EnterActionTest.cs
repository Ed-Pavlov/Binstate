using System;
using System.Text;
using BeatyBit.Binstate;
using Binstate.Tests.Util;
using FakeItEasy;
using NUnit.Framework;

namespace Binstate.Tests;

public class EnterActionTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_action(RaiseWay raiseWay)
  {
    var onEnter = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_action_wo_argument_if_argument_passed(RaiseWay raiseWay)
  {
    var onEnter = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToStateX, "argument");

    // --assert
    A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_action_w_argument(RaiseWay raiseWay)
  {
    const string expected = "argument";
    var          onEnter  = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToStateX, expected);

    // --assert
    A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_action_w_argument_from_prev_active_state(RaiseWay raiseWay)
  {
    const string expected = "argument";
    var          onEnter1 = A.Fake<Action<string>>();
    var          onEnter2 = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter1).AddTransition(GoToStateY, StateY);
    builder.DefineState(StateY).OnEnter(onEnter2);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToStateX, expected);

    // --act
    target.Raise(raiseWay, GoToStateY);

    // --assert
    A.CallTo(() => onEnter2(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_parent_enter_action_w_argument(RaiseWay raiseWay)
  {
    const string expected      = "argument";
    var          onEnterParent = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Parent).OnEnter(onEnterParent);
    builder.DefineState(Child).AsSubstateOf(Parent); // Child is w/o Enter and argument
    builder.DefineState(Initial).AddTransition(GoToChild, Child);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToChild, expected);

    // --assert
    A.CallTo(() => onEnterParent(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_parent_enter_action_w_argument_from_active_state(RaiseWay raiseWay)
  {
    const string expected      = "argument";
    var          onEnterParent = A.Fake<Action<string>>();
    var          onEnter1      = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Parent).OnEnter(onEnterParent);
    builder.DefineState(Child).AsSubstateOf(Parent); // Child is w/o Enter and argument
    builder.DefineState(StateX).OnEnter(onEnter1).AddTransition(GoToChild, Child);
    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToStateX, expected);

    // --act
    target.Raise(raiseWay, GoToChild);

    // --assert
    A.CallTo(() => onEnterParent(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_action_w_argument_but_not_from_prev_active_state(RaiseWay raiseWay)
  {
    const string expected = "argument";
    var          onEnter1 = A.Fake<Action<string>>();
    var          onEnter2 = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter1).AddTransition(GoToStateY, StateY);
    builder.DefineState(StateY).OnEnter(onEnter2);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToStateX, expected + "bad");

    // --act
    target.Raise(raiseWay, GoToStateY, expected);

    // --assert
    A.CallTo(() => onEnter2(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_action_w_argument_from_prev_active_state_but_not_passed(RaiseWay raiseWay)
  {
    const string expected = "argument";
    var          onEnter1 = A.Fake<Action<string>>();
    var          onEnter2 = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter1).AddTransition(GoToStateY, StateY);
    builder.DefineState(StateY).OnEnter(onEnter2);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToStateX, expected);

    // --act
    target.Raise(raiseWay, GoToStateY, expected + "bad", true);

    // --assert
    A.CallTo(() => onEnter2(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_to_enter_two_arguments_one_from_active_state_and_other_passed_to_raise(RaiseWay raiseWay)
  {
    const string expectedString        = "argument";
    var          expectedStringBuilder = new StringBuilder("expected");
    var          onEnter               = A.Fake<Action<StringBuilder, string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).OnEnter<string>(_ => { }).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial, expectedString);

    // --act
    target.Raise(raiseWay, GoToStateX, expectedStringBuilder);

    // --assert
    A.CallTo(() => onEnter(expectedStringBuilder, expectedString)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  [Description("When Enter action requires two arguments get one passed to Raise and another from active state")]
  public void should_pass_to_enter_tuple_mixed_from_passed_and_argument_from_active_state(RaiseWay raiseWay)
  {
    const string expectedString        = "argument";
    var          expectedStringBuilder = new StringBuilder("expected");
    var          onEnter               = A.Fake<Action<ITuple<StringBuilder, string>>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).OnEnter<string>(_ => { }).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial, expectedString);

    // --act
    target.Raise(raiseWay, GoToStateX, expectedStringBuilder);

    // --assert
    A.CallTo(() => onEnter(new BeatyBit.Binstate.Tuple<StringBuilder, string>(expectedStringBuilder, expectedString))).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_to_enter_two_arguments_from_active_state(RaiseWay raiseWay)
  {
    const string expectedString        = "argument";
    var          expectedStringBuilder = new StringBuilder("expected");
    var          onEnter               = A.Fake<Action<StringBuilder, string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException, new Builder.Options{ArgumentTransferMode = ArgumentTransferMode.Free});

    builder.DefineState(Initial).OnEnter<string>(_ => { }).AddTransition(GoToStateY, StateY);
    builder.DefineState(StateY).AsSubstateOf(Initial).OnEnter<StringBuilder>(_ => { }).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnEnter(onEnter);

    var target = builder.Build(Initial, expectedString);
    target.Raise(raiseWay, GoToStateY, expectedStringBuilder);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    A.CallTo(() => onEnter(expectedStringBuilder, expectedString)).MustHaveHappenedOnceAndOnly();
  }
}