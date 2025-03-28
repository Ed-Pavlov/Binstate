using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal abstract class Persistence<TState>
{
  internal class StateMachineData
  {
    [JsonIgnore]
    private readonly Dictionary<TState, StateData> _statesMap;

    [JsonConstructor]
    public StateMachineData(string signature, TState activeStateId, StateData[] states)
    {
      ActiveStateId = activeStateId;
      States        = states;
      Signature     = signature;

      _statesMap = states.ToDictionary(_ => _.StateId);
    }

    public string Signature     { get; }
    public TState ActiveStateId { get; }
    [JsonInclude]
    private StateData[] States { get; }

    public void RestoreStateArguments<TEvent>(IEnumerable<IState<TState, TEvent>> states)
    {
      foreach(var state in states)
      {
        var argumentType = state.GetArgumentTypeSafe();
        if(argumentType is not null) // requires argument
        {
          var persistedState = _statesMap.GetValueSafe(state.Id);
          if(persistedState is null) Throw.ParanoiaException($"persistence signature matches but state {state.Id} is not found in serialized data.");

          var argument = persistedState.GerArgumentFor(argumentType);
          Argument.SetArgumentByReflectionUnsafe(state, argumentType, argument);
        }
      }
    }
  }

  internal class StateData
  {
    [JsonConstructor]
    public StateData(TState stateId, object? argument)
    {
      StateId  = stateId;
      Argument = argument;
    }

    public TState StateId { get; }

    [JsonInclude]
    private object? Argument { get; }

    public object? GerArgumentFor(Type stateArgumentType)
    {
      if(Argument is null)
      {
        if(stateArgumentType.IsValueType) Throw.ParanoiaException("value type argument can't be null.");
      }
      else
      {
        if(! stateArgumentType.IsInstanceOfType(Argument))
          Throw.ParanoiaException($"persisted argument should be instance of the state argument type {stateArgumentType}, but it is {Argument.GetType()}.");
      }

      return Argument;
    }
  }
}