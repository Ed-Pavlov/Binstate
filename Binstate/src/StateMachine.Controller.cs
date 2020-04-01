namespace Binstate
{
  public partial class StateMachine
  {
    private class Controller : IStateMachine
    {
      private readonly object _state;
      private readonly StateMachine _stateMachine;

      public Controller(object state, StateMachine stateMachine)
      {
        _state = state;
        _stateMachine = stateMachine;
      }

      public void Fire(object trigger) => _stateMachine.Fire(trigger);
      public void Fire<T>(object trigger, T parameter) => _stateMachine.Fire(trigger, parameter);

      public bool InMyState => _stateMachine.IsInStateInternal(_state);
    }
  }
}