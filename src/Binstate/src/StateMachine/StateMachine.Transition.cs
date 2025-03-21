﻿using System;
using System.Collections.Generic;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal partial class StateMachine<TState, TEvent>
{
  /// <summary>
  /// Performing transition is split into two parts, the first one is "readonly", preparing and checking all the data, can throw an exception.
  /// </summary>
  /// <returns>
  /// Returns null if:
  /// no transition found by specified <paramref name="event" /> from the current state
  /// dynamic transition returns 'null'
  /// </returns>
  /// <exception cref="TransitionException"> Throws if passed argument doesn't match the 'enter' action of the target state. </exception>
  private TransitionData? PrepareTransition<TArgument>(TEvent @event, TArgument argument, bool argumentIsFallback)
  {
    try
    {
      _lock.WaitOne(); // if all goes well, it will be set after PerformTransition finished

      if(! _activeState.FindTransitionTransitive(@event, out var transition) // looks for a transition through all parent states
      || ! transition.GetTargetStateId(out var stateId))
      {
        // no transition by specified event is found or dynamic transition cancelled it
        _lock.Set();

        return null;
      }

      var targetState = GetStateById(stateId);

      var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);
      var argumentsBag   = PrepareArgument(argument, argumentIsFallback, targetState, commonAncestor, _activeState);

      return new TransitionData(_activeState, transition, targetState, commonAncestor, argumentsBag);
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
      if(state is null) Throw.ParanoiaException("it can't be null before it is equal to commonAncestor");

      argumentResolver.PrepareArgumentForState(state, argument, argumentIsFallback, sourceState);
      state = state.ParentState;
    }

    return argumentResolver.ArgumentsBag;
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

    var enterActions = new List<Action>();

    try
    {
      try
      {
        // exit all active states which are not parent for the new state
        while(currentActiveState != commonAncestor)
        {
          if(currentActiveState is null)
            Throw.ParanoiaException("currentActiveState can't become null earlier then be equal to commonAncestor");

          currentActiveState.ExitSafe(_onException);
          currentActiveState = currentActiveState.ParentState;
        }

        // invoke action attached to the transition itself
        prevActiveState.CallTransitionActionSafe(transition, _onException);

        // and then activate new active states
        _activeState = targetState;

        while(targetState != commonAncestor)
        {
          if(targetState is null)
            Throw.ParanoiaException("targetState can't become null earlier then be equal to commonAncestor");

          var enterAction = ActivateStateNotGuarded(targetState, argumentsBag);
          enterActions.Add(enterAction);
          targetState = targetState.ParentState;
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

    return true; // just to reduce the amount of code calling this method
  }

  private static void CallEnterActionsInReverseOrder(List<Action> enterActions)
  { // call them in reverse order, parent's 'enter' is called first, child's one last
    for(var i = enterActions.Count - 1; i >= 0; i--)
      enterActions[i]();
  }

  /// <summary>
  /// Doesn't acquire lock itself, caller should care about safe context
  /// </summary>
  /// <returns> Returns 'enter' action of the state </returns>
  private Action ActivateStateNotGuarded(IState state, Argument.Bag argumentsBag, bool callEnterAction = true)
  {
    state.IsActive = true; // set is as active inside the lock, see implementation of State class for details

    var setArgument = argumentsBag.GetValueSafe(state);

    // prepare 'enter' action - will be called later
    return () =>
    {
      // set the Argument property of the state if Argument is required
      setArgument?.Invoke();

      // set Argument property before calling EnterSafe, due to it uses this property
      if(callEnterAction)
      {
        var controller = new Controller(state, this);
        state.EnterSafe(controller, _onException);
      }
    };
  }

  private readonly struct TransitionData
  {
    public readonly IState                 CurrentActiveState;
    public readonly ITransition            Transition;
    public readonly IState<TState, TEvent> TargetState;
    public readonly IState?                CommonAncestor;
    public readonly Argument.Bag           ArgumentsBag;

    public TransitionData(
      IState                 currentActiveState,
      ITransition            transition,
      IState<TState, TEvent> targetState,
      IState?                commonAncestor,
      Argument.Bag           argumentsBag)
    {
      CurrentActiveState = currentActiveState;
      Transition         = transition;
      TargetState        = targetState;
      ArgumentsBag       = argumentsBag;
      CommonAncestor     = commonAncestor;
    }
  }
}