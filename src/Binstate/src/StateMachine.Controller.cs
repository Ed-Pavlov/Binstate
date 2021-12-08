using System;
using System.Threading.Tasks;

namespace Binstate;

public partial class StateMachine<TState, TEvent>
{
  private class Controller : IStateController<TEvent>
  {
    private readonly StateMachine<TState, TEvent> _owner;
    private readonly IState                       _state;

    internal Controller(IState state, StateMachine<TState, TEvent> stateMachine)
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
  }
}