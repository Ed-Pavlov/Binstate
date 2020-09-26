using System;
using System.Collections.Generic;
using Binstate;
using NUnit.Framework;

namespace Instate.Tests
{
  public abstract class StateMachineTestBase
  {
    protected const string Initial = "Initial";
    protected const int Event1 = 1;
    protected const int Event2 = 2;
    protected const string State1 = "state1";
    protected const string State2 = "state2";
    protected const string Root = "Root";
    protected const string Parent = "Parent";
    protected const string Child = "Child";
    protected const string Terminated = "Terminated";
    protected const int Terminate = 8932;
    
    protected const string OnEnter = "OnEnter";
    protected const string OnExit = "OnExit";

    protected void OnException(Exception exception) => Assert.Fail(exception.Message);

    public delegate bool Raise<TState, TEvent>(StateMachine<TState, TEvent> stateMachine, TEvent @event);
    public delegate bool RaiseArgument<TState, TEvent>(StateMachine<TState, TEvent> stateMachine, TEvent @event, int argument);
    public delegate bool Propagate<TState, TEvent>(StateMachine<TState, TEvent> stateMachine, TEvent @event);
    
    protected static IEnumerable<TestCaseData> raise_and_raise_async_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Raise<string, int>((_, e) => _.Raise(e))).SetName("Raise");
      yield return new TestCaseData(new Raise<string, int>((_, e) => _.RaiseAsync(e).Result)).SetName("RaiseAsync");
    }
    
    protected static IEnumerable<TestCaseData> propagate_and_propagate_async_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Propagate<string, int>((_, e) => _.RaisePropagate<int>(e))).SetName("RaisePropagate");
      yield return new TestCaseData(new Propagate<string, int>((_, e) => _.RaisePropagateAsync<int>(e).Result)).SetName("RaisePropagateAsync");
    }
    
    protected static IEnumerable<TestCaseData> raise_and_raise_async_with_argument_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new RaiseArgument<string, int>((_, e, arg) => _.Raise(e, arg))).SetName("Raise");
      yield return new TestCaseData(new RaiseArgument<string, int>((_, e, arg) => _.RaiseAsync(e, arg).Result)).SetName("RaiseAsync");
    }
  }
}