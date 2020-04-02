using System;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine
  {
    private bool IsControllerInState(object state) => Equals(state, _currentControllerState);
    
    private class Controller : IStateMachine
    {
      private readonly object _state;
      private readonly StateMachine _stateMachine;

      public Controller([NotNull] object state, [NotNull] StateMachine stateMachine)
      {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
      }

      public void Fire(object trigger)
      {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        _stateMachine.Fire(trigger);
      }

      public void Fire<T>(object trigger, [CanBeNull] T parameter)
      {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        _stateMachine.Fire(trigger, parameter);
      }

      public bool InMyState => _stateMachine.IsControllerInState(_state);
    }
  }
}