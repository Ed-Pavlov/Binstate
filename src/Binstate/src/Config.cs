using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This class provides syntax-sugar to configure the state machine.
  /// </summary>
  public static partial class Config<TState, TEvent>
  {
    private interface IStateFactory
    {
      State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState);
    }

    private class NoArgumentStateFactory : IStateFactory
    {
      public State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState)
      {
        var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");

          transitions.Add(transition.Event, transition);
        }

        return new State<TState, TEvent>(stateConfig.StateId, stateConfig.EnterAction, null, stateConfig.ExitAction, transitions, parentState);
      }
    }

    private class StateFactory<TArgument> : IStateFactory
    {
      public State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState)
      {
        var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");

          transitions.Add(transition.Event, transition);
        }

        return new State<TState, TEvent, TArgument>(stateConfig.StateId, stateConfig.EnterAction, stateConfig.ExitAction, transitions, parentState);
      }
    }
  }
}