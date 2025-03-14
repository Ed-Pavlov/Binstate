using System.Collections.Generic;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  internal class StateConfig
  {
    public readonly TState                                         StateId;
    public readonly Dictionary<TEvent, Transition<TState, TEvent>> TransitionList = new();

    public IStateFactory Factory       = new StateFactory<Unit>();
    public Maybe<TState> ParentStateId = Maybe<TState>.Nothing;
    public object?       EnterAction;
    public object?       ExitAction;

    public StateConfig(TState stateId) => StateId = stateId;

    public IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState) => Factory.CreateState(this, parentState);
  }
}