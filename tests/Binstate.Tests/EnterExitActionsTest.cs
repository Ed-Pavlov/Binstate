using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests
{
  public class EnterActionTest : StateMachineTestBase
  {
    [Test]
    public void should_call_enter_of_initial_state()
    {
      var entered = false;
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial)
        .OnEnter(() => entered = true).AddTransition(Event1, () => null);
      
      // --act
      builder.Build(Initial);
      
      // --assert
      entered.Should().BeTrue();
    }
    
    [TestCaseSource(nameof(RaiseWays))]
    public void should_call_enter_on_activation(RaiseWay raiseWay)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial)
        .AddTransition(Event1, State1);

      builder.DefineState(State1)
        .OnEnter(_ => actual.Add(State1));

      var stateMachine = builder.Build(Initial);

      // --act
      stateMachine.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(State1);
    }

    [Test]
    public void should_not_accept_async_void_enter_action()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      var state = builder.DefineState(State1);
      
      // --act
      Action action = () => state.OnEnter(AsyncVoidMethod);
      
      // --assert
      action.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    }

    [Test]
    public void should_not_accept_async_void_simple_enter_action()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      var state = builder.DefineState(State1);
      
      // --act
      Action action = () => state.OnEnter(SimpleAsyncVoidMethod);
      
      // --assert
      action.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    }

    [Test]
    public void should_throw_exception_if_initial_state_enter_requires_argument()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial).OnEnter<int>(_ => throw new InvalidOperationException()).AddTransition(Event1, () => null);
      
      // --act
      Action target = () => builder.Build(Initial);
      
      // --assert
      target.Should().ThrowExactly<TransitionException>().WithMessage("The enter action of the initial state must not require argument.");
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_finish_enter_before_call_exit_and_call_next_enter(RaiseWay raiseWay)
    {
      var actual = new List<string>();

      const string enter1 = nameof(enter1);
      const string exit1 = nameof(exit1);
      const string enter2 = nameof(enter2);
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter(_ =>
        {
          Thread.Sleep(299);
          actual.Add(enter1);
        })
        .OnExit(() =>
        {
          Thread.Sleep(382);
          actual.Add(exit1);
        }) 
        .AddTransition(Event2, State2);

      builder.DefineState(State2)
        .OnEnter(_ => actual.Add(enter2));

      var target = builder.Build(Initial);
      target.Raise(raiseWay, Event1);
      
      // --act
      target.Raise(raiseWay, Event2);
      
      // --assert
      actual.Should().BeEquivalentTo(enter1, exit1, enter2);
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_call_exit_and_enter_on_reentering(RaiseWay raiseWay)
    {
      const string enter = nameof(enter);
      const string exit = nameof(exit);
      
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      
      builder
        .DefineState(State1)
        .OnEnter(_ => actual.Add(enter))
        .OnExit(() => actual.Add(exit))
        .AllowReentrancy(Event1);

      var target = builder.Build(Initial);
      target.Raise(raiseWay, Event1);

      // --act
      target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(enter, exit, enter);
    }
    
#pragma warning disable CS1998
    private static async void AsyncVoidMethod(IStateMachine<int> _)
    {
    }

    private static async void SimpleAsyncVoidMethod()
    {
    }
#pragma warning restore
  }
}