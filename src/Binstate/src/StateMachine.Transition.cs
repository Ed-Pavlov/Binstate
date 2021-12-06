using System;
using System.Collections.Generic;

namespace Binstate;

public partial class StateMachine<TState, TEvent>
{
  /// <summary>
  ///   Performing transition is split into two parts, the first one is "read only", preparing and checking all the data, can throw an exception.
  /// </summary>
  /// <returns>
  ///   Returns null if:
  ///   no transition found by specified <paramref name="event" /> from the current state
  ///   dynamic transition returns 'null'
  /// </returns>
  /// <exception cref="TransitionException"> Throws if passed argument doesn't match the 'enter' action of the target state. </exception>
  private TransitionData? PrepareTransition<TArgument>(TEvent @event, TArgument argument, bool argumentHasPriority)
  {
    try
    {
      _lock.WaitOne();

      if(! _activeState.FindTransitionTransitive(@event, out var transition) // looks for a transition through all parent states
      || ! transition!.GetTargetStateId(out var stateId))
      {
        // no transition by specified event is found or dynamic transition returns null as target state id
        _lock.Set();

        return null;
      }

      var targetState = GetStateById(stateId!);

      var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);
      var argumentsBag = PrepareArguments(argument, argumentHasPriority, targetState, commonAncestor, _activeState);

      return new TransitionData(_activeState, transition, targetState, argumentsBag, commonAncestor);
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

  private static ArgumentsBag PrepareArguments<TArgument>(
    TArgument               argument,
    bool                    argumentHasPriority,
    IState<TState, TEvent>  targetState,
    IState<TState, TEvent>? commonAncestor,
    IState<TState, TEvent>  sourceState)
  {
    var argumentWithCache = new Argument.WithCache();

    var map = new ArgumentsBag();

    var argumentTarget = targetState;
    while(argumentTarget != commonAncestor)
    {
      if(argumentTarget is null) throw new InvalidOperationException("It can't be null before it is equal to commonAncestor");
      var argumentTargetCopy = argumentTarget; // anti-closure copy

      var argumentType = argumentTarget.GetArgumentTypeSafe();
      if(argumentType is not null)
      {
        var passedArgumentIsSuitable = argumentType.IsAssignableFrom(typeof(TArgument));
        if(argumentHasPriority && passedArgumentIsSuitable)
          map.Add(argumentTarget, () => Argument.SetArgument(argumentTargetCopy, argument));
        else
        {
          if(argumentWithCache.GetArgumentSource(sourceState, argumentType, out var argumentSource))
            map.Add(argumentTarget, () => Argument.SetArgumentByReflection(argumentSource, argumentTargetCopy));
          else if(passedArgumentIsSuitable) // suitable but has no priority
            map.Add(argumentTarget, () => Argument.SetArgument(argumentTargetCopy, argument));
          else
            Throw.NoArgument(argumentTarget, argumentType);
        }
      }

      argumentTarget = argumentTarget.ParentState;
    }

    return map;
  }

  /// <summary>
  ///   Performs changes in the state machine state. Doesn't throw any exceptions, exceptions from the user code, 'enter' and 'exit' actions are translated
  ///   into the delegate passed to <see cref="Builder{TState,TEvent}(System.Action{System.Exception})" />
  /// </summary>

  // [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "foreach is more readable here")]
  private bool PerformTransition(TransitionData transitionData)
  {
    var currentActiveState = transitionData.CurrentActiveState;
    var prevActiveState    = currentActiveState;
    var transition         = transitionData.Transition;
    var targetState        = transitionData.TargetState;
    var argumentsBag       = transitionData.ArgumentsBag;
    var commonAncestor     = transitionData.CommonAncestor;

    var enterActions = new List<Action>();

    try
    {
      // exit all active states which are not parent for the new state
      while(currentActiveState != commonAncestor)
      {
        currentActiveState!.ExitSafe(_onException); // currentActiveState can't become null earlier then be equal to commonAncestor
        currentActiveState = currentActiveState.ParentState;
      }

      // invoke action attached to the transition itself
      prevActiveState.CallTransitionActionSafe(transition, _onException);

      // and then activate new active states
      _activeState = targetState;

      while(targetState != commonAncestor)
      {
        var enterAction = ActivateStateNotGuarded(targetState!, argumentsBag); // targetState can't become null earlier then be equal to commonAncestor
        enterActions.Add(enterAction);
        targetState = targetState!.ParentState;
      }
    }
    finally // no exception should be thrown here, but paranoia is my life
    {
      _lock.Set();
    }

    // call 'enter' actions out of the lock due to it can block execution
    // call them in reverse order, parent's 'enter' is called first, child's one last
    for(var i = enterActions.Count - 1; i >= 0; i--)
      enterActions[i]();

    return true; // just to reduce amount of code calling this method
  }

  /// <summary>
  ///   Doesn't acquire lock itself, caller should care about safe context
  /// </summary>
  /// <returns> Returns 'enter' action of the state </returns>
  private Action ActivateStateNotGuarded(IState<TState, TEvent> state, ArgumentsBag argumentsBag)
  {
    state.IsActive = true; // set is as active inside the lock, see implementation of State class for details
    var controller = new Controller(state, this);
    return () =>
    {
      argumentsBag[state].Invoke(); // set the Argument property of the state
      state.EnterSafe(controller, _onException);
    };
  }

  private static Tuple<TArgument, TRelay> CreateTuple<TArgument, TRelay>(MixOf<TArgument, TRelay> mixOf)
    => new Tuple<TArgument, TRelay>(mixOf.PassedArgument.Value, mixOf.RelayedArgument.Value);

  private static MixOf<TArgument, TRelay> PrepareRealArgument<TArgument, TRelay>(
    TArgument               argument,
    IState<TState, TEvent>? sourceState,
    Maybe<TRelay>           backupRelayArgument)
  {
    if(! Argument.IsSpecified<TRelay>()) // no relaying argument
      return Argument.IsSpecified<TArgument>() ? new MixOf<TArgument, TRelay>(argument.ToMaybe(), Maybe<TRelay>.Nothing) : MixOf<TArgument, TRelay>.Empty;

    var state = sourceState;

    while(state != null)
    {
      if(state is State<TState, TEvent, TRelay> stateWithArgument)
        return Argument.IsSpecified<TArgument>()
                 ? stateWithArgument.CreateTuple(argument)
                 : new MixOf<TArgument, TRelay>(Maybe<TArgument>.Nothing, stateWithArgument.Argument.ToMaybe());

      state = state.ParentState;
    }

    if(backupRelayArgument.HasValue)
      return Argument.IsSpecified<TArgument>()
               ? new MixOf<TArgument, TRelay>(argument.ToMaybe(),       backupRelayArgument)
               : new MixOf<TArgument, TRelay>(Maybe<TArgument>.Nothing, backupRelayArgument);

    throw new TransitionException(
      "Raise with relaying argument is called from the state w/o an attached value and a backup argument for relay is not provided"
    );
  }

  private readonly struct TransitionData
  {
    public readonly IState<TState, TEvent>     CurrentActiveState;
    public readonly Transition<TState, TEvent> Transition;
    public readonly IState<TState, TEvent>     TargetState;
    public readonly ArgumentsBag               ArgumentsBag;
    public readonly IState<TState, TEvent>?    CommonAncestor;

    public TransitionData(
      IState<TState, TEvent>     currentActiveState,
      Transition<TState, TEvent> transition,
      IState<TState, TEvent>     targetState,
      ArgumentsBag               argumentsBag,
      IState<TState, TEvent>?    commonAncestor)
    {
      CurrentActiveState = currentActiveState;
      Transition         = transition;
      TargetState        = targetState;
      ArgumentsBag       = argumentsBag;
      CommonAncestor     = commonAncestor;
    }
  }

  private class ArgumentsBag : Dictionary<IState, Action> { }
}