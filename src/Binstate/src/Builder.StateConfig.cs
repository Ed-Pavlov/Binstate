using System;
using System.Collections.Generic;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  /// <summary>
  /// The <see cref="Builder{TState,TEvent}"/> accumulates all data about a state machine's state in this class during configuration
  /// and then uses it to create an implementation of <see cref="IState{TState, TEvent}"/> for use in <see cref="StateMachine{TState,TEvent}"/>.
  /// </summary>
  internal class StateConfig
  {
    private readonly IStateFactory                                   _factory;
    private readonly Dictionary<TEvent, ITransition<TState, TEvent>> _transitionList = new();

    public StateConfig(TState stateId, IStateFactory stateFactory)
    {
      StateId  = stateId;
      _factory = stateFactory;
    }

    public readonly TState StateId;

    public IReadOnlyDictionary<TEvent, ITransition<TState, TEvent>> TransitionList => _transitionList;

    public Maybe<TState> ParentStateId
    {
      get;

      set
      {
        if(field.HasValue) throw Paranoia.GetException("code written in the way that property is set only once.");
        field = value;
      }
    }

    public object? EnterAction
    {
      get;

      set
      {
        if(field is not null) throw Paranoia.GetException("code written in the way that property is set only once.");
        field = value;
      }
    }

    public object? ExitAction
    {
      get;

      set
      {
        if(field is not null) throw Paranoia.GetException("code written in the way that property is set only once.");
        field = value;
      }
    }

    public void AddReentrantTransition<TEventArgument>(TEvent @event, Action<TEventArgument>? action)
    {
      var transition = new StateMachine<TState, TEvent>.Transition<Unit, TEventArgument>(@event, StateId, action, true);
      _transitionList.Add(@event, transition);
    }

    public void AddTransition<TStateArgument, TEventArgument>(
      TEvent                                                             @event,
      TState                                                             targetStateId,
      Transition<TStateArgument, TEventArgument>.Guard?                  guard,
      Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>? action)
      => _transitionList.Add(@event, new StateMachine<TState, TEvent>.Transition<TStateArgument, TEventArgument>(@event, targetStateId, guard, action));

    public void AddTransition<TStateArgument, TEventArgument>(
      TEvent                                                                   @event,
      Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> selectState,
      Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>?       action)
      => _transitionList.Add(@event, new StateMachine<TState, TEvent>.Transition<TStateArgument, TEventArgument>(@event, selectState, action));

    public IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState) => _factory.CreateState(this, parentState);
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
    IState<TState, TEvent> CreateState(StateConfig stateConfig, IState<TState, TEvent>? parentState);
  }

  private class StateFactory<TArgument> : IStateFactory
  {
    public IState<TState, TEvent> CreateState(StateConfig stateConfig, IState<TState, TEvent>? parentState)
      => new State<TState, TEvent, TArgument>(
        stateConfig.StateId,
        stateConfig.EnterAction,
        stateConfig.ExitAction,
        stateConfig.TransitionList,
        parentState
      );
  }
}