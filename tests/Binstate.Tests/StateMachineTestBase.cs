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

    protected static IEnumerable<TestCaseData> raise_and_raise_async_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Func<StateMachine<string, int>, int, bool>((_, e) => _.Raise(e))).SetName("Raise");
      yield return new TestCaseData(new Func<StateMachine<string, int>, int, bool>((_, e) => _.RaiseAsync(e).Result)).SetName("RaiseAsync");
    }
  }
}