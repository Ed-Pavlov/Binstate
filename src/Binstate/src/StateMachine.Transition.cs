using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    private readonly AutoResetEvent _lock = new AutoResetEvent(true);

    /// <summary>
    /// Performing transition is split into two parts, the first one is "read only", preparing and checking all the data, can throw an exception. 
    /// </summary>
    /// <returns>Returns null if:
    /// no transition found by specified <paramref name="event"/> from the current state
    /// dynamic transition returns 'null'
    /// </returns>
    /// <exception cref="TransitionException">Throws if passed argument doesn't match the 'enter' action of the target state.</exception>
    private TransitionData<T>? PrepareTransition<T>(TEvent @event, T argument)
    {
      try
      {
        _lock.WaitOne();

        var activeState = _activeStates.Peek(); // there should be at least one active state, don't need to check count

        if (!activeState.FindTransitionTransitive(@event, out var transition)
          || !transition.GetTargetStateId(out var stateId)) // looks for a transition through all parent states
        {
          _lock.Set();
          return null;
        }

        var newState = GetStateById(stateId);

        var commonAncestor = FindLeastCommonAncestor(newState, activeState);
        var statesToEnter = newState.GetAllStatesForActivationTillParent(commonAncestor); // get states from activeState with all parents till newState itself to activate 
        ValidateStates(statesToEnter, activeState, @event, argument); // validate before changing any state of the state machine
        return new TransitionData<T>(activeState, commonAncestor, transition, statesToEnter, argument);
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
    /// into the delegate passed to <see cref="Builder{TState,TEvent}(Action{Exception})"/> 
    /// </summary>
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "foreach is more readable here")]
    private bool PerformTransition<T>(TransitionData<T> transitionData)
    {
      var activeState = transitionData.ActiveState;
      var commonAncestor = transitionData.CommonAncestor;
      var transition = transitionData.Transition;
      var statesToEnter = transitionData.StatesToEnter;
      var argument = transitionData.Argument;
      
      var enterActions = new List<Action>();
      try
      {
        // exit all active states which are not parent for the new state
        while (activeState != null && activeState != commonAncestor)
        {
          activeState.ExitSafe(_onException);
          _activeStates.Pop(); // remove from active states after exiting
          activeState = _activeStates.Count == 0 ? null : _activeStates.Peek();
        }

        // invoke action attached to the transition itself
        transition.InvokeActionSafe(_onException);

        // and then activate new active states
        foreach (var state in statesToEnter)
        {
          var enterAction = ActivateStateNotGuarded(state, argument);
          enterActions.Add(enterAction);
        }
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
    private Action ActivateStateNotGuarded<T>(State<TState, TEvent> state, T argument)
    {
      var controller = new Controller(state, this);
      state.IsActive = true; // set is as active inside the lock, see implementation of State class for details
      _activeStates.Push(state);
      return () => state.EnterSafe(controller, argument, _onException);
    }

    private readonly struct TransitionData<T>
    {
      public readonly State<TState, TEvent> ActiveState;
      public readonly State<TState, TEvent> CommonAncestor;
      public readonly Transition<TState, TEvent> Transition;
      public readonly IReadOnlyCollection<State<TState, TEvent>> StatesToEnter;
      public readonly T Argument;

      public TransitionData(
        State<TState, TEvent> activeState,
        State<TState, TEvent> commonAncestor,
        Transition<TState, TEvent> transition,
        IReadOnlyCollection<State<TState, TEvent>> statesToEnter,
        T argument)
      {
        ActiveState = activeState;
        CommonAncestor = commonAncestor;
        Transition = transition;
        StatesToEnter = statesToEnter;
        Argument = argument;
      }
    }
  }
}