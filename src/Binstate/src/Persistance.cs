using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal class Persistance<TState>
{
  internal class StateMachineData
  {
    [JsonConstructor]
    public StateMachineData(string signature, TState activeStateId, StateData[] states)
    {
      ActiveStateId = activeStateId;
      States        = states;
      Signature     = signature;

      StatesMap = states.ToDictionary(_ => _.StateId);
    }

    public string Signature     { get; }
    public TState ActiveStateId { get; }
    [JsonInclude]
    private StateData[] States { get; }

    [JsonIgnore]
    public Dictionary<TState, StateData> StatesMap { get; }

    public StateData GetPersistedState(TState              stateId)            => StatesMap[stateId];

    public bool GetArgumentFor<TEvent>(IState<TState, TEvent> state, out object? argument)
    {
      argument = null;

      var argumentType = state.GetArgumentTypeSafe();
      if(argumentType is null) return false; // the state doesn't require argument

      var persistedState = StatesMap.GetValueSafe(state.Id);
      if(persistedState is null) Throw.ParanoiaException($"persistence signature matches but state {state.Id} is not found in serialized data.");

      argument = persistedState.GerArgumentFor(argumentType);
      return true;
    }

    public void RestoreStateArguments<TEvent>(IEnumerable<IState<TState, TEvent>> states)
    {
      foreach(var state in states)
      {
        var argumentType = state.GetArgumentTypeSafe();
        if(argumentType is not null) // requires argument
        {
          var persistedState = StatesMap.GetValueSafe(state.Id);
          if(persistedState is null) Throw.ParanoiaException($"persistence signature matches but state {state.Id} is not found in serialized data.");

          var argument = persistedState.GerArgumentFor(argumentType);
          Argument.SetArgumentByReflectionUnsafe(state, argument);
        }
      }
    }
  }

  internal class StateData
  {
    [JsonConstructor]
    public StateData(TState stateId, bool isArgumentSet, object? argument)
    {
      StateId       = stateId;
      IsArgumentSet = isArgumentSet;
      Argument      = argument;
    }

    public TState StateId { get; }

    [JsonInclude]
    private bool IsArgumentSet { get; }
    [JsonInclude]
    private object? Argument { get; }

    public object? GerArgumentFor(Type stateArgumentType)
    {
      if(! IsArgumentSet) Throw.ParanoiaException("argument of active state should be persisted.");

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