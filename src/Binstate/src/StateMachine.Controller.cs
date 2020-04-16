using System;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    private bool IsControllerInState(TState state) => Equals(state, _currentControllerState);
    
    private class Controller : IStateMachine<TEvent>
    {
      private readonly TState _stateId;
      private readonly StateMachine<TState, TEvent> _stateMachine;

      public Controller([NotNull] TState stateId, [NotNull] StateMachine<TState, TEvent> stateMachine)
      {
        _stateId = stateId ?? throw new ArgumentNullException(nameof(stateId));
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
      }

      public void RaiseAsync([NotNull] TEvent @event)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        _stateMachine.RaiseAsync(@event);
      }

      public void RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T parameter)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        _stateMachine.RaiseAsync(@event, parameter);
      }

      public bool InMyState => _stateMachine.IsControllerInState(_stateId);
    }
  }
}