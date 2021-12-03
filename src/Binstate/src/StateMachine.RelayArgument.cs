using System;
using System.Threading.Tasks;

namespace Binstate;

public partial class StateMachine<TState, TEvent>
{
  private sealed class Relayer<TRelay> : IStateMachine<TState, TEvent>
  {
    private readonly StateMachine<TState, TEvent> _owner;
    private readonly Maybe<TRelay>                _backupValue;

    internal Relayer(StateMachine<TState, TEvent> owner, Maybe<TRelay> backupValue)
    {
      _owner       = owner;
      _backupValue = backupValue;
    }

    /// <inheritdoc />
    public bool Raise(TEvent @event)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));

      return _owner.PerformTransitionSync(@event, Unit.Default, _backupValue);
    }

    /// <inheritdoc />
    public bool Raise<T>(TEvent @event, T argument)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));

      return _owner.PerformTransitionSync(@event, argument, _backupValue);
    }

    /// <inheritdoc />
    public Task<bool> RaiseAsync(TEvent @event)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));

      return _owner.PerformTransitionAsync<Unit, TRelay>(@event, default, _backupValue);
    }

    /// <inheritdoc />
    public Task<bool> RaiseAsync<T>(TEvent @event, T argument)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));

      return _owner.PerformTransitionAsync(@event, argument, _backupValue);
    }
  }
}