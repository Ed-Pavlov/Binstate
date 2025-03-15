using System.Collections.Generic;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  /// <summary>
  /// The <see cref="Builder{TState,TEvent}"/> accumulates all data about a state machine's state in this class during configuration
  /// and then uses it to create an implementation of <see cref="IState{TState, TEvent}"/> for use in <see cref="StateMachine{TState,TEvent}"/>.
  /// </summary>
  internal class StateData
  {
    // paranoia mode on
    private Maybe<TState>? _parentStateId;
    private Maybe<object?> _enterAction;
    private Maybe<object?> _exitAction;

    public StateData(TState stateId) => StateId = stateId;

    public readonly TState                                         StateId;
    public readonly Dictionary<TEvent, Transition<TState, TEvent>> TransitionList = new();

    public Maybe<TState> ParentStateId
    {
      get => _parentStateId ?? Maybe<TState>.Nothing;

      set
      {
        if(_parentStateId is not null) Throw.ParanoiaException("code written in the way that property is set only once.");
        _parentStateId = value;
      }
    }

    public object? EnterAction
    {
      get => _enterAction.GetValueSafe();

      set
      {
        if(_enterAction.HasValue) Throw.ParanoiaException("code written in the way that property is set only once.");
        _enterAction = value.ToMaybe();
      }
    }

    public object? ExitAction
    {
      get => _exitAction.GetValueSafe();

      set
      {
        if(_exitAction.HasValue) Throw.ParanoiaException("code written in the way that property is set only once.");
        _exitAction = value.ToMaybe();
      }
    }

    public IStateFactory Factory { private get; set; } = new StateFactory<Unit>();

    public IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState) => Factory.CreateState(this, parentState);
  }

  /// <summary>
  /// There are two types of the state in the system with and w/o Argument. To make a code type safe and avoid
  /// boxing of value type arguments, the <see cref="State{TState, TEvent, TArguments}" /> class has TArgument generic argument.
  /// What type will be used depends on the state 'enter', 'exit', and 'transition' actions configuration and becomes known during calling
  /// <see cref="Builder{TState,TEvent}.Build{T}"/> method.
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