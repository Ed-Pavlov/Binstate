using System;
using System.Collections.Generic;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class EnterExitActionsTest : StateMachineTestBase
{
  [Test]
  public void should_call_enter_of_initial_state()
  {
    var entered = false;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial)
           .OnEnter(() => entered = true)
           .AddTransition(GoToStateX, () => null);

    // --act
    builder.Build(Initial);

    // --assert
    entered.Should().BeTrue();
  }

  [Test]
  public void should_call_enter_of_initial_state_with_argument()
  {
    const string expected = "expectedValue";
    var          actual   = expected + "bad";

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial)
           .OnEnter<string>(arg => actual = arg)
           .AddTransition(GoToStateX, () => null);

    // --act
    builder.Build(Initial, expected);

    // --assert
    actual.Should().Be(expected);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_enter_on_activation(RaiseWay raiseWay)
  {
    var actual = new List<string>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial)
           .AddTransition(GoToStateX, StateX);

    builder.DefineState(StateX)
           .OnEnter(_ => actual.Add(StateX));

    var stateMachine = builder.Build(Initial);

    // --act
    stateMachine.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeEquivalentTo(StateX);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_finish_enter_before_call_exit_and_call_next_enter(RaiseWay raiseWay)
  {
    var actual = new List<string>();

    const string enter1 = nameof(enter1);
    const string exit1  = nameof(exit1);
    const string enter2 = nameof(enter2);

    // --arrange
    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

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
     .AddTransition(GoToStateY, StateY);

    builder.DefineState(StateY)
           .OnEnter(_ => actual.Add(enter2));

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToStateX);

    // --act
    target.Raise(raiseWay, GoToStateY);

    // --assert
    actual.Should().BeEquivalentTo(enter1, exit1, enter2);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_exit_and_enter_on_reentering(RaiseWay raiseWay)
  {
    const string enter = nameof(enter);
    const string exit  = nameof(exit);

    var actual = new List<string>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter(_ => actual.Add(enter))
     .OnExit(() => actual.Add(exit))
     .AllowReentrancy(GoToStateX);

    var target = builder.Build(Initial);
    target.Raise(raiseWay, GoToStateX);

    // --act
    target.Raise(raiseWay, GoToStateX);

    // --assert
    actual.Should().BeEquivalentTo(enter, exit, enter);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_argument_to_exit_action(RaiseWay raiseWay)
  {
    const int expected = 398;

    var onExit = A.Fake<Action<int>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(Final);

    builder
     .DefineState(StateX)
     .OnEnter<int>(_ => { })
     .OnExit(onExit)
     .AddTransition(GoToStateX, Final);

    var target = builder.Build(Initial);
    target.Raise(GoToStateX, expected);

    // --act
    target.Raise(raiseWay, GoToStateX); // exit State1

    // --assert
    A.CallTo(() => onExit(expected)).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_call_on_exit_wo_argument_if_specified(RaiseWay raiseWay)
  {
    var onExit = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, StateX);
    builder.DefineState(Final);

    builder
     .DefineState(StateX)
     .OnEnter<int>(_ => { })
     .OnExit(onExit)
     .AddTransition(GoToStateX, Final);

    var target = builder.Build(Initial);
    target.Raise(GoToStateX, 3987);

    // --act
    target.Raise(raiseWay, GoToStateX); // exit State1

    // --assert
    A.CallTo(() => onExit()).MustHaveHappenedOnceExactly();
  }
}