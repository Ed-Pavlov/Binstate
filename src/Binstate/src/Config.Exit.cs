using System;

namespace Binstate;

// ReSharper disable once UnusedTypeParameter
public static partial class Config<TState, TEvent>
{
  /// <summary>
  /// This class is used to configure exit action of the currently configured state.
  /// </summary>
  public class Exit : Transitions
  {
    internal IExitActionInvoker? ExitActionInvoker;

    /// <inheritdoc />
    protected Exit(TState stateId) : base(stateId) { }

    /// <summary>
    /// Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    public virtual Transitions OnExit(Action exitAction)
    {
      if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));
      ExitActionInvoker = ExitActionInvokerFactory.Create(exitAction);
      return this;
    }
  }

  /// <inheritdoc />
  public class Exit<T> : Exit
  {
    private readonly Exit _state;

    /// <inheritdoc />
    public Exit(Exit state) : base(state.StateId) => _state = state;

    /// <inheritdoc />
    public override Transitions AddTransition(TEvent @event, TState stateId, Action? action = null) => _state.AddTransition(@event, stateId, action);

    /// <inheritdoc />
    public override Transitions AddTransition(TEvent @event, GetState<TState> getState) => _state.AddTransition(@event, getState);

    /// <inheritdoc />
    public override Transitions AddTransition(TEvent @event, Func<TState?> getState) => _state.AddTransition(@event, getState);

    /// <inheritdoc />
    public override void AllowReentrancy(TEvent @event) => _state.AllowReentrancy(@event);

    /// <inheritdoc />
    public override Transitions OnExit(Action exitAction) => _state.OnExit(exitAction);


    /// <summary>
    /// Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    public Transitions OnExit(Action<T> exitAction)
    {
      if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));
      _state.ExitActionInvoker = ExitActionInvokerFactory.Create(exitAction);
      return this;
    }
  }
}