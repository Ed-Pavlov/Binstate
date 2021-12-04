using System;

namespace Binstate;

// ReSharper disable once UnusedTypeParameter
public static partial class Config<TState, TEvent>
{
  /// <inheritdoc cref="IExit" />
  public class Exit : Transitions, IExit
  {
    internal IActionInvoker? ExitActionInvoker;

    /// <inheritdoc />
    protected Exit(TState stateId) : base(stateId) { }

    /// <inheritdoc />
    public virtual ITransitions OnExit(Action exitAction)
    {
      if(exitAction == null) throw new ArgumentNullException(nameof(exitAction));
      ExitActionInvoker = new ActionInvoker(exitAction);
      return this;
    }
  }
}