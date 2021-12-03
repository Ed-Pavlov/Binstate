using System;
using System.Collections.Generic;
using System.Linq;

namespace Binstate;

/// <summary>
/// This class is used to configure and build a state machine.  
/// </summary>
public class Builder<TState, TEvent> where TState : notnull where TEvent : notnull
{
  private readonly Action<Exception>                                _onException;
  private readonly Dictionary<TState, Config<TState, TEvent>.State> _stateConfigs = new();

  /// <summary>
  /// Creates a builder of a state machine, use it to define state and configure transitions.
  /// </summary>
  /// <param name="onException">All exception thrown from enter and exit actions passed to the state machine are caught in order to not break the state of the
  /// state machine. Use this action to be notified about these exceptions.</param>
  public Builder(Action<Exception> onException) => _onException = onException ?? throw new ArgumentNullException(nameof(onException));

  /// <summary>
  /// Defines the new state in the state machine, if it is already defined throws an exception
  /// </summary>
  /// <param name="stateId">Id of the state, is used to reference it from other elements of the state machine.</param>
  /// <remarks>Use returned syntax-sugar object to configure the new state.</remarks>
  public Config<TState, TEvent>.State DefineState(TState stateId)
  {
    if(stateId is null) throw new ArgumentNullException(nameof(stateId));

    var stateConfig = new Config<TState, TEvent>.State(stateId);
    _stateConfigs.Add(stateId, stateConfig);

    return stateConfig;
  }

  /// <summary>
  /// Defines the new state in the state machine, if it is already defined, returns the configurator.
  /// </summary>
  /// <param name="stateId">Id of the state, is used to reference it from other elements of the state machine.</param>
  /// <remarks>Use returned syntax-sugar object to configure the new state.</remarks>
  public Config<TState, TEvent>.State GetOrDefineState(TState stateId)
  {
    if(stateId is null) throw new ArgumentNullException(nameof(stateId));

    if(!_stateConfigs.TryGetValue(stateId, out var stateConfig))
      stateConfig = DefineState(stateId);

    return stateConfig;
  }

  /// <summary>
  /// Validates consistency and builds the state machine using provided configuration. 
  /// </summary>
  /// <param name="initialStateId">The initial state of the state machine.</param>
  /// <param name="initialStateArgument">If initial state requires argument use this overload to pass it</param>
  /// <param name="enableLooseRelaying">Enables non strict relaying model of the argument attached ot the state. If relayed argument type
  /// is <see cref="ITuple{TPassed,TRelay}"/> all 'enter' actions receiving arguments of type  <see cref="ITuple{TPassed,TRelay}"/>, TArgument, and TRelay
  /// will receive corresponding argument.
  /// If not enabled - all 'enter' action in child/parent relation should have the same parameter type of declared 'enter' action. Also it's possible that some
  /// of state requires an argument but some not.
  /// </param>
  /// <exception cref="InvalidOperationException">Throws if there are any inconsistencies in the provided configuration.</exception>
  public StateMachine<TState, TEvent> Build<T>(TState initialStateId, T initialStateArgument, bool enableLooseRelaying = false)
  {
    if(initialStateId is null) throw new ArgumentNullException(nameof(initialStateId));

    if(!_stateConfigs.ContainsKey(initialStateId))
      throw new ArgumentException($"No state '{initialStateId}' is defined");

    var initialStateConfig = _stateConfigs[initialStateId];

    if(initialStateConfig.TransitionList.Count == 0)
      throw new ArgumentException("No transitions defined from the initial state");

    // create all states
    var states = new Dictionary<TState, State<TState, TEvent>>();

    foreach(var stateConfig in _stateConfigs.Values)
      CreateStateAndAddToMap(stateConfig, states);

    var initialState = states[initialStateId];

    if(Argument.IsSpecified<T>())
    {
      if(initialState.EnterArgumentType is null)
        throw new InvalidOperationException("The enter action of the initial state doesn't require argument, but argument is provided.");
    }
    else
    {
      if(initialState.EnterArgumentType is not null)
        throw new InvalidOperationException("The enter action of the initial state requires argument, but no argument is provided.");
    }

    ValidateTransitions(states);

    if(!enableLooseRelaying)
      ValidateSubstateEnterArgument(states);

    var stateMachine = new StateMachine<TState, TEvent>(states, _onException);
    stateMachine.SetInitialState(initialStateId, initialStateArgument);

    return stateMachine;
  }

  /// <summary>
  /// Validates consistency and builds the state machine using provided configuration. 
  /// </summary>
  /// <param name="initialStateId">The initial state of the state machine.</param>
  /// <param name="enableLooseRelaying">Enables non strict relaying model of the argument attached ot the state. If relayed argument type
  /// is <see cref="ITuple{TPassed,TRelay}"/> all 'enter' actions receiving arguments of type  <see cref="ITuple{TPassed,TRelay}"/>, TArgument, and TRelay
  /// will receive corresponding argument.
  /// If not enabled - all 'enter' action in child/parent relation should have the same parameter type of declared 'enter' action. Also it's possible that some
  /// of state requires an argument but some not.
  /// </param>
  /// <exception cref="InvalidOperationException">Throws if there are any inconsistencies in the provided configuration.</exception>
  public StateMachine<TState, TEvent> Build(TState initialStateId, bool enableLooseRelaying = false)
    => Build<Unit>(initialStateId, default, enableLooseRelaying);

  private State<TState, TEvent> CreateStateAndAddToMap(Config<TState, TEvent>.State stateConfig, Dictionary<TState, State<TState, TEvent>> states)
  {
    if(!states.TryGetValue(stateConfig.StateId, out var state)) // state could be already created during creating parent states
    {
      state = stateConfig.CreateState(
        stateConfig.ParentStateId.HasValue
          ? CreateStateAndAddToMap(_stateConfigs[stateConfig.ParentStateId.Value!], states) // recursive call to create the parent state;
          : null);

      states.Add(state.Id, state);
    }

    return state;
  }

  private static void ValidateSubstateEnterArgument(Dictionary<TState, State<TState, TEvent>> states)
  {
    foreach(var value in states.Values)
    {
      var state = value;

      if(state.EnterArgumentType is not null)
      {
        var parentState = state.ParentState;

        while(parentState is not null) // it will check the same states several times, may be I'll optimize it later
          if(parentState.EnterArgumentType is null)
          {
            parentState = parentState.ParentState;
          }
          else
          {
            if(parentState.EnterArgumentType.IsAssignableFrom(state.EnterArgumentType))
            {
              state       = parentState;
              parentState = parentState.ParentState;
            }
            else
            {
              throw new InvalidOperationException(
                $"Parent state '{parentState.Id}' enter action requires argument of type '{parentState.EnterArgumentType}' whereas it's child state '{state.Id}' requires "
              + $"argument of not assignable to the parent type '{state.EnterArgumentType}'");
            }
          }
      }
    }
  }

  // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
  private void ValidateTransitions(Dictionary<TState, State<TState, TEvent>> states)
  {
    foreach(var stateConfig in _stateConfigs.Values)
    foreach(var transition in
            stateConfig.TransitionList.Values.Where(_ => _.IsStatic)) // do not check dynamic transitions because they are depends on the app state
    {
      transition.GetTargetStateId(out var targetStateId);

      if(!states.ContainsKey(targetStateId!))
        throw new InvalidOperationException(
          $"The transition '{transition.Event}' from the state '{stateConfig.StateId}' references not defined state '{targetStateId}'");
    }
  }
}