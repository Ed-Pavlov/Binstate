using System.Collections.Generic;

namespace Binstate;

public partial class Builder<TState, TEvent>
{
  internal class StateData
  {
    public readonly TState                                         StateId;
    public readonly Dictionary<TEvent, Transition<TState, TEvent>> TransitionList = new();

    public IStateFactory Factory       = new StateFactory<Unit>();
    public Maybe<TState> ParentStateId = Maybe<TState>.Nothing;
    public object?       EnterAction;
    public object?       ExitAction;

    public StateData(TState stateId) => StateId = stateId;

    public IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState) => Factory.CreateState(this, parentState);
  }

  /// <summary>
  /// There are two types of the state in the system with and w/o Argument. To make a code type safe and avoid
  /// boxing of value type arguments, the <see cref="State{TState, TEvent, TArguments}" /> class has TArgument generic argument.
  /// What type will be used depends on the state Enter, Exit, and Transition actions configuration and becomes known during runtime.
  /// If no arguments required the type <see cref="Unit"/> is used as a TArgument, and it is treated by the implementation as "no argument required".
  /// </summary>
  internal interface IStateFactory
  {
    IState<TState, TEvent> CreateState(StateData stateData, IState<TState, TEvent>? parentState);
  }

  private class StateFactory<TArgument> : IStateFactory
  {
    public IState<TState, TEvent> CreateState(StateData stateData, IState<TState, TEvent>? parentState)
      => new State<TState, TEvent, TArgument>(
        stateData.StateId,
        stateData.EnterAction,
        stateData.ExitAction,
        stateData.TransitionList,
        parentState
      );
  }

}