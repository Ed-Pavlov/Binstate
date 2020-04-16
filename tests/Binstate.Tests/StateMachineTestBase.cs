namespace Instate.Tests
{
  public abstract class StateMachineTestBase
  {
    protected const string Initial = "Initial";
    protected const int Event1 = 1;
    protected const string State1 = "state1";
    protected const string Terminated = "Terminated";
    protected const int Terminate = 2;
    
    protected const string OnEnter = "OnEnter";
    protected const string OnExit = "OnExit";
  }
}