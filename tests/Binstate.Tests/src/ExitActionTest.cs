﻿using System;
using System.Text;
using Binstate.Tests.Util;
using FakeItEasy;
using NUnit.Framework;

namespace Binstate.Tests;

public class ExitActionTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_exit_action(RaiseWay raiseWay)
  {
    var onExit = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).OnExit(onExit).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    A.CallTo(() => onExit()).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_exit_action_wo_argument_if_enter_w_argument(RaiseWay raiseWay)
  {
    var onExit = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).OnEnter<string>(_ => { }).OnExit(onExit).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX);

    var target = builder.Build(Initial, "argh");

    // --act
    target.Raise(raiseWay, GoToStateX, "argument");

    // --assert
    A.CallTo(() => onExit()).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_exit_action_w_argument(RaiseWay raiseWay)
  {
    const string expected = "argument";
    var          onExit  = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).OnExit(onExit).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX);

    var target = builder.Build(Initial, expected);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    A.CallTo(() => onExit(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_exit_action_w_argument_from_prev_active_state(RaiseWay raiseWay)
  {
    const string expected = "argument";
    var          onExitInitial = A.Fake<Action<string>>();
    var          onExitX = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).OnExit(onExitInitial).AddTransition(GoToStateX, StateX);
    builder.DefineState(StateX).OnExit(onExitX).AddTransition(GoToStateY, StateY);
    builder.DefineState(StateY);

    var target = builder.Build(Initial, expected);
    target.Raise(raiseWay, GoToStateX);

    // --act
    target.Raise(raiseWay, GoToStateY);

    // --assert
    A.CallTo(() => onExitX(expected)).MustHaveHappenedOnceAndOnly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_parent_exit_action_w_argument(RaiseWay raiseWay)
  {
    const string expected      = "argument";
    var          onExitParent = A.Fake<Action<string>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Parent).OnExit(onExitParent);
    builder.DefineState(Initial).AsSubstateOf(Parent).AddTransition(GoToChild, Child);
    builder.DefineState(Child); // Child is w/o Exit and argument

    var target = builder.Build(Initial, expected);

    // --act
    target.Raise(raiseWay, GoToChild);

    // --assert
    A.CallTo(() => onExitParent(expected)).MustHaveHappenedOnceAndOnly();
  }
}