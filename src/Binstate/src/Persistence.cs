using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal static class Persistence<TState>
{
  internal class Read
  {
    internal class StateMachineData
    {
      [JsonIgnore]
      private readonly Dictionary<TState, StateData> _statesMap;

      [JsonConstructor]
      public StateMachineData(string signature, TState activeStateId, StateData[] states)
      {
        ActiveStateId = activeStateId;
        Signature     = signature;
        States        = states;

        _statesMap = states.ToDictionary(_ => _.StateId);
      }

      public string      Signature     { get; }
      public TState      ActiveStateId { get; }
      public StateData[] States        { get; }

      public void RestoreStateArguments<TEvent>(IEnumerable<IState<TState, TEvent>> states, ICustomSerializer? customSerializer = null)
      {
        foreach(var state in states)
        {
          var argumentType = state.GetArgumentTypeSafe();
          if(argumentType is not null) // requires argument
          {
            var persistedState = _statesMap.GetValueSafe(state.Id);
            if(persistedState is null) throw Paranoia.GetException($"persistence signature matches but state {state.Id} is not found in serialized data.");

            var argument = persistedState.GerArgumentFor(argumentType, customSerializer);
            Argument.SetArgumentByReflectionUnsafe(state, argumentType, argument);
          }
        }
      }
    }

    internal class StateData
    {
      [JsonConstructor]
      public StateData(TState stateId, string argumentType, object? argument)
      {
        StateId      = stateId;
        ArgumentType = argumentType;
        Argument     = argument;

        if(argument is JsonElement jsonElement)
        {
          var type = Type.GetType(argumentType);
          if(type is not null)
          {
            var converter = TypeDescriptor.GetConverter(type);
            Argument = converter.ConvertFromInvariantString(jsonElement.GetRawText());
          }
        }
      }

      public TState  StateId      { get; }
      public string  ArgumentType { get; }
      public object? Argument     { get; private set; }

      public object? GerArgumentFor(Type stateArgumentType, ICustomSerializer? customSerializer)
      {
        if(Argument is null)
          throw Paranoia.GetException($"persisted argument is null, but it is required for state {StateId}.");

        if(Argument is JsonElement jsonElement)
        {
          if(customSerializer is null) throw new InvalidOperationException("If not primitive types used as arguments or state ID, then the valid custom serializer must be provided");

          Argument = customSerializer.Deserialize(jsonElement.GetRawText());
        }

        if(! stateArgumentType.IsInstanceOfType(Argument))
          throw new InvalidOperationException($"Persisted argument should be instance of the state argument type {stateArgumentType}, but it is {Argument.GetType()}.");

        return Argument;
      }
    }
  }

  internal static class Write
  {
    internal class StateMachineData
    {
      public StateMachineData(string signature, TState activeStateId, StateData[] states)
      {
        ActiveStateId = activeStateId;
        States        = states;
        Signature     = signature;
      }

      public string      Signature     { get; }
      public TState      ActiveStateId { get; }
      public StateData[] States        { get; }
    }

    internal class StateData
    {
      public StateData(TState stateId, string argumentType, object? argument)
      {
        StateId      = stateId;
        ArgumentType = argumentType;
        Argument     = argument;
      }

      public TState  StateId      { get; }
      public string  ArgumentType { get; }
      public object? Argument     { get; }
    }
  }
}