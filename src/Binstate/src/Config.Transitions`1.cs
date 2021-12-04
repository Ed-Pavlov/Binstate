using System;

namespace Binstate;

/// <summary>
/// This class provides syntax-sugar to configure the state machine.
/// </summary>
public static partial class Config<TState, TEvent>
{
  /// <inheritdoc cref="ITransitions{T}" />
  public class Transitions<T> : ITransitions<T>
  {
    private readonly Transitions _configTransitions;

    /// <summary/>
    protected Transitions(Transitions configTransitions) => _configTransitions = configTransitions;

    /// <summary>
    /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref>
    /// when <paramref name="event"> event is raised</paramref> 
    /// </summary>
    public ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T>? action = null)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(stateId is null) throw new ArgumentNullException(nameof(stateId));

      var getState= new GetState<TState>(
        (out TState? state) =>
        {
          state = stateId;
          return true;
        });

      var actionInvoker = action is null ? null : new ActionInvoker<T>(action);
      _configTransitions.TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, true, actionInvoker));
      return this;
    }

    /// <inheritdoc cref="ITransitions" />
    public ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null) => _configTransitions.AddTransition(@event, stateId, action);

    /// <inheritdoc cref="ITransitions" />
    public ITransitions AddTransition(TEvent @event, GetState<TState> getState) => _configTransitions.AddTransition(@event, getState);

    /// <inheritdoc cref="ITransitions" />
    public ITransitions AddTransition(TEvent @event, Func<TState?> getState) => _configTransitions.AddTransition(@event, getState);

    /// <inheritdoc cref="ITransitions" />
    public void AllowReentrancy(TEvent @event) => _configTransitions.AllowReentrancy(@event);
  }
}