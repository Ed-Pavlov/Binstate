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
}