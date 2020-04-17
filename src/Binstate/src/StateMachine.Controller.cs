using System;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    private class Controller : IStateMachine<TEvent>
    {
      private readonly State<TState, TEvent> _state;
      private readonly StateMachine<TState, TEvent> _owner;

      internal Controller(State<TState, TEvent> state, StateMachine<TState, TEvent> stateMachine)
      {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _owner = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
      }

      public bool InMyState => _state.IsActive;

      public void RaiseAsync([NotNull] TEvent @event)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        _owner.RaiseAsync(@event);
      }

      public void RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T parameter)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        _owner.RaiseAsync(@event, parameter);
      }
    }
  }
}