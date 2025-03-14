using System;
using System.Collections.Generic;

namespace Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class TransitionsEx : ITransitionsEx
    {
      public readonly StateData StateData;

      protected TransitionsEx(StateData stateData) => StateData = stateData;

      public void AllowReentrancy(TEvent @event) => AddTransition(@event, StateData.StateId);

      public ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(stateId is null) throw new ArgumentNullException(nameof(stateId));

        AddTransitionToList(@event, StaticGetState(stateId), true, action);
        return this;
      }

      public ITransitions AddTransition(TEvent @event, GetState<TState> getState)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(getState is null) throw new ArgumentNullException(nameof(getState));

        AddTransitionToList(@event, getState, false, null);
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

#pragma warning disable CS8622
        var getStateWrapper = new GetState<TState>(
          (out TState state) =>
          {
            state = getState() ?? default!;
            return ! EqualityComparer<TState?>.Default.Equals(state, default);
          }
        );
#pragma warning restore CS8622

        AddTransitionToList(@event, getStateWrapper, false, null);
        return this;
      }

#pragma warning disable CS8622
      protected static GetState<TState> StaticGetState(TState stateId)
        => (out TState? state) =>
        {
          state = stateId;
          return true;
        };
#pragma warning restore CS8622

      protected void AddTransitionToList(TEvent @event, GetState<TState> getState, bool isStatic, object? action)
        => StateData.TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, action));
    }
  }
}