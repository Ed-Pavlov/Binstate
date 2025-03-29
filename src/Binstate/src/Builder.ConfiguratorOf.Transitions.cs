using System;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class TransitionsEx : ITransitionsEx
    {
      public readonly StateData StateData;

      protected TransitionsEx(StateData stateData) => StateData = stateData;

      public void AllowReentrancy(TEvent @event) => AddTransitionToList(@event, CreateStaticGetState(StateData.StateId), true, true, null);

      public ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(stateId is null) throw new ArgumentNullException(nameof(stateId));

        AddTransitionToList(@event, CreateStaticGetState(stateId), true, false, action);
        return this;
      }

      public ITransitions AddTransition(TEvent @event, GetState<TState> getState)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(getState is null) throw new ArgumentNullException(nameof(getState));

        AddTransitionToList(@event, getState, false, false, null);
        return this;
      }

      public ITransitions<T> AddTransition<T>(TEvent @event, TState stateId, Action<T> action)
      {
        StateData.Factory = new StateFactory<T>();

        var transitions = new Transitions<T>(StateData);
        return transitions.AddTransition(@event, stateId, action); // delegate call
      }

      /// <inheritdoc />
      public ITransitions AddTransition(TEvent @event, Func<TState?> getState)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(getState is null) throw new ArgumentNullException(nameof(getState));

        AddTransitionToList(@event, CreateDynamicGetState(getState), false, false, null);
        return this;
      }

      protected void AddTransitionToList(TEvent @event, GetState<TState> getState, bool isStatic, bool isReentrant, object? action)
        => StateData.TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, isReentrant, action));
    }
  }
}