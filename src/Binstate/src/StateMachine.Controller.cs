using System;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine
  {
    private bool IsControllerInState(object state) => Equals(state, _currentControllerState);
    
    private class Controller : IStateMachine
    {
      private readonly object _stateId;
      private readonly StateMachine _stateMachine;

      public Controller([NotNull] object stateId, [NotNull] StateMachine stateMachine)
      {
        _stateId = stateId ?? throw new ArgumentNullException(nameof(stateId));
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
      }

      public void RaiseAsync([NotNull] object @event)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        _stateMachine.RaiseAsync(@event);
      }

      public void RaiseAsync<T>([NotNull] object @event, [CanBeNull] T parameter)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        _stateMachine.RaiseAsync(@event, parameter);
      }

      public bool InMyState => _stateMachine.IsControllerInState(_stateId);
    }
  }
}