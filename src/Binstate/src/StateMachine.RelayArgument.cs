using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    private sealed class Relayer<TRelay> : IStateMachine<TState, TEvent>
    {
      private readonly StateMachine<TState, TEvent> _owner;

      internal Relayer(StateMachine<TState, TEvent> owner) => _owner = owner;

      /// <inheritdoc />
      public bool Raise([NotNull] TEvent @event)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionSync<Unit, TRelay>(@event, null);
      }

      /// <inheritdoc />
      public bool Raise<T>([NotNull] TEvent @event, [CanBeNull] T argument)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionSync<T, TRelay>(@event, argument);
      }

      /// <inheritdoc />
      public Task<bool> RaiseAsync([NotNull] TEvent @event)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionAsync<Unit, TRelay>(@event, default);
      }

      /// <inheritdoc />
      public Task<bool> RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionAsync<T, TRelay>(@event, argument);
      }
    }
  }
}