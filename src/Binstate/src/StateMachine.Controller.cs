using System;
using System.Threading.Tasks;
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

      public Task RaiseAsync([NotNull] object @event)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        return _stateMachine.RaiseAsync(@event);
      }

      public Task RaiseAsync<T>([NotNull] object @event, [CanBeNull] T parameter)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        return _stateMachine.RaiseAsync(@event, parameter);
      }

      public bool InMyState => _stateMachine.IsControllerInState(_state);
    }
  }
}