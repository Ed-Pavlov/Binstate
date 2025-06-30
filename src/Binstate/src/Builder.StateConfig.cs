using System;
using System.Collections.Generic;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  internal abstract class StateConfig
  {
    protected readonly Dictionary<TEvent, ITransition<TState, TEvent>> _transitionList = new();

    protected StateConfig(TState stateId) => StateId  = stateId;

    public TState StateId { get; }

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

    public abstract IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState);
  }

  /// <summary>
  /// The <see cref="Builder{TState,TEvent}"/> accumulates all data about a state machine's state in this class during configuration
  /// and then uses it to create an implementation of <see cref="IState{TState, TEvent}"/> for use in <see cref="StateMachine{TState,TEvent}"/>.
  /// </summary>
  internal class StateConfig<TStateArgument> : StateConfig
  {
    public StateConfig(TState stateId) :base(stateId){}

    public State<TState, TEvent, TStateArgument>.EnterAction? EnterAction
    {
      get;

      set
      {
        if(field is not null) throw Paranoia.GetException("code written in the way that property is set only once.");
        field = value;
      }
    }

    public State<TState, TEvent, TStateArgument>.ExitAction? ExitAction
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

    public void AddTransition<TEventArgument>(
      TEvent                                                             @event,
      TState                                                             targetStateId,
      Transition<TStateArgument, TEventArgument>.Guard?                  guard,
      Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>? action)
      => _transitionList.Add(@event, new StateMachine<TState, TEvent>.Transition<TStateArgument, TEventArgument>(@event, targetStateId, guard, action));

    public void AddTransition<TEventArgument>(
      TEvent                                                                   @event,
      Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> selectState,
      Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>?       action)
      => _transitionList.Add(@event, new StateMachine<TState, TEvent>.Transition<TStateArgument, TEventArgument>(@event, selectState, action));

    public override IState<TState, TEvent> CreateState(IState<TState, TEvent>? parentState)
      => new State<TState, TEvent, TStateArgument>(
        StateId,
        EnterAction,
        ExitAction,
        TransitionList,
        parentState
      );
  }
}