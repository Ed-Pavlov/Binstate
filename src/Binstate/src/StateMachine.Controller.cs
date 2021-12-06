using System;
using System.Threading.Tasks;

namespace Binstate;

public partial class StateMachine<TState, TEvent> where TState : notnull where TEvent : notnull
{
  private class Controller : IStateController<TEvent>
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

    public bool RaiseAsync<T>(TEvent @event, T argument)
    {
      var data = _owner.PrepareTransition(@event, argument, true);
      if(data is null) return false;

      Task.Run(() => _owner.PerformTransition(data.Value));
      return true;
    }

    [Obsolete(
      "Since version 1.2 relaying arguments from the currently active states to states require them performs automatically."
    + "This method is not needed and adds nothing to the behaviour of the state machine."
    )]
    public IAutoTransition<TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true) => this;
  }
}