using System;

namespace Binstate
{
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
  }
}
