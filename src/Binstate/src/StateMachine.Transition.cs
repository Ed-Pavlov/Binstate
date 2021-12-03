using System;
using System.Collections.Generic;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    /// <summary>
    /// Performing transition is split into two parts, the first one is "read only", preparing and checking all the data, can throw an exception. 
    /// </summary>
    /// <returns>Returns null if:
    /// no transition found by specified <paramref name="event"/> from the current state
    /// dynamic transition returns 'null'
    /// </returns>
    /// <exception cref="TransitionException">Throws if passed argument doesn't match the 'enter' action of the target state.</exception>
    private TransitionData<TArgument, TRelay>? PrepareTransition<TArgument, TRelay>(TEvent @event, TArgument argument, Maybe<TRelay> backupRelayArgument)
    {
      try
      {
        _lock.WaitOne();

        if(!_activeState!.FindTransitionTransitive(@event, out var transition) // looks for a transition through all parent states
        || !transition!.GetTargetStateId(out var stateId))
        {
          // no transition by specified event is found or dynamic transition returns null as target state id
          _lock.Set();

          return null;
        }

        var targetState = GetStateById(stateId!);

        var mixedArgument  = PrepareRealArgument(argument, _activeState, backupRelayArgument);
        var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);
        ValidateStates(_activeState, @event, targetState, mixedArgument, commonAncestor); // validate before changing any state of the state machine

        return new TransitionData<TArgument, TRelay>(_activeState, transition, targetState, mixedArgument, commonAncestor);
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
    /// into the delegate passed to <see cref="Builder{TState,TEvent}(Action{Exception})"/> 
    /// </summary>

    // [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "foreach is more readable here")]
    private bool PerformTransition<TArgument, TRelay>(TransitionData<TArgument, TRelay> transitionData)
    {
      var currentActiveState = transitionData.CurrentActiveState;
      var transition         = transitionData.Transition;
      var targetState        = transitionData.TargetState;
      var argument           = transitionData.Argument;
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
        transition.InvokeActionSafe(_onException);

        // and then activate new active states
        _activeState = targetState;

        while(targetState != commonAncestor) // targetState can't become null earlier then be equal to commonAncestor
        {
          var enterAction = ActivateStateNotGuarded(targetState!, argument);
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
    /// Doesn't acquire lock itself, caller should care about safe context 
    /// </summary>
    /// <returns>Returns 'enter' action of the state</returns>
    private Action ActivateStateNotGuarded<TArgument, TRelay>(State<TState, TEvent> state, MixOf<TArgument, TRelay> argument)
    {
      state.IsActive = true; // set is as active inside the lock, see implementation of State class for details

      var controller = new Controller(state, this);

      return state switch
             {
               IState<TState, TEvent, TArgument> passedArgumentState =>
                 () => passedArgumentState.EnterSafe(controller, argument.PassedArgument.Value, _onException),
               
               IState<TState, TEvent, TRelay> relayedArgumentState =>
                 () => relayedArgumentState.EnterSafe(controller, argument.RelayedArgument.Value, _onException),
               
               IState<TState, TEvent, ITuple<TArgument, TRelay>> bothArgumentsState =>
                 () => bothArgumentsState.EnterSafe(controller, CreateTuple(argument), _onException),
               
               _ => () => state.EnterSafe(controller, _onException) // no arguments state
             };
    }

    private static Tuple<TArgument, TRelay> CreateTuple<TArgument, TRelay>(MixOf<TArgument, TRelay> mixOf)
      => new(mixOf.PassedArgument.Value, mixOf.RelayedArgument.Value);

    private static MixOf<TArgument, TRelay> PrepareRealArgument<TArgument, TRelay>(
      TArgument              argument,
      State<TState, TEvent>? sourceState,
      Maybe<TRelay>          backupRelayArgument)
    {
      if(!Argument.IsSpecified<TRelay>()) // no relaying argument
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
                 ? new MixOf<TArgument, TRelay>(argument.ToMaybe(), backupRelayArgument)
                 : new MixOf<TArgument, TRelay>(Maybe<TArgument>.Nothing, backupRelayArgument);

      throw new TransitionException(
        "Raise with relaying argument is called from the state w/o an attached value and a backup argument for relay is not provided");
    }

    private readonly struct TransitionData<TArgument, TRelay>
    {
      public readonly State<TState, TEvent>      CurrentActiveState;
      public readonly Transition<TState, TEvent> Transition;
      public readonly State<TState, TEvent>      TargetState;
      public readonly MixOf<TArgument, TRelay>   Argument;
      public readonly State<TState, TEvent>?     CommonAncestor;

      public TransitionData(
        State<TState, TEvent>      currentActiveState,
        Transition<TState, TEvent> transition,
        State<TState, TEvent>      targetState,
        MixOf<TArgument, TRelay>   argument,
        State<TState, TEvent>?     commonAncestor)
      {
        CurrentActiveState = currentActiveState;
        Transition         = transition;
        TargetState        = targetState;
        Argument           = argument;
        CommonAncestor     = commonAncestor;
      }
    }
  }
}