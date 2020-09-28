﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
    private TransitionData<MixOf<TArgument, TRelay>>? PrepareTransition<TArgument, TRelay>(TEvent @event, TArgument argument, Maybe<TRelay> backupRelayArgument)
    {
      try
      {
        _lock.WaitOne();

        if (!_activeState.FindTransitionTransitive(@event, out var transition)  // looks for a transition through all parent states
          || !transition.GetTargetStateId(out var stateId))
        { // no transition by specified event is found or dynamic transition returns null as target state id
          _lock.Set();
          return null;
        }

        var targetState = GetStateById(stateId);

        var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);
        var statesToEnter = targetState.GetAllStatesForActivationTillParent(commonAncestor); // get states from activeState with all parents till newState itself to activate
        var mixedArgument = PrepareRealArgument(argument, _activeState, backupRelayArgument);
        ValidateStates(statesToEnter, _activeState, @event, mixedArgument); // validate before changing any state of the state machine

        return new TransitionData<MixOf<TArgument, TRelay>>(_activeState, transition, targetState, statesToEnter, commonAncestor, mixedArgument);
      }
      catch (TransitionException)
      {
        _lock.Set();
        throw;
      }
      catch (Exception exception)
      {
        _onException(exception);
        _lock.Set();
        return null;
      }
    }

    /// <summary>
    /// Performs changes in the state machine state. Doesn't throw any exceptions, exceptions from the user code, 'enter' and 'exit' actions are translated
    /// into the delegate passed to <see cref="Builder{TState,TEvent}(Action{Exception}, bool)"/> 
    /// </summary>
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "foreach is more readable here")]
    private bool PerformTransition<TArgument, TRelay>(TransitionData<MixOf<TArgument, TRelay>> transitionData)
    {
      var currentActiveState = transitionData.CurrentActiveState;
      var transition = transitionData.Transition;
      var targetState = transitionData.TargetState;
      var statesToEnter = transitionData.StatesToEnter;
      var argument = transitionData.Argument;
      var commonAncestor = transitionData.CommonAncestor;

      var enterActions = new List<Action>();
      try
      {
        // exit all active states which are not parent for the new state
        while (currentActiveState != commonAncestor)
        {
          currentActiveState.ExitSafe(_onException);
          currentActiveState = currentActiveState.ParentState;
        }

        // invoke action attached to the transition itself
        transition.InvokeActionSafe(_onException);

        // and then activate new active states
        foreach (var state in statesToEnter)
        {
          var enterAction = ActivateStateNotGuarded(state, argument);
          enterActions.Add(enterAction);
        }

        _activeState = targetState;
      }
      finally // no exception should be thrown here, but paranoia is my life
      {
        _lock.Set();
      }

      // call Enter actions out of the lock due to it can block execution
      foreach (var enterAction in enterActions)
        enterAction();

      return true; // just to reduce amount of code calling this method
    }

    /// <summary>
    /// Doesn't acquire lock itself, caller should care about safe context 
    /// </summary>
    private Action ActivateStateNotGuarded<TArgument, TRelay>(State<TState, TEvent> state, MixOf<TArgument, TRelay> argument)
    {
      state.IsActive = true; // set is as active inside the lock, see implementation of State class for details
      var controller = new Controller(state, this);

      return state switch
      {
        IState<TState, TEvent, TArgument> passedArgumentState => () => passedArgumentState.EnterSafe(controller, argument.PassedArgument.Value, _onException),
        IState<TState, TEvent, TRelay> relayedArgumentState => () => relayedArgumentState.EnterSafe(controller, argument.RelayedArgument.Value, _onException),
        IState<TState, TEvent, ITuple<TArgument, TRelay>> bothArgumentsState => () => bothArgumentsState.EnterSafe(controller, argument.ToTupleUnsafe(), _onException),
        _ => () => state.EnterSafe(controller, _onException) // no arguments state
      };
    }

    private static MixOf<TArgument, TRelay> PrepareRealArgument<TArgument, TRelay>(TArgument argument, State<TState, TEvent> sourceState, Maybe<TRelay> backupRelayArgument)
    {
      if (!Argument.IsSpecified<TRelay>()) // no relaying argument
        return Argument.IsSpecified<TArgument>() ? new MixOf<TArgument, TRelay>(argument.ToMaybe(), Maybe<TRelay>.Nothing) : MixOf<TArgument, TRelay>.Empty;

      var state = sourceState;
      while (state != null)
      {
        if (state is State<TState, TEvent, TRelay> stateWithArgument)
          return Argument.IsSpecified<TArgument>() ? stateWithArgument.CreateTuple(argument) : new MixOf<TArgument, TRelay>(Maybe<TArgument>.Nothing, stateWithArgument.Argument.ToMaybe());

        state = state.ParentState;
      }
      
      if(backupRelayArgument.HasValue)
        return Argument.IsSpecified<TArgument>()
          ? new MixOf<TArgument, TRelay>(argument.ToMaybe(), backupRelayArgument) 
          : new MixOf<TArgument, TRelay>(Maybe<TArgument>.Nothing, backupRelayArgument);
      
      throw new TransitionException("propagating from the state w/o a state and a backup argument for relay");
    }

    private readonly struct TransitionData<T>
    {
      public readonly State<TState, TEvent> TargetState;
      public readonly State<TState, TEvent> CurrentActiveState;
      public readonly State<TState, TEvent> CommonAncestor;
      public readonly Transition<TState, TEvent> Transition;
      public readonly IEnumerable<State<TState, TEvent>> StatesToEnter;
      public readonly T Argument;

      public TransitionData(
        State<TState, TEvent> currentActiveState,
        Transition<TState, TEvent> transition,
        State<TState, TEvent> targetState,
        IEnumerable<State<TState, TEvent>> statesToEnter,
        State<TState, TEvent> commonAncestor,
        T argument)
      {
        TargetState = targetState;
        CurrentActiveState = currentActiveState;
        CommonAncestor = commonAncestor;
        Transition = transition;
        StatesToEnter = statesToEnter;
        Argument = argument;
      }
    }
  }
}