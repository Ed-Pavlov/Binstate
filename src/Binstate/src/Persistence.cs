using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

/// <summary>
/// Provides functionality for persisting and restoring the state of a state machine.
/// Contains nested classes for handling state data and serialization.
/// </summary>
public static partial class Persistence
{
  internal static string Serialize(this StateMachineData stateMachineData) => JsonSerializer.Serialize(stateMachineData);

  internal static StateMachineData DeserializeStateMachineData(string serializedData)
  {
    var persistedStateMachine = JsonSerializer.Deserialize<StateMachineData>(serializedData);
    if(persistedStateMachine is null) throw new ArgumentException($"'{nameof(serializedData)}' is not a valid serialized state machine");
    return persistedStateMachine;
  }

  internal class StateMachineData
  {
    [JsonConstructor]
    public StateMachineData(string signature, Item activeStateIdItem, StateData[] statesData)
    {
      ActiveStateIdItem = activeStateIdItem;
      StatesData        = statesData;
      Signature         = signature;
    }

    public string Signature { get; }
    [JsonInclude]
    private Item ActiveStateIdItem { get; }
    [JsonInclude]
    private StateData[] StatesData { get; }

    public void RestoreStateArguments<TState, TEvent>(
      out TState                          activeStateId,
      IEnumerable<IState<TState, TEvent>> states,
      ICustomSerializer?                  customSerializer = null)
    {
      activeStateId = (TState)ActiveStateIdItem.Deserialize(customSerializer);

      IReadOnlyDictionary<TState, object> statesMap =
        StatesData.Select(
                     stateData =>
                     {
                       var stateId  = (TState)stateData.StateId.Deserialize(customSerializer);
                       var argument = stateData.Argument.Deserialize(customSerializer);
                       return ( stateId, argument );
                     }
                   )
                  .ToDictionary<(TState stateId, object argument), TState, object>(tuple => tuple.stateId!, tuple => tuple.argument);

      foreach(var state in states)
      {
        var argumentType = state.GetArgumentTypeSafe();
        if(argumentType is not null) // requires argument
        {
          var argument = statesMap.GetValueSafe(state.Id);
          if(argument is null) throw Paranoia.GetException($"persistence signature matches but state {state.Id} is not found in serialized data.");

          Argument.SetArgumentByReflectionUnsafe(state, argumentType, argument);
        }
      }
    }
  }

  internal class StateData
  {
    public StateData(Item stateId, Item argument)
    {
      StateId  = stateId;
      Argument = argument;
    }

    public Item StateId  { get; }
    public Item Argument { get; }
  }
}