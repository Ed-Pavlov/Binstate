using System;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    /// <summary>
    /// This interface allows you to specify an action that will be called upon exiting the state currently being configured.
    /// </summary>
    public interface IExitAction : ITransitions
    {
      /// <summary>
      /// Specifies an action to be executed upon exiting the state.
      /// </summary>
      /// <param name="exitAction">The action to execute.</param>
      /// <returns>An <see cref="ITransitions"/> instance for configuring possible transitions.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="exitAction"/> is null.</exception>
      ITransitions OnExit(Action? exitAction = null);
    }


    /// <inheritdoc cref="IExitAction"/>
    public interface IExitAction<TArgument> : ITransitions<TArgument>
    {
      /// <inheritdoc cref="IExitAction.OnExit"/>
      ITransitions<TArgument> OnExit(Action? exitAction = null);

      /// <inheritdoc cref="IExitAction.OnExit"/>
      ITransitions<TArgument> OnExit(Action<TArgument> exitAction);
    }
  }
}