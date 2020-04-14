using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class MultiThreadingTest : StateMachineTestBase
  {
    [Test]
    public void blocking_enter_should_be_stopped_by_changing_state()
    {
      // --arrange
      var actual = new List<string>();
      var entered = new ManualResetEvent(false);
      
      void BlockingEnter(IStateMachine machine)
      {
        entered.Set();
        while (machine.InMyState) Thread.Sleep(100);
        actual.Add(OnEnter);
      }
      
      var builder = new Builder();
      
      builder
        .AddState(Initial)
        .AddTransition(Event1, State1);

      builder
        .AddState(State1)
        .OnEnter(BlockingEnter)
        .OnExit(() => actual.Add(OnExit))
        .AddTransition(Terminate, Terminated);

      builder
        .AddState(Terminated)
        .OnEnter(_ => actual.Add(Terminated));
      
      var target = builder.Build(Initial);

      target.RaiseAsync(Event1); // raise async to not to block test execution
      entered.WaitOne(); // wait till OnEnter will block execution
      
      // --act
      target.Raise(Terminate);

      // -- assert
      actual.Should().Equal(OnEnter, OnExit, Terminated);
    }

    [Test, Timeout(5000)]
    public void async_enter_should_not_block()
    {
      // --arrange
      var entered = new ManualResetEvent(false);
      
      async Task AsyncEnter(IStateMachine stateMachine)
      {
        entered.Set();
        while (stateMachine.InMyState) await Task.Delay(100);
      }
      
      var builder = new Builder();
      builder
        .AddState(Initial)
        .AddTransition(Event1, State1);

      builder
        .AddState(State1)
        .OnEnter(AsyncEnter)
        .AddTransition(Terminate, Terminated);

      builder
        .AddState(Terminated);
      
      var target = builder.Build(Initial);
      
      // --act
      target.Raise(Event1);
      

      // --assert
      entered.WaitOne(TimeSpan.FromSeconds(5)).Should().BeTrue();
      
      // --cleanup
      target.Raise(Terminate); // exit async method
    }
  }
}