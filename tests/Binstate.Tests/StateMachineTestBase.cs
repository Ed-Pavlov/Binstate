using System;
using NUnit.Framework;

namespace Instate.Tests
{
  public abstract class StateMachineTestBase
  {
    protected const string Initial = "Initial";
    protected const int Event1 = 1;
    protected const string State1 = "state1";
    protected const string State2 = "state2";
    protected const string Root = "Root";
    protected const string Parent = "Parent";
    protected const string Child = "Child";
    protected const string Terminated = "Terminated";
    protected const int Terminate = 2;
    
    protected const string OnEnter = "OnEnter";
    protected const string OnExit = "OnExit";

    protected void OnException(Exception exception) => Assert.Fail(exception.Message);
  }
}