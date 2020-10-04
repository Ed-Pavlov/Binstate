using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests
{
  public class MultiThreadingTest : StateMachineTestBase
  {
    [Test]
    public void eternal_async_enter_should_be_stopped_by_changing_state()
    {
      const string enter = nameof(enter);
      const string exit = nameof(exit);
      
      // --arrange
      var actual = new List<string>();
      var entered = new ManualResetEvent(false);
      
      void BlockingEnter(IStateMachine<int> machine)
      {
        entered.Set();
        while (machine.InMyState) Thread.Sleep(100);
        actual.Add(enter);
      }
      
      var builder = new Builder<string, int>(OnException);
      
      builder.DefineState(Initial).AddTransition(Event1, State1);

      builder.DefineState(State1)
        .OnEnter(BlockingEnter)
        .OnExit(() => actual.Add(exit))
        .AddTransition(Event2, State2);

      builder
        .DefineState(State2)
        .OnEnter(_ => actual.Add(State2));
      
      var target = builder.Build(Initial);

      target.RaiseAsync(Event1); // raise async to not to block test execution
      entered.WaitOne(); // wait till OnEnter will block execution
      
      // --act
      target.Raise(Event2);

      // -- assert
      actual.Should().Equal(enter, exit, State2);
    }

    [TestCaseSource(nameof(RaiseWays)), Timeout(5000)]
    public void async_enter_should_not_block(RaiseWay raiseWay)
    {
      // --arrange
      var entered = new ManualResetEvent(false);
      
      async Task AsyncEnter(IStateMachine<int> stateMachine)
      {
        entered.Set();
        while (stateMachine.InMyState) await Task.Delay(546);
      }
      
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter(AsyncEnter)
        .AddTransition(Event2, State2);

      builder.DefineState(State2);
      
      var target = builder.Build(Initial);
      
      // --act
      target.Raise(raiseWay, Event1);

      // --assert
      entered.WaitOne(TimeSpan.FromSeconds(4)).Should().BeTrue();
      
      // --cleanup
      target.Raise(Event2); // exit async method
    }
  }
}