using System;
using System.Threading.Tasks;

namespace Binstate;

public partial class StateMachine<TState, TEvent> where TState : notnull where TEvent : notnull
{
  private class Controller : IStateMachine<TEvent>
  {
    private readonly StateMachine<TState, TEvent> _owner;
    private readonly IState<TState, TEvent>       _state;

    internal Controller(IState<TState, TEvent> state, StateMachine<TState, TEvent> stateMachine)
    {
      _state = state        ?? throw new ArgumentNullException(nameof(state));
      _owner = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
    }

    public bool InMyState => _state.IsActive;

    public bool RaiseAsync(TEvent @event) => RaiseAsync<Unit>(@event, default);

    public bool RaiseAsync<T>(TEvent @event, T argument) => RaiseAsyncInternal(@event, argument, Maybe<Unit>.Nothing);

    public IAutoTransition<TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true) => new ControllerRelayer<TRelay?>(
      this, relayArgumentIsRequired ? Maybe<TRelay?>.Nothing : default(TRelay).ToMaybe()
    );

    /// <summary>
    ///   Implementation shared between <see cref="Controller" /> itself and <see cref="ControllerRelayer{TRelay}" />
    /// </summary>
    private bool RaiseAsyncInternal<T, TRelay>(TEvent @event, T? argument, Maybe<TRelay> backupValue)
    {
      var data = _owner.PrepareTransition(@event, argument, backupValue);

      if(data is null) return false;

      Task.Run(() => _owner.PerformTransition(data.Value));

      return true;
    }

    private class ControllerRelayer<TRelay> : IAutoTransition<TEvent>
    {
      private readonly Maybe<TRelay> _backupValue;
      private readonly Controller    _owner;

      public ControllerRelayer(Controller owner, Maybe<TRelay> backupValue)
      {
        _owner       = owner;
        _backupValue = backupValue;
      }

      public bool RaiseAsync(TEvent @event) => RaiseAsync<Unit>(@event, default);

      public bool RaiseAsync<T>(TEvent @event, T argument)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));

        return _owner.RaiseAsyncInternal(@event, argument, _backupValue);
      }
    }
  }
}