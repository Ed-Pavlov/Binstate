using System;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class Transitions<T> : TransitionsEx, ITransitions<T>
    {
      public Transitions(StateData stateData) : base(stateData) { }

      public ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T> action)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(stateId is null) throw new ArgumentNullException(nameof(stateId));
        if(action == null) throw new ArgumentNullException(nameof(action));

        AddTransitionToList(@event, CreateStaticGetState(stateId), true, action);
        return this;
      }
    }
  }
}