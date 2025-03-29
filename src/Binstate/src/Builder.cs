using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

/// <summary>
/// Just the base type for the <see cref="Builder{TState, TEvent}"/> to simplify the syntax of instantiating <see cref="Options"/>
/// </summary>
public partial class Builder
{
#pragma warning disable CS8622
  internal static GetState<TState> CreateStaticGetState<TState>(TState value)
    => (out TState? state) =>
    {
      state = value;
      return true;
    };

  internal static GetState<TState> CreateDynamicGetState<TState>(Func<TState?> getState)
    => (out TState? state) =>
    {
      state = getState();
      return ! EqualityComparer<TState?>.Default.Equals(state, default);
    };
#pragma warning restore CS8622
}

/// <summary>
/// This class is used to configure and build a state machine.
/// </summary>
/// <typeparam name="TState">Objects of this type are used as dictionary keys, it's your responsibility to provide valid type and objects.</typeparam>
/// <typeparam name="TEvent">Objects of this type are used as dictionary keys, it's your responsibility to provide valid type and objects.</typeparam>
public partial class Builder<TState, TEvent> : Builder
  where TState : notnull
  where TEvent : notnull
{
  private readonly Action<Exception> _onException;
  private readonly Options           _options;

  private readonly Dictionary<TState, ConfiguratorOf.State> _stateConfigurators = new();

  /// <summary>
  /// Creates a builder of a state machine, use it to define state and configure transitions.
  /// </summary>
  /// <param name="onException">
  /// All exception thrown from 'enter', 'exit', and 'transition' actions passed to the state machine are caught in order not break the state of the state machine.
  /// Use this action to be notified about these exceptions.
  /// </param>
  /// <param name="options"> Configuration options for the state machine builder. </param>
  public Builder(Action<Exception> onException, Options? options = null)
  {
    _onException = onException ?? throw new ArgumentNullException(nameof(onException));
    _options     = options     ?? new Options();
  }

  /// <summary>
  /// Defines the new state in the state machine, if it is already defined throws an exception.
  /// </summary>
  /// <param name="stateId"> ID of the state; is used to reference it from other elements of the state machine. </param>
  /// <remarks> Use returned syntax-sugar object to configure the new state. </remarks>
  public ConfiguratorOf.IState DefineState(TState stateId)
  {
    if(stateId is null) throw new ArgumentNullException(nameof(stateId));

    if(! _options.AllowDefaultValueAsStateId && EqualityComparer<TState>.Default.Equals(stateId, default!))
      throw new ArgumentException($"'{nameof(stateId)}' cannot be default value", nameof(stateId));

    var state = new ConfiguratorOf.State(new StateData(stateId));
    _stateConfigurators.Add(stateId, state);
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

    return _stateConfigurators.TryGetValue(stateId, out var state) ? state : DefineState(stateId);
  }

  /// <summary>
  /// Validates consistency and builds the state machine using provided configuration.
  /// </summary>
  /// <param name="initialStateId"> The initial state of the state machine. </param>
  /// <exception cref="InvalidOperationException"> Throws if there are any inconsistencies in the provided configuration. </exception>
  public IStateMachine<TEvent> Build(TState initialStateId) => Build<Unit>(initialStateId, default);

  /// <summary>
  /// Validates consistency and builds the state machine using provided configuration.
  /// </summary>
  /// <param name="initialStateId"> The initial state of the state machine. </param>
  /// <param name="initialStateArgument"> If initial state requires argument use this overload to pass it </param>
  /// <exception cref="InvalidOperationException"> Throws if there are any inconsistencies in the provided configuration. </exception>
  public IStateMachine<TEvent> Build<TArgument>(TState initialStateId, TArgument initialStateArgument)
  {
    if(initialStateId is null) throw new ArgumentNullException(nameof(initialStateId));

    if(! _stateConfigurators.TryGetValue(initialStateId, out var initialStateConfigurator))
      throw new ArgumentException($"No state '{initialStateId}' is defined");

    if(! IsTransitionDefined(initialStateConfigurator.StateData))
      throw new ArgumentException("No transitions defined from the initial state nor from its parents.");

    var states = CreateStates();

    var stateMachine = new StateMachine<TState, TEvent>(states, _onException, initialStateId, CreatePersistenceSignature());
    stateMachine.EnterInitialState(initialStateArgument);
    return stateMachine;
  }

  /// <summary>
  /// Restores a state machine instance from serialized data.
  /// </summary>
  /// <param name="serializedData">A JSON string representing the previously saved state machine's state.
  /// See <see cref="IStateMachine{TEvent}.Serialize"/>.</param>
  /// <param name="customSerializer">An optional serializer used for state argument restoration. If not primitive or string type is used
  /// as <typeparamref name="TState"/> or an argument passed to <see cref="IStateMachine{TEvent}.Raise"/> custom serializer should be provided.</param>
  /// <returns>A restored instance of the state machine configured with the states and active state from the provided serialized data.</returns>
  public IStateMachine<TEvent> Restore(string serializedData, ICustomSerializer? customSerializer = null)
  {
    if(string.IsNullOrWhiteSpace(serializedData)) throw new ArgumentNullException(nameof(serializedData));

    if(! _options.EnableStateMachinePersistence)
      throw new InvalidOperationException(
        $"This {nameof(Builder)} is not configured for persistence. "
      + $"Set {nameof(Builder)}.{nameof(Options)}.{nameof(Options.EnableStateMachinePersistence)} to true to enable it."
      );

    var persistedStateMachine = Persistence.DeserializeStateMachineData(serializedData);
    var persistenceSignature = CreatePersistenceSignature();
    if(persistedStateMachine.Signature != persistenceSignature)
      throw new ArgumentException(
        $"The passed {nameof(serializedData)} doesn't match the configuration of this {nameof(Builder)}. "
      + $"Check that the {nameof(Builder)} using for restoring is configured the same way as the one used for building "
      + $"the stored state machine."
      );

    var states = CreateStates();

    persistedStateMachine.RestoreStateArguments(out var activeStateId, states.Values, customSerializer);

    var stateMachine = new StateMachine<TState, TEvent>(states, _onException, activeStateId, persistenceSignature);
    stateMachine.EnterInitialState();
    return stateMachine;
  }

  /// <summary>
  /// Creates and initializes the states of the state machine using the configuration data provided in the builder.
  /// This method also validates the state transitions and ensures that the argument transfer rules are consistent.
  /// </summary>
  /// <returns>
  /// Returns a dictionary containing the states of the state machine, where the key is the state ID and the value is the corresponding state object.
  /// </returns>
  private Dictionary<TState, IState<TState, TEvent>> CreateStates()
  {
    var states = new Dictionary<TState, IState<TState, TEvent>>();

    foreach(var stateConfigurator in _stateConfigurators.Values)
      CreateStateAndAddToMapRecursively(stateConfigurator.StateData, states);

    ValidateTransitions(states);

    if(_options.ArgumentTransferMode == ArgumentTransferMode.Strict)
      ValidateSubstateEnterArgument(states.Values);

    return states;
  }

  private IState<TState, TEvent> CreateStateAndAddToMapRecursively(StateData stateData, Dictionary<TState, IState<TState, TEvent>> states)
  {
    if(! states.TryGetValue(stateData.StateId, out var state)) // state could be already created during creating parent states
    {
      state = stateData.CreateState(
        stateData.ParentStateId.HasValue
          ? CreateStateAndAddToMapRecursively(
            _stateConfigurators[stateData.ParentStateId.Value].StateData, states
          ) // recursive call to create the parent state;
          : null
      );

      states.Add(state.Id, state);
    }

    return state;
  }

  private bool IsTransitionDefined(StateData stateData)
  {
    var parent = stateData;
    while(parent is not null)
    {
      if(parent.TransitionList.Count > 0)
        return true;

      parent = parent.ParentStateId.HasValue ? _stateConfigurators[parent.ParentStateId.Value].StateData : null;
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
            $"Parent state '{parentState}' requires argument of type '{parentState.GetArgumentTypeSafe()}' whereas it's child state '{state}' requires "
          + $"argument of not assignable to the parent type '{state.GetArgumentTypeSafe()}'. "
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
    foreach(var stateConfig in _stateConfigurators.Values.Select(_ => _.StateData))
    foreach(var transition in stateConfig.TransitionList.Values.Where(_ => _.IsStatic)) // do not check dynamic transitions because they depend on the app state
    {
      if(! transition.GetTargetStateId(out var targetStateId))
        throw Paranoia.GetException("it's impossible to have a transition without target state");

      if(! states.ContainsKey(targetStateId))
        throw new InvalidOperationException(
          $"The transition '{transition.Event}' from the state '{stateConfig.StateId}' references not defined state '{targetStateId}'"
        );
    }
  }

  /// <summary>
  /// Create a unique signature for this instance based on _options and _stateConfigurators.
  /// </summary>
  private string? CreatePersistenceSignature()
  {
    if(! _options.EnableStateMachinePersistence) return null;

    using var sha256 = SHA256.Create();

    using var stream = new MemoryStream();
    using var writer = new StreamWriter(stream);

    writer.Write(JsonSerializer.Serialize(_options));

    foreach(var stateData in _stateConfigurators.Values.Select(_ => _.StateData))
    {
      writer.Write(JsonSerializer.Serialize(stateData.StateId));
      stateData.ParentStateId
               .Apply(
                  _ =>
                  {
                    if(_.HasValue)
                      writer.Write(JsonSerializer.Serialize(_.Value));
                  }
                );

      writer.Write(stateData.EnterAction?.GetType());
      writer.Write(stateData.ExitAction?.GetType());

      foreach(var transition in stateData.TransitionList.Values)
      {
        writer.Write(JsonSerializer.Serialize(transition.Event));
        if(transition.IsStatic)
        {
          transition.GetTargetStateId(out var targetStateId);
          writer.Write(JsonSerializer.Serialize(targetStateId));
          writer.Write(transition.TransitionAction?.GetType());
        }
      }
    }

    writer.Flush();
    writer.BaseStream.Seek(0, SeekOrigin.Begin);

    var hashBytes = sha256.ComputeHash(stream);
    return Convert.ToBase64String(hashBytes);
  }
}