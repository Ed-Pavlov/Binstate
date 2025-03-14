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
      /// <returns>An <see cref="ITransitionsEx"/> instance for configuring possible transitions.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="exitAction"/> is null.</exception>
      ITransitionsEx OnExit(Action exitAction);
    }

    /// <inheritdoc cref="IExitAction"/>
    public interface IExitActionEx : IExitAction
    {
      /// <summary>
      /// Specifies an action with an argument to be executed upon exiting the state.
      /// </summary>
      /// <typeparam name="TArgument">The type of the argument.</typeparam>
      /// <param name="exitAction">The action to execute.</param>
      /// <returns>An <see cref="ITransitionsEx"/> instance for configuring possible transitions.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="exitAction"/> is null.</exception>
      /// <remarks>Refer to <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> for argument usage details.</remarks>
      ITransitions<TArgument> OnExit<TArgument>(Action<TArgument> exitAction);
    }

    /// <inheritdoc cref="IExitAction"/>
    public interface IExitAction<out TArgument> : ITransitions<TArgument>
    {
      /// <inheritdoc cref="IExitActionEx.OnExit"/>
      ITransitions<TArgument> OnExit(Action exitAction);

      /// <inheritdoc cref="IExitActionEx.OnExit"/>
      ITransitions<TArgument> OnExit(Action<TArgument> exitAction);
    }
  }
}