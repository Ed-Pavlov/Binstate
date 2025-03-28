using System;
using System.Threading.Tasks;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal partial class StateMachine<TState, TEvent>
{
  private class Controller : IStateController<TEvent>
  {
    private readonly StateMachine<TState, TEvent> _stateMachine;
    private readonly IState                       _state;

    internal Controller(StateMachine<TState, TEvent> stateMachine, IState state)
    {
      _state        = state        ?? throw new ArgumentNullException(nameof(state));
      _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
    }

    /// <inheritdoc />
    public bool InMyState => _state.IsActive;

    /// <inheritdoc />
    public bool RaiseAsync(TEvent @event) => RaiseAsync<Unit>(@event, default);

    /// <inheritdoc />
    public bool RaiseAsync<T>(TEvent @event, T argument, bool argumentIsFallback = false)
    {
      var data = _stateMachine.PrepareTransition(@event, argument, argumentIsFallback);
      if(data is null) return false;

      Task.Run(() => _stateMachine.PerformTransition(data.Value));
      return true;
    }
  }
}