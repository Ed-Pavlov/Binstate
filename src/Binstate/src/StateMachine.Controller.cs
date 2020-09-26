using System;
using System.Threading.Tasks;
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

      public bool RaiseAsync([NotNull] TEvent @event)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return RaiseAsync<Unit>(@event, default);
      }

      public bool RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        var data = _owner.PrepareTransition<T>(@event, default);
        if (data == null)
          return false;
        
        Task.Run(() => _owner.PerformTransition(data.Value));
        return true;
      }
    }
  }
}