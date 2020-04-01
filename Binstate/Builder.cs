using System.Collections.Generic;
using System.Linq;

namespace Binstate
{
  public static class Builder
  {
    private static readonly List<StateConfig> States = new List<StateConfig>();
    
    public static StateConfig AddState(object state)
    {
      var stateConfig = new StateConfig(state);
      States.Add(stateConfig);
      return stateConfig;
    }
    
    public static StateMachine Build(object initialState)
    {
      var states = new Dictionary<object, State>();
      foreach (var stateConfig in States)
      {
        var transitions = stateConfig.Transitions.ToDictionary(transition => transition.Trigger);
        var state = new State(stateConfig.State, stateConfig.Enter, stateConfig.Exit, transitions);
        states.Add(stateConfig.State, state);
      }
      return new StateMachine(states[initialState], states);
    }
  }
}