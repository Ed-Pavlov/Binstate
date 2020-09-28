﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    private sealed class Relayer<TRelay> : IStateMachine<TState, TEvent>
    {
      private readonly StateMachine<TState, TEvent> _owner;
      private readonly Maybe<TRelay> _backupValue;

      internal Relayer(StateMachine<TState, TEvent> owner, Maybe<TRelay> backupValue)
      {
        _owner = owner;
        _backupValue = backupValue;
      }

      /// <inheritdoc />
      public bool Raise([NotNull] TEvent @event)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionSync<Unit, TRelay>(@event, null, _backupValue);
      }

      /// <inheritdoc />
      public bool Raise<T>([NotNull] TEvent @event, [CanBeNull] T argument)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionSync(@event, argument, _backupValue);
      }

      /// <inheritdoc />
      public Task<bool> RaiseAsync([NotNull] TEvent @event)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionAsync<Unit, TRelay>(@event, default, _backupValue);
      }

      /// <inheritdoc />
      public Task<bool> RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        return _owner.PerformTransitionAsync(@event, argument, _backupValue);
      }
    }
  }
}