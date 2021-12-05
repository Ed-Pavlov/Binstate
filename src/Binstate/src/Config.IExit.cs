using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  ///   This interface is used to configure exit action of the currently configured state.
  /// </summary>
  public interface IExit : ITransitions
  {
    /// <summary>
    ///   Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    ITransitions OnExit(Action exitAction);
  }

  /// <summary>
  ///
  /// </summary>
  public interface IExitEx : IExit
  {
    /// <summary>
    ///
    /// </summary>
    ITransitions<T> OnExit<T>(Action<T> exitAction);
  }

  /// <summary>
  ///   This interface is used to configure exit action of the currently configured state.
  /// </summary>
  public interface IExit<out T> : ITransitions<T>
  {
    /// <summary>
    ///   Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    ITransitions<T> OnExit(Action exitAction);

    /// <summary>
    ///   Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    ITransitions<T> OnExit(Action<T> exitAction);
  }
}