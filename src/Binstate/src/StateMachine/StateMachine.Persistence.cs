using System;
using System.Linq;
using System.Text.Json;

namespace BeatyBit.Binstate;

internal partial class StateMachine<TState, TEvent>
{
  public string Serialize()
  {
    if(_persistenceSignature is null)
      throw new InvalidOperationException(
        $"This StateMachine is not configured for persistence. "
      + $"Set {nameof(Builder)}.{nameof(Builder.Options)}.{nameof(Builder.Options.EnableStateMachinePersistence)} to true to enable it."
      );

    try
    {
      _lock.WaitOne();

      var states = _states.Values.Select(
                             state =>
                             {
                               var argument = state.GetArgumentAsObject();
                               return argument.HasValue ? new Persistence<TState>.StateData(state.Id, argument) : null;
                             }
                           )
                          .Where(_ => _ is not null)
                          .Cast<Persistence<TState>.StateData>()
                          .ToArray();

      var data = new Persistence<TState>.StateMachineData(_persistenceSignature, _activeState.Id, states);
      return JsonSerializer.Serialize(data);
    }
    finally
    {
      _lock.Set();
    }
  }
}