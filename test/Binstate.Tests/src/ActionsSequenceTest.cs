using System;
using System.Collections.Generic;
using System.Threading;
using BeatyBit.Binstate;
using Binstate.Tests.Util;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class EnterExitActionsTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  public void should_finish_enter_before_call_exit_and_call_next_enter(RaiseWay raiseWay)
  {
    var actual = new List<string>();

    const string enter1 = nameof(enter1);
    const string exit1  = nameof(exit1);
    const string enter2 = nameof(enter2);

    // --arrange
    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter(
        _ =>
        {
          Thread.Sleep(299);
          actual.Add(enter1);
        }
      )
     .OnExit(
        () =>
        {
          Thread.Sleep(382);
          actual.Add(exit1);
        }
      )
     .AddTransition(GoToY, StateY);

    builder.DefineState(StateY)
           .OnEnter(_ => actual.Add(enter2));

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToX);

    // --act
    target.Raise(raiseWay, GoToY);

    // --assert
    actual.Should().BeEquivalentTo(enter1, exit1, enter2);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_not_call_exit_and_enter_on_reentering_define_with_allow_reentrancy(RaiseWay raiseWay)
  {
    const string enter = nameof(enter);
    const string exit  = nameof(exit);

    var actual = new List<string>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter(_ => actual.Add(enter))
     .OnExit(() => actual.Add(exit))
     .AllowReentrancy(GoToX);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToX);

    // --act
    var result = target.Raise(raiseWay, GoToX);

    // --assert
    actual.Should().BeEquivalentTo(enter);
    result.Should().BeTrue();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_exit_and_enter_on_reentering(RaiseWay raiseWay)
  {
    const string enter = nameof(enter);
    const string exit  = nameof(exit);

    var actual = new List<string>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter(_ => actual.Add(enter))
     .OnExit(() => actual.Add(exit))
     .AddTransition(GoToX, StateX);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToX);

    // --act
    target.Raise(raiseWay, GoToX);

    // --assert
    actual.Should().BeEquivalentTo(enter, exit, enter);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_exit_and_transition_in_order(RaiseWay raiseWay)
  {
    var onEnter      = A.Fake<Action>();
    var onExit       = A.Fake<Action>();
    var onTransition = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AddTransition(GoToX, StateX);
    builder.DefineState(StateY);
    builder.DefineState(StateX)
           .OnEnter(onEnter)
           .OnExit(onExit)
           .AddTransitionSimple(GoToY, StateY, onTransition);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToX);
    target.Raise(raiseWay, GoToY);

    // --assert
    A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => onExit()).MustHaveHappenedOnceExactly())
     .Then(A.CallTo(() => onTransition()).MustHaveHappenedOnceExactly());
  }
}