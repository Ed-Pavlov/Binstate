using System;

namespace BeatyBit.Binstate;

/// <inheritdoc />
public class StateMachineException : Exception
{
  /// <inheritdoc />
  protected StateMachineException(string message) : base(message) { }
}

/// <inheritdoc />
public class TransitionException : StateMachineException
{
  /// <inheritdoc />
  public TransitionException(string message) : base(message) { }

  internal static TransitionException NoArgumentException(IState state)
    => new TransitionException(
      $"The state '{state}' requires argument of type '{state.GetArgumentTypeSafe()}' but no argument of compatible type has passed to "
    + $"the Raise(Async) method and no compatible argument is found in the currently active states."
    );

  internal static TransitionException NoEventArgumentException<TEvent, TState, TArgument>(TEvent @event, TState sourceState, TState? targetState)
    => new TransitionException(
      $"The transition by event '{@event}' from '{sourceState}' {(targetState is null ? "" : $"to '{targetState}'")} requires argument of type '{typeof(TArgument)}' "
    + $"but no argument of compatible type has passed to the Raise(Async) method."
    );

  internal static Exception NoStateArgumentException<TEvent, TState, TArgument>(TEvent @event, TState sourceState, TState? targetState)
    where TEvent : notnull where TState : notnull
    => new TransitionException(
      $"The transition by event '{@event}'from '{sourceState}' {(targetState is null ? "" : $"to '{targetState}'")} requires argument of type '{typeof(TArgument)}' "
    + $"but no argument of compatible type is found in the currently active states."
    );
}