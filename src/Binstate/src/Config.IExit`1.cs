using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  /// This interface is used to configure exit action of the currently configured state.
  /// </summary>
  public interface IExit<out T> : IExit, ITransitions<T>
  {
    /// <summary>
    /// Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    ITransitions<T> OnExit(Action<T> exitAction);
  }
}