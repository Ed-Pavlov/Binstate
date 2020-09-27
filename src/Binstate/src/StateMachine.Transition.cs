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
    private TransitionData<MixOf<TA, TP>>? PrepareTransition<TA, TP>(TEvent @event, TA argument)
    {
      try
      {
        _lock.WaitOne();

        if (!_activeState.FindTransitionTransitive(@event, out var transition)
          || !transition.GetTargetStateId(out var stateId)) // looks for a transition through all parent states
        {
          _lock.Set();
          return null;
        }

        var targetState = GetStateById(stateId);

        var commonAncestor = FindLeastCommonAncestor(targetState, _activeState);
        var statesToEnter = targetState.GetAllStatesForActivationTillParent(commonAncestor); // get states from activeState with all parents till newState itself to activate
        var mixedArgument = PrepareRealArgument<TA, TP>(argument, _activeState);
        ValidateStates(statesToEnter, _activeState, @event, mixedArgument); // validate before changing any state of the state machine

        return new TransitionData<MixOf<TA, TP>>(_activeState, transition, targetState, statesToEnter, commonAncestor, mixedArgument);
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

    private static MixOf<TA, TP> PrepareRealArgument<TA, TP>(TA argument, State<TState, TEvent> sourceState)
    {
      MixOf<TA, TP> mix;
      if (typeof(TP) == typeof(Unit))
        mix = typeof(TA) == typeof(Unit) ? MixOf<TA, TP>.Empty : new MixOf<TA, TP>(argument.ToMaybe(), Maybe<TP>.Nothing);
      else
      {
        if (!(sourceState is State<TState, TEvent, TP> stateWithArgument)) // trying to relay an argument but active state has not an argument of passed type
          throw new TransitionException("propagating from the state w/o a state");

        mix = typeof(TA) == typeof(Unit) ? new MixOf<TA, TP>(Maybe<TA>.Nothing, stateWithArgument.Argument.ToMaybe()) : stateWithArgument.CreateTuple(argument);
      }

      return mix;
    }

    /// <summary>
    /// Performs changes in the state machine state. Doesn't throw any exceptions, exceptions from the user code, 'enter' and 'exit' actions are translated
    /// into the delegate passed to <see cref="Builder{TState,TEvent}(Action{Exception})"/> 
    /// </summary>
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "foreach is more readable here")]
    private bool PerformTransition<TA, TP>(TransitionData<MixOf<TA, TP>> transitionData)
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
    private Action ActivateStateNotGuarded<TA, TP>(State<TState, TEvent> state, MixOf<TA, TP> argument)
    {
      state.IsActive = true; // set is as active inside the lock, see implementation of State class for details
      var controller = new Controller(state, this);

      return state switch
      {
        IState<TState, TEvent, TA> passedArgumentState => () => passedArgumentState.EnterSafe(controller, argument.PassedArgument.Value, _onException),
        IState<TState, TEvent, TP> relayedArgumentState => () => relayedArgumentState.EnterSafe(controller, argument.RelayedArgument.Value, _onException),
        IState<TState, TEvent, ITuple<TA, TP>> bothArgumentsState => () => bothArgumentsState.EnterSafe(controller, argument.ToTuple(), _onException),
        _ => () => state.EnterSafe(controller, _onException) // no arguments state
      };
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