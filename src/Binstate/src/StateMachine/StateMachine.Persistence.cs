using System;
using System.Linq;
using System.Text.Json;

namespace BeatyBit.Binstate;

internal partial class StateMachine<TState, TEvent>
{
  /// <inheritdoc />
  public string Serialize(ICustomSerializer? customSerializer = null)
  {
    if(_persistenceSignature is null)
      throw new InvalidOperationException(
        $"This StateMachine is not configured for persistence. "
      + $"Set {nameof(Builder)}.{nameof(Builder.Options)}.{nameof(Builder.Options.EnableStateMachinePersistence)} to true to enable it."
      );

    try
    {
      _lock.WaitOne(); // prevent state machine from being used

      var states = _states.Values.Select(
                             state =>
                             {
                               var maybe = state.GetArgumentAsObject();
                               if(! maybe.HasValue) return null; // no argument - no serialization needed

                               var argument = maybe.Value;
                               if(argument is null) return null; // argument is null - no serialization needed

                               var stateIdData  = Persistence.CreateItem(state.Id, customSerializer);
                               var argumentData = Persistence.CreateItem(argument, customSerializer);
                               return new Persistence.StateData(stateIdData, argumentData);
                             }
                           )
                          .Where(_ => _ is not null)
                          .Cast<Persistence.StateData>()
                          .ToArray();

      var activeStateId    = Persistence.CreateItem(_activeState.Id, customSerializer);
      var stateMachineData = new Persistence.StateMachineData(_persistenceSignature, activeStateId, states);
      return stateMachineData.Serialize();
    }
    finally
    {
      _lock.Set(); // allow state machine to be used again
    }
  }
}