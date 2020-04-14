namespace Instate.Tests
{
  public abstract class StateMachineTestBase
  {
    protected const string Initial = "Initial";
    protected const string Event1 = "event1";
    protected const string State1 = "state1";
    protected const string Terminated = "Terminated";
    protected const string Terminate = "terminate";
    
    protected const string OnEnter = "OnEnter";
    protected const string OnExit = "OnExit";
  }
}