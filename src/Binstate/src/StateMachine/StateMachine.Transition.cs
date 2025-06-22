using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal partial class StateMachine<TState, TEvent>
{
  /// <summary>
  /// Performing transition is split into two parts.
  /// This is the first one, it's "readonly", preparing and checking all the data, can throw an exception.
  /// </summary>
  /// <returns>
  /// Returns null if:
  /// no transition found by specified <paramref name="event" /> from the current state
  /// dynamic transition returns 'null'
  /// </returns>
  /// <exception cref="TransitionException"> Throws if <paramref name="eventArgument"/> doesn't match the argument required by the target state. </exception>
  private TransitionData? PrepareTransition<TEventArgument>(TEvent @event, TEventArgument eventArgument, bool argumentIsFallback)
  {
    try
    {
      _lock.WaitOne(); // if all goes well, it will be set after PerformTransition finished

      if(! _activeState.FindTransitionTransitive(@event, out var transition)) // looks for a transition through all parent states
      {
        // no transition by specified event is found
        _lock.Set();
        return null;
      }

      var transitionArgumentsProviders =
        transition.IsStatic
          ? Argument.NoTransitionArguments
          : PrepareArgumentsForTransition(transition, eventArgument, _activeState);

      if(! transition.GetTargetStateId(_activeState.Id, transitionArgumentsProviders, out var stateId))
      {
        _lock.Set(); // dynamic transition canceled it
        return null;
      }

      var targetState    = GetStateById(stateId);
      var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);

      var argumentsBag = PrepareArgument(eventArgument, argumentIsFallback, targetState, commonAncestor, _activeState);


      return new TransitionData(_activeState, transition, transitionArgumentsProviders, targetState, commonAncestor, argumentsBag);
    }
    catch(TransitionException)
    {
      _lock.Set();

      throw;
    }
    catch(Exception exception)
    {
      _onException(exception);
      _lock.Set();

      return null;
    }
  }

  /// <summary>
  /// Performs changes in the state machine state. Doesn't throw any exceptions, exceptions from the user code, 'enter' and 'exit' actions are translated
  /// into the delegate passed to <see cref="Builder{TState,TEvent}(Action{System.Exception}, Builder{TState,TEvent}.Options)" />
  /// </summary>
  private bool PerformTransition(TransitionData transitionData)
  {
    var currentActiveState = transitionData.CurrentActiveState;
    var prevActiveState    = transitionData.CurrentActiveState;
    var transition         = transitionData.Transition;
    var targetState        = transitionData.TargetState;
    var commonAncestor     = transitionData.CommonAncestor;
    var argumentsBag       = transitionData.ArgumentsBag;

    if(transition.IsReentrant)
      return true; // no need to call any actions on reentrant transitions

    var enterActions = new List<Action>();

    try
    {
      try
      {
        // exit all active states which are not parent for the new state
        while(currentActiveState != commonAncestor)
        {
          if(currentActiveState is null) throw Paranoia.GetException("currentActiveState can't become null earlier then be equal to commonAncestor");

          currentActiveState.ExitSafe(_onException);
          currentActiveState = currentActiveState.ParentState;
        }

        // invoke action attached to the transition itself
//        transition.CallActionSafe(transitionData.CurrentActiveState, transitionData.TargetState, transitionArgumentsProviders);
        prevActiveState.CallTransitionActionSafe(transition, _onException);

        // and then activate new active states
        _activeState = targetState;

        var state = targetState;
        while(state != commonAncestor)
        {
          if(state is null) throw Paranoia.GetException($"{nameof(state)} can't become null earlier then be equal to {nameof(commonAncestor)}");

          var setArgumentAction = argumentsBag.GetValueSafe(state);
          var enterAction       = CreateActivateStateNotGuardedAction(state, setArgumentAction);
          enterActions.Add(enterAction);
          state = state.ParentState;
        }
      }
      finally // no exception should be thrown here, but paranoia is my life
      {
        _lock.Set();
      }

      // call 'enter' actions out of the lock due to it can block execution
      CallEnterActionsInReverseOrder(enterActions);
    }
    catch(Exception exception)
    {
      _onException(exception);
    }

    return true; // just to reduce the amount of a caller method code
  }

  private static Argument.Bag PrepareArgument<TArgument>(
    TArgument argument,
    bool      argumentIsFallback,
    IState    targetState,
    IState?   commonAncestor,
    IState    sourceState)
  {
    var argumentResolver = new Argument.Resolver();

    var state = targetState;
    while(state != commonAncestor)
    {
      if(state is null) throw Paranoia.GetException("it can't be null before it is equal to commonAncestor");

      argumentResolver.PrepareArgumentForState(state, argument, argumentIsFallback, sourceState);
      state = state.ParentState;
    }

    return argumentResolver.ArgumentsBag;
  }

  private static void CallEnterActionsInReverseOrder(List<Action> enterActions)
  { // call them in reverse order, parent's 'enter' is called first, child's one last
    for(var i = enterActions.Count - 1; i >= 0; i--)
      enterActions[i]();
  }

  /// <summary>
  /// Doesn't acquire lock itself, caller should care about safe context
  /// </summary>
  /// <returns> Returns an action which should be called to call 'enter' action of the state </returns>
  private Action CreateActivateStateNotGuardedAction(IState<TState, TEvent> state, Action<IState>? setArgument)
  {
    state.IsActive = true; // set is as active inside the lock, see implementation of State class for details

    var controller = new Controller(this, state);

    // prepare 'enter' action - will be called later
    return () =>
    {
      // set the Argument property of the state if Argument is required, do it BEFORE calling EnterSafe, due to it uses this property
      setArgument?.Invoke(state);
      state.EnterSafe(controller, _onException);
    };
  }

  private readonly struct TransitionData
  {
    public readonly IState                                        CurrentActiveState;
    public readonly ITransition                                   Transition;
    public readonly Tuple<IArgumentProvider?, IArgumentProvider?> TransitionArgumentsProviders;
    public readonly IState<TState, TEvent>                        TargetState;
    public readonly IState?                                       CommonAncestor;

    public readonly IReadOnlyDictionary<IState, Action<IState>> ArgumentsBag;

    public TransitionData(
      IState                                        currentActiveState,
      ITransition<TState, TEvent>                   transition,
      Tuple<IArgumentProvider?, IArgumentProvider?> transitionArgumentsProviders,
      IState<TState, TEvent>                        targetState,
      IState?                                       commonAncestor,
      Argument.Bag                                  argumentsBag)
    {
      CurrentActiveState           = currentActiveState;
      Transition                   = transition;
      TransitionArgumentsProviders = transitionArgumentsProviders;
      TargetState                  = targetState;
      ArgumentsBag                 = argumentsBag;
      CommonAncestor               = commonAncestor;
    }
  }

  internal class Transition<TStateArgument, TEventArgument> : ITransition<TState, TEvent>
  {
    private static readonly Tuple<Type?, Type?> EmptyArgumentTypes = new(null, null);

    private static readonly Type? StateArgumentType = typeof(TStateArgument) == typeof(Unit) ? null : typeof(TStateArgument);
    private static readonly Type? EventArgumentType = typeof(TEventArgument) == typeof(Unit) ? null : typeof(TEventArgument);

    private static readonly Tuple<Type?, Type?> _argumentTypes
      = StateArgumentType == null && EventArgumentType == null
          ? EmptyArgumentTypes
          : ArgumentsTuple.Create(StateArgumentType, EventArgumentType);

    private readonly Maybe<TState> _targetStateId;

    public Transition(
      TEvent                  @event,
      TState                  targetStateId,
      Action<TEventArgument>? action,
      bool                    isReentrant = false)
      : this(@event, targetStateId, null, ActionToTransitionAction(action))
      => IsReentrant = isReentrant;

    public Transition(
      TEvent                                                                      @event,
      TState                                                                      targetStateId,
      Binstate.Transition<TStateArgument, TEventArgument>.Guard?                  guard, //TODO: is it bad, that in StateMachine the type Builder... is used?
      Binstate.Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>? action)
      : this(@event, ConvertGuardToSelector(targetStateId, guard), action)
    {
      _targetStateId   = targetStateId.ToMaybe();
      IsReentrant      = false;
      TransitionAction = action;
      IsStatic         = guard is null;
    }

    public Transition(
      TEvent                                                                            @event,
      Binstate.Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> stateSelector,
      Binstate.Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>?       transitionAction)
    {
      Event            = @event;
      _stateSelector   = stateSelector;
      TransitionAction = transitionAction;
    }

    public TEvent Event { get; }

    public Tuple<Type?, Type?> ArgumentTypes => _argumentTypes;

    private readonly Binstate.Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> _stateSelector;

    public TState GetTargetStateId()
    {
      if(! IsStatic) throw new InvalidOperationException("This method can be called only for static transitions");
      return _targetStateId.Value;
    }

    public bool GetTargetStateId(
      TState                                        sourceState,
      Tuple<IArgumentProvider?, IArgumentProvider?> argumentProviders,
      [NotNullWhen(true)] out TState?               targetStateId)
    {
      switch(IsStatic)
      {
        case true:
          targetStateId = _targetStateId.Value;
          return true;

        default:
        {
          var arguments = CreateArguments(sourceState, default, argumentProviders);
          var context   = new Binstate.Transition<TStateArgument, TEventArgument>.Context<TState, TEvent>(Event, sourceState, default, arguments);
          return _stateSelector(context, out targetStateId);
        }
      }
    }

    public void CallActionSafe(TState sourceState, TState targetState, Tuple<IArgumentProvider?, IArgumentProvider?> transitionArgumentsProviders)
    {
      var arguments = CreateArguments(sourceState, targetState, transitionArgumentsProviders);
      var context   = new Binstate.Transition<TStateArgument, TEventArgument>.Context<TState, TEvent>(Event, sourceState, targetState, arguments);

      //TODO: call action
    }

    private ITuple<TStateArgument, TEventArgument> CreateArguments(
      TState                                        sourceState,
      TState?                                       targetState,
      Tuple<IArgumentProvider?, IArgumentProvider?> argumentProviders)
    {
      var stateArgument = GetArgument<TStateArgument>(argumentProviders.ItemX);
      if(! stateArgument.HasValue)
        throw TransitionException.NoStateArgumentException<TEvent, TState, TEventArgument>(Event, sourceState, targetState);

      var eventArgument = GetArgument<TEventArgument>(argumentProviders.ItemY);
      if(! eventArgument.HasValue)
        throw TransitionException.NoEventArgumentException<TEvent, TState, TEventArgument>(Event, sourceState, targetState);

      return ArgumentsTuple.Create(stateArgument.Value, eventArgument.Value);
    }

    private static Maybe<T> GetArgument<T>(IArgumentProvider? argumentProvider)
      => typeof(TStateArgument) == typeof(Unit) // no argument is required
           ? new Maybe<T>(default!)
           : argumentProvider is null // argument required, but not provided
             ? Maybe<T>.Nothing
             : ( (IGetArgument<T>)argumentProvider ).Argument.ToMaybe();

    public bool IsStatic { get; }

    public object? TransitionAction { get; }
    public bool    IsReentrant      { get; }

    private static Binstate.Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>? ActionToTransitionAction(Action<TEventArgument>? action)
      => action is null ? null : _ => action(_.Arguments.ItemY);

    private static Binstate.Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> ConvertGuardToSelector(
      TState                                                     targetStateId,
      Binstate.Transition<TStateArgument, TEventArgument>.Guard? guard)
    {
      if(guard is null) return Empty;

      return (
          Binstate.Transition<TStateArgument, TEventArgument>.Context<TState, TEvent> context,
          [NotNullWhen(true)]out TState?                                              state)
        =>
      {
        if(guard(context.Arguments))
        {
          state = targetStateId;
          return true;
        }

        state = default;
        return false;
      };

    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
      var stateName = IsStatic ? _targetStateId.HasValue.ToString() : "dynamic";
      return $"[{Event} -> {stateName}]";
    }

    private static readonly Binstate.Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> Empty
      = (Binstate.Transition<TStateArgument, TEventArgument>.Context<TState, TEvent> _, [NotNullWhen(true)]out TState? state) =>
      {
        state = default;
        return false;
      };
  }
}