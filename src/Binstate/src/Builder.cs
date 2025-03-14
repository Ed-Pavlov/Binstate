using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatyBit.Binstate;

/// <summary>
/// This class is used to configure and build a state machine.
/// </summary>
public partial class Builder<TState, TEvent>
  where TState : notnull
  where TEvent : notnull
{
  private readonly Action<Exception>                        _onException;
  private readonly bool                                     _allowDefaultValue;
  private readonly Dictionary<TState, ConfiguratorOf.State> _stateConfigs = new Dictionary<TState, ConfiguratorOf.State>();

  /// <summary>
  /// Creates a builder of a state machine, use it to define state and configure transitions.
  /// </summary>
  /// <param name="onException">
  /// All exception thrown from 'enter', 'exit', and 'transition' actions passed to the state machine are caught in order not break the state of the state machine.
  /// Use this action to be notified about these exceptions.
  /// </param>
  /// <param name="allowDefaultValue">Specifies if the 'default' value can be used as a valid State ID
  /// in case of <typeparamref name="TState"/> is a <see cref="ValueType"/>, e.g. <see cref="Enum"/>.</param>
  public Builder(Action<Exception> onException, bool allowDefaultValue = false)
  {
    _onException       = onException ?? throw new ArgumentNullException(nameof(onException));
    _allowDefaultValue = allowDefaultValue;
  }

  /// <summary>
  /// Defines the new state in the state machine, if it is already defined throws an exception.
  /// </summary>
  /// <param name="stateId"> ID of the state; is used to reference it from other elements of the state machine. </param>
  /// <remarks> Use returned syntax-sugar object to configure the new state. </remarks>
  public ConfiguratorOf.IState DefineState(TState stateId)
  {
    if(stateId is null) throw new ArgumentNullException(nameof(stateId));

    if(! _allowDefaultValue && EqualityComparer<TState>.Default.Equals(stateId, default!))
      throw new ArgumentException($"'{nameof(stateId)}' cannot be default value", nameof(stateId));

    var state = new ConfiguratorOf.State(new StateData(stateId));
    _stateConfigs.Add(stateId, state);
    return state;
  }

  /// <summary>
  /// Defines the new state in the state machine, if it is already defined, returns the configurator.
  /// </summary>
  /// <param name="stateId"> ID of the state; is used to reference it from other elements of the state machine. </param>
  /// <remarks> Use returned syntax-sugar object to configure the new state. </remarks>
  public ConfiguratorOf.IState GetOrDefineState(TState stateId)
  {
    if(stateId is null) throw new ArgumentNullException(nameof(stateId));

    return _stateConfigs.TryGetValue(stateId, out var state) ? state : DefineState(stateId);
  }

  /// <summary>
  /// Validates consistency and builds the state machine using provided configuration.
  /// </summary>
  /// <param name="initialStateId"> The initial state of the state machine. </param>
  /// <param name="initialStateArgument"> If initial state requires argument use this overload to pass it </param>
  /// <param name="argumentTransferMode">The mode of transferring arguments to new newly activated states. See <see cref="ArgumentTransferMode"/> for details.</param>
  /// <exception cref="InvalidOperationException"> Throws if there are any inconsistencies in the provided configuration. </exception>
  public IStateMachine<TEvent> Build<T>(TState initialStateId, T initialStateArgument, ArgumentTransferMode argumentTransferMode = ArgumentTransferMode.Strict)
  {
    if(initialStateId is null) throw new ArgumentNullException(nameof(initialStateId));

    if(! _stateConfigs.TryGetValue(initialStateId, out var initialStateConfig))
      throw new ArgumentException($"No state '{initialStateId}' is defined");

    if(! IfTransitionDefined(initialStateConfig.StateData))
      throw new ArgumentException("No transitions defined from the initial state nor from its parents.");

    // create all states
    var states = new Dictionary<TState, IState<TState, TEvent>>();
    foreach(var stateConfig in _stateConfigs.Values)
      CreateStateAndAddToMap(stateConfig.StateData, states);

    ValidateTransitions(states);

    if(argumentTransferMode == ArgumentTransferMode.Strict)
      ValidateSubstateEnterArgument(states.Values);

    var stateMachine = new StateMachine<TState, TEvent>(states, _onException, initialStateId);
    stateMachine.EnterInitialState(initialStateArgument);
    return stateMachine;
  }

  /// <summary>
  /// Validates consistency and builds the state machine using provided configuration.
  /// </summary>
  /// <param name="initialStateId"> The initial state of the state machine. </param>
  /// <param name="argumentTransferMode">The mode of transferring arguments to new newly activated states. See <see cref="ArgumentTransferMode"/> for details.</param>
  /// <exception cref="InvalidOperationException"> Throws if there are any inconsistencies in the provided configuration. </exception>
  public IStateMachine<TEvent> Build(TState initialStateId, ArgumentTransferMode argumentTransferMode = ArgumentTransferMode.Strict)
    => Build<Unit>(initialStateId, default, argumentTransferMode);

  private IState<TState, TEvent> CreateStateAndAddToMap(StateData stateData, Dictionary<TState, IState<TState, TEvent>> states)
  {
    if(! states.TryGetValue(stateData.StateId, out var state)) // state could be already created during creating parent states
    {
      state = stateData.CreateState(
        stateData.ParentStateId.HasValue
          ? CreateStateAndAddToMap(_stateConfigs[stateData.ParentStateId.Value].StateData, states) // recursive call to create the parent state;
          : null
      );

      states.Add(state.Id, state);
    }

    return state;
  }

  private bool IfTransitionDefined(StateData stateData)
  {
    var parent = stateData;
    while(parent is not null)
    {
      if(parent.TransitionList.Count > 0)
        return true;

      parent = parent.ParentStateId.HasValue ? _stateConfigs[parent.ParentStateId.Value].StateData : null;
    }

    return false;
  }

  private static void ValidateSubstateEnterArgument(IEnumerable<IState<TState, TEvent>> states)
  {
    foreach(var value in states.Where(_ => _.IsRequireArgument())) // it will check the same states several times, maybe I'll optimize it later
    {
      var state       = value;
      var parentState = state.ParentState;

      while(parentState is not null)
      {
        if(! parentState.IsRequireArgument()) // find the first parent state that requires argument
        {
          parentState = parentState.ParentState;
          continue;
        }

        if(parentState.CanAcceptArgumentFrom(state)) // check if the parent state can accept argument from the current state and go further
        {
          state       = parentState;
          parentState = parentState.ParentState;
        }
        else
          throw new InvalidOperationException(
            $"Parent state '{parentState}' requires argument of type '{parentState.GetArgumentType()}' whereas it's child state '{state}' requires "
          + $"argument of not assignable to the parent type '{state.GetArgumentType()}'. "
          + $"Consider enabling {nameof(ArgumentTransferMode)}.{ArgumentTransferMode.Free}."
          );
      }
    }
  }

  /// <summary>
  /// Validates that transitions don't reference not defined states
  /// </summary>
  /// <exception cref="InvalidOperationException" />
  private void ValidateTransitions(Dictionary<TState, IState<TState, TEvent>> states)
  {
    foreach(var stateConfig in _stateConfigs.Values.Select(_ => _.StateData))
    foreach(var transition in stateConfig.TransitionList.Values.Where(_ => _.IsStatic)) // do not check dynamic transitions because they depend on the app state
    {
      if(! transition.GetTargetStateId(out var targetStateId))
        Throw.ImpossibleException("it's impossible to have a transition without target state");

      if(! states.ContainsKey(targetStateId))
        throw new InvalidOperationException(
          $"The transition '{transition.Event}' from the state '{stateConfig.StateId}' references not defined state '{targetStateId}'"
        );
    }
  }
}