using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <inheritdoc cref="IExit{T}" />
  public class Exit<T> : Transitions<T>, IExit<T>
  {
    private readonly Exit _configureExit;

    /// <inheritdoc />
    public Exit(Exit configureExit) : base(configureExit) => _configureExit = configureExit;

    /// <inheritdoc cref="Exit.OnExit" />
    public ITransitions<T> OnExit(Action exitAction)
    {
      _configureExit.OnExit(exitAction);
      return this;
    }


    /// <summary>
    /// Specifies the action to be called on exiting the currently configured state.
    /// </summary>
    public ITransitions<T> OnExit(Action<T> exitAction)
    {
      if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));
      _configureExit.ExitActionInvoker = new ActionInvoker<T>(exitAction);
      return this;
    }
  }
}