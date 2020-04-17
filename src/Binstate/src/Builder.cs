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
    private readonly List<Config<TState, TEvent>.Substate> _states = new List<Config<TState, TEvent>.Substate>();

    /// <summary>
    /// Defines the new state in the state machine
    /// </summary>
    /// <param name="stateId">Id of the state, is used to reference it from other elements of the state machine.</param>
    /// <remarks>Use returned syntax-sugar object to configure the new state.</remarks>
    public Config<TState, TEvent>.Substate DefineState([NotNull] TState stateId)
    {
      if (stateId.IsNull()) throw new ArgumentNullException(nameof(stateId));

      var stateConfig = new Config<TState, TEvent>.Substate(stateId);
      _states.Add(stateConfig);
      return stateConfig;
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
      foreach (var stateConfig in _states)
      {
        var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");
          transitions.Add(transition.Event, transition);
        }

        var state = new State<TState, TEvent>(stateConfig.StateId, stateConfig.EnterAction, stateConfig.ExitAction, transitions);
        states.Add(stateConfig.StateId, state);
      }

      // bind states with parent states
      foreach (var stateConfig in _states)
        if (stateConfig.ParentStateId.IsNotNull())
        {
          var state = states[stateConfig.StateId];
          var parentState = states[stateConfig.ParentStateId];
          state.AddParent(parentState);
        }

      if (!states.ContainsKey(initialState))
        throw new ArgumentException($"No state '{initialState}' is defined");
      ValidateStateMachine(states);

      return new StateMachine<TState, TEvent>(states[initialState], states);
    }

    private static void ValidateStateMachine(Dictionary<TState, State<TState, TEvent>> states)
    {
      foreach (var state in states.Values)
      foreach (var transition in state.Transitions.Values.Where(_ => _.IsStatic))
      {
        if (!states.ContainsKey(transition.GetTargetStateId()))
          throw new InvalidOperationException($"Transition '{transition.Event}' from state '{state.Id}' references not defined state '{transition.GetTargetStateId}'");
      }
    }
  }
}