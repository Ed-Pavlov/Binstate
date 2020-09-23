using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This class is used to configure and build a state machine.  
  /// </summary>
  public class Builder<TState, TEvent>
  {
    private readonly Action<Exception> _onException;
    private readonly Dictionary<TState, Config<TState, TEvent>.Substate> _stateConfigs = new Dictionary<TState, Config<TState, TEvent>.Substate>();

    /// <summary>
    /// Creates a builder of a state machine, use it to define state and configure transitions.
    /// </summary>
    /// <param name="onException">All exception thrown from actions passed to the state machine are caught in order to not break the state of the
    /// state machine. Use this action to be notified about these exceptions.</param>
    public Builder([NotNull] Action<Exception> onException) => _onException = onException ?? throw new ArgumentNullException(nameof(onException));

    /// <summary>
    /// Defines the new state in the state machine
    /// </summary>
    /// <param name="stateId">Id of the state, is used to reference it from other elements of the state machine.</param>
    /// <remarks>Use returned syntax-sugar object to configure the new state.</remarks>
    public Config<TState, TEvent>.Substate DefineState([NotNull] TState stateId)
    {
      if (stateId.IsNull()) throw new ArgumentNullException(nameof(stateId));

      var stateConfig = new Config<TState, TEvent>.Substate(stateId);
      _stateConfigs.Add(stateId, stateConfig);
      return stateConfig;
    }

    private State<TState, TEvent> CreateStateAndAddToMap([NotNull] Config<TState, TEvent>.Substate stateConfig, Dictionary<TState, State<TState, TEvent>> states)
    {
      if (!states.TryGetValue(stateConfig.StateId, out var state)) // state could be already created during creating parent states
      {
        state = CreateState(
          stateConfig,
          stateConfig.ParentStateId.IsNotNull()
            ? CreateStateAndAddToMap(_stateConfigs[stateConfig.ParentStateId], states) // recursive call to create the parent state;
            : null);
        states.Add(state.Id, state);
      }
      return state;
    }

    private static State<TState, TEvent> CreateState(
      [NotNull] Config<TState, TEvent>.Substate stateConfig,
      [CanBeNull] State<TState, TEvent> parentState)
    {
      var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
      foreach (var transition in stateConfig.TransitionList)
      {
        if (transitions.ContainsKey(transition.Event))
          throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");

        transitions.Add(transition.Event, transition);
      }

      return new State<TState, TEvent>(stateConfig.StateId, stateConfig.EnterAction, stateConfig.EnterArgumentType, stateConfig.ExitAction, transitions, parentState);
    }

    /// <summary>
    /// Validates consistency and builds the state machine using provided configuration. 
    /// </summary>
    /// <param name="initialState">The initial state of the state machine. The entering action of the initial state is not called by building the state machine.</param>
    /// <exception cref="InvalidOperationException">Throws if there are any inconsistencies in the provided configuration.</exception>
    public StateMachine<TState, TEvent> Build([NotNull] TState initialState)
    {
      if (initialState.IsNull()) throw new ArgumentNullException(nameof(initialState));

      // create all states
      var states = new Dictionary<TState, State<TState, TEvent>>();
      foreach (var stateConfig in _stateConfigs.Values) 
        CreateStateAndAddToMap(stateConfig, states);

      if (!states.ContainsKey(initialState))
        throw new ArgumentException($"No state '{initialState}' is defined");

      ValidateTransitions(states);
      ValidateSubstateEnterArgument(states);

      return new StateMachine<TState, TEvent>(states[initialState], states, _onException);
    }

    private static void ValidateSubstateEnterArgument(Dictionary<TState, State<TState, TEvent>> states)
    {
      foreach (var value in states.Values)
      {
        var state = value;
        if (state.EnterArgumentType != null)
        {
          var parentState = state.ParentState;
          while (parentState != null) // it will check the same states several times, may be I'll optimize it later
          {
            if (parentState.EnterArgumentType == null)
              parentState = parentState.ParentState;
            else
            {
              if (parentState.EnterArgumentType.IsAssignableFrom(state.EnterArgumentType))
              {
                state = parentState;
                parentState = parentState.ParentState;
              }
              else
                throw new InvalidOperationException(
                  $"Parent state '{parentState.Id}' enter action requires argument of type '{parentState.EnterArgumentType}' whereas it's child state '{state.Id}' requires "
                  + $"argument of not assignable to the parent type '{state.EnterArgumentType}'");
            }
          }
        }
      }
    }
    
    private void ValidateTransitions(Dictionary<TState, State<TState, TEvent>> states)
    {
      foreach (var stateConfig in _stateConfigs.Values)
        foreach (var transition in stateConfig.TransitionList.Where(_ => _.IsStatic)) // do not check dynamic transitions because they are depends on the app state
        {
          var targetStateId = transition.GetTargetStateId(_ => { });

          if (!states.TryGetValue(targetStateId, out var state)) // static transition can't throw an exception
            throw new InvalidOperationException($"The transition '{transition.Event}' from the state '{stateConfig.StateId}' references not defined state '{targetStateId}'");

          if (transition.ArgumentType == null)
          {
            if (state.EnterArgumentType != null)
              throw new InvalidOperationException(
                $"The transition '{transition.Event}' from the state '{stateConfig.StateId}' to the state '{targetStateId}' doesn't require argument " +
                $"but enter action of the target state requires an argument of type '{state.EnterArgumentType}'");
          }
          else
          {
            if (state.EnterArgumentType == null)
              throw new InvalidOperationException(
                $"The transition '{transition.Event}' from the state '{stateConfig.StateId}' to the state '{targetStateId}' requires argument " +
                $"of type '{transition.ArgumentType}' but enter action of the target state defined without argument");

            if (!state.EnterArgumentType.IsAssignableFrom(transition.ArgumentType))
              throw new InvalidOperationException(
                $"The enter action argument of type '{state.EnterArgumentType}' is not assignable from the transition argument of type '{transition.ArgumentType}'. " +
                $"See transition '{transition.Event}' from the state '{stateConfig.StateId}' to the state '{targetStateId}'");
          }
        }
    }
  }
}