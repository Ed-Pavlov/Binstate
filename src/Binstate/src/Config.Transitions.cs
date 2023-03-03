using System;
using System.Collections.Generic;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  internal class Transitions : ITransitionsEx
  {
    public readonly StateConfig StateConfig;

    protected Transitions(StateConfig stateConfig) => StateConfig = stateConfig;

    /// <summary>
    ///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref> when <paramref name="event"> event is raised </paramref>
    /// </summary>
    public ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(stateId is null) throw new ArgumentNullException(nameof(stateId));

      AddTransitionToList(@event, StaticGetState(stateId), true, action);
      return this;
    }

    /// <inheritdoc />
    public ITransitions AddTransition(TEvent @event, GetState<TState> getState)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(getState is null) throw new ArgumentNullException(nameof(getState));

      AddTransitionToList(@event, getState, false, null);
      return this;
    }

    /// <inheritdoc />
    public ITransitions AddTransition(TEvent @event, Func<TState?> getState)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(getState is null) throw new ArgumentNullException(nameof(getState));

#pragma warning disable CS8622
      var getStateWrapper = new GetState<TState>(
        (out TState? state) =>
        {
          state = getState();
          return ! EqualityComparer<TState?>.Default.Equals(state, default);
        }
      );
#pragma warning restore CS8622

      AddTransitionToList(@event, getStateWrapper, false, null);
      return this;
    }

    public void AllowReentrancy(TEvent @event) => AddTransition(@event, StateConfig.StateId);

    public ITransitions<T> AddTransition<T>(TEvent @event, TState stateId, Action<T> action)
    {
      StateConfig.Factory = new StateFactory<T>();

      var transitions = new Transitions<T>(StateConfig);
      return transitions.AddTransition(@event, stateId, action); // delegate call
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
      => StateConfig.TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, action));
  }
}