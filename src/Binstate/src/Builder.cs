using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Binstate
{
  public class Builder
  {
    private readonly Config.Entering _initialState = new Config.Entering(Guid.NewGuid().ToString());

    private readonly List<Config.Entering> _states = new List<Config.Entering>();

    public Builder() => _states = new List<Config.Entering>() {_initialState};

    public Config.Transition InitialState => _initialState;
    
    public Config.Entering AddState([NotNull] object state)
    {
      if (state == null) throw new ArgumentNullException(nameof(state));
      
      var stateConfig = new Config.Entering(state);
      _states.Add(stateConfig);
      return stateConfig;
    }
    
    public StateMachine Build()
    {
      var states = new Dictionary<object, State>();
      foreach (var stateConfig in _states)
      {
        var transitions = new Dictionary<object, Transition>();
        foreach (var transition in stateConfig.Transitions)
        {
          if(transitions.ContainsKey(transition.Trigger)) throw new InvalidOperationException($"Trigger '{transition.Trigger}' is already added to state '{stateConfig.State}'");
          transitions.Add(transition.Trigger, transition);
        }
        
        var state = new State(stateConfig.State, stateConfig.Enter, stateConfig.Exit, transitions);
        states.Add(stateConfig.State, state);
      }

      ValidateStateMachine(states);
      
      return new StateMachine(states[_initialState.State], states);
    }

    private static void ValidateStateMachine(Dictionary<object, State> states)
    {
      foreach (var state in states.Values)
      {
        foreach (var transition in state.Transitions.Values)
        {
          if (!states.ContainsKey(transition.State))
            throw new InvalidOperationException($"Transition '{transition.Trigger}' from state '{state.Id}' references nonexistent state '{transition.State}'");
        }
      }
    }
  }
}