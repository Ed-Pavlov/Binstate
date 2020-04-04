namespace Instate.Tests
{
  public abstract class StateMachineTestBase
  {
    protected const string initial = "Initial";
    protected const string event1 = "event1";
    protected const string state1 = "state1";
    protected const string terminated = "Terminated";
    protected const string terminate = "terminate";
    
    protected const string OnEnter = "OnEnter";
    protected const string OnExit = "OnExit";
  }
}