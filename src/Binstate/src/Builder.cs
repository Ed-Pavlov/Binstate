using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This class is used to configurate and build a state machine  
  /// </summary>
  public class Builder
  {
    private readonly List<Config.Entering> _states = new List<Config.Entering>();

    /// <summary>
    /// Defines new state in the state machine
    /// </summary>
    /// <param name="state">Id of the state, is used to reference it from other elements of the state machine.</param>
    /// <remarks>Use returned syntax-sugar object to configure the new state</remarks>
    public Config.Entering AddState([NotNull] object state)
    {
      if (state == null) throw new ArgumentNullException(nameof(state));

      var stateConfig = new Config.Entering(state);
      _states.Add(stateConfig);
      return stateConfig;
    }

    /// <summary>
    /// Validates consistency and builds the state machine using provided configuration. 
    /// </summary>
    /// <param name="initialState">The initial state of the state machine. OnEnter of the initial state is not called by building the state machine.</param>
    /// <exception cref="InvalidOperationException">Throws if there are any inconsistences in the provide configuration.</exception>
    public StateMachine Build([NotNull] object initialState)
    {
      if (initialState == null) throw new ArgumentNullException(nameof(initialState));

      var states = new Dictionary<object, State>();
      foreach (var stateConfig in _states)
      {
        var transitions = new Dictionary<object, Transition>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Trigger '{transition.Event}' is already added to state '{stateConfig.State}'");
          transitions.Add(transition.Event, transition);
        }

        var state = new State(stateConfig.State, stateConfig.Enter, stateConfig.Exit, transitions);
        states.Add(stateConfig.State, state);
      }

      if (!states.ContainsKey(initialState))
        throw new ArgumentException($"No state '{initialState}' registered");
      ValidateStateMachine(states);

      return new StateMachine(states[initialState], states);
    }

    private static void ValidateStateMachine(Dictionary<object, State> states)
    {
      foreach (var state in states.Values)
      {
        foreach (var transition in state.Transitions.Values)
        {
          if (!states.ContainsKey(transition.State))
            throw new InvalidOperationException($"Transition '{transition.Event}' from state '{state.Id}' references nonexistent state '{transition.State}'");
        }
      }
    }
  }
}