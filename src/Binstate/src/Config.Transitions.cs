using System;
using System.Collections.Generic;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <inheritdoc />
  public class Transitions : ITransitions
  {
    internal readonly TState StateId;

    /// <summary> Protected ctor </summary>
    protected Transitions(TState stateId) => StateId = stateId ?? throw new ArgumentNullException(nameof(stateId));

    internal readonly Dictionary<TEvent, Transition<TState, TEvent>> TransitionList = new();

    /// <summary>
    /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref> when <paramref name="event"> event is raised</paramref> 
    /// </summary>
    public virtual ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(stateId is null) throw new ArgumentNullException(nameof(stateId));

      var getStateWrapper = new GetState<TState>(
        (out TState? state) =>
        {
          state = stateId;

          return true;
        });

      var actionInvoker = action is null ? null : new ActionInvoker(action);
      return AddTransition(@event, getStateWrapper, true, actionInvoker);
    }

    /// <inheritdoc />
    public virtual ITransitions AddTransition(TEvent @event, GetState<TState> getState)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(getState is null) throw new ArgumentNullException(nameof(getState));

      return AddTransition(@event, getState, false, null);
    }

    /// <inheritdoc />
    public virtual ITransitions AddTransition(TEvent @event, Func<TState?> getState)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(getState is null) throw new ArgumentNullException(nameof(getState));

      var getStateWrapper = new GetState<TState>(
        (out TState? state) =>
        {
          state = getState();

          return !EqualityComparer<TState?>.Default.Equals(state, default);
        });

      return AddTransition(@event, getStateWrapper, false, null);
    }

    /// <inheritdoc />
    public virtual void AllowReentrancy(TEvent @event) => AddTransition(@event, StateId);

    private Transitions AddTransition(TEvent @event, GetState<TState> getState, bool isStatic, IActionInvoker? action)
    {
      TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, action));
      return this;
    }
  }
}