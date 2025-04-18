using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatyBit.Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class MultiThreadingTest : StateMachineTestBase
{
  [Test]
  public void eternal_async_enter_should_be_stopped_by_changing_state()
  {
    const string enter = nameof(enter);
    const string exit  = nameof(exit);

    // --arrange
    var actual  = new List<string>();
    var entered = new ManualResetEvent(false);

    void BlockingEnter(IStateController<int> machine)
    {
      entered.Set();
      while(machine.InMyState) Thread.Sleep(100);
      actual.Add(enter);
    }

    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToX, StateX);

    builder.DefineState(StateX)
           .OnEnter(BlockingEnter)
           .OnExit(() => actual.Add(exit))
           .AddTransition(GoToY, StateY);

    builder
     .DefineState(StateY)
     .OnEnter(_ => actual.Add(StateY));

    var target = builder.Build(Initial);

    target.RaiseAsync(GoToX); // raise async to not block test execution
    entered.WaitOne(1000);         // wait till OnEnter will block execution

    // --act
    target.Raise(GoToY);

    // -- assert
    actual.Should().Equal(enter, exit, StateY);
  }

  [TestCaseSource(nameof(RaiseWays))]
  [Timeout(5000)]
  public void async_enter_should_not_block(RaiseWay raiseWay)
  {
    // --arrange
    var entered = new ManualResetEvent(false);

    async Task AsyncEnter(IStateController<int> stateMachine)
    {
      entered.Set();
      while(stateMachine.InMyState) await Task.Delay(546);
    }

    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AddTransition(GoToX, StateX);

    builder
     .DefineState(StateX)
     .OnEnter(AsyncEnter)
     .AddTransition(GoToY, StateY);

    builder.DefineState(StateY);

    var target = builder.Build(Initial);

    // --act
    target.Raise(raiseWay, GoToX);

    // --assert
    entered.WaitOne(TimeSpan.FromSeconds(4)).Should().BeTrue();

    // --cleanup
    target.Raise(GoToY); // exit async method
  }
}