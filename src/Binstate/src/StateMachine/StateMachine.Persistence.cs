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
      _lock.WaitOne();

      var states = _states.Values.Select(
                             state =>
                             {
                               var maybe = state.GetArgumentAsObject();
                               if(!maybe.HasValue) return null;

                               var argument = maybe.Value;
                               if(argument is null) return null;

                               var argumentType = argument.GetType().FullName;
                               if(argumentType is null) throw Paranoia.GetException("ArgumentType is null");

                               if(! argument.GetType().IsPrimitive && argument.GetType() != typeof(string))
                               {
                                 if(customSerializer is null) throw new InvalidOperationException("If not primitive types used as arguments, then the valid custom serializer must be provided");
                                 argument = customSerializer.Serialize(argument);
                               }

                               return new Persistence<TState>.Write.StateData(state.Id, argumentType, argument);
                             }
                           )
                          .Where(_ => _ is not null)
                          .Cast<Persistence<TState>.Write.StateData>()
                          .ToArray();

      var data = new Persistence<TState>.Write.StateMachineData(_persistenceSignature, _activeState.Id, states);
      return JsonSerializer.Serialize(data);
    }
    finally
    {
      _lock.Set();
    }
  }
}