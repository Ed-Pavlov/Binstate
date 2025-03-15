using System;
using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

internal static class Throw
{
  [DoesNotReturn]
  public static void NoArgument(IState state)
    => throw new TransitionException(
         $"The state '{state}' requires argument of type '{state.GetArgumentType()}' but no argument of compatible type has passed to "
       + $"the Raise(Async) method and no compatible argument is found in the currently active states."
       );

  [DoesNotReturn]
  public static void ParanoiaException(IState targetState)
    => ParanoiaException($"all verifications should be performed in the caller part. Target state = {targetState}");

  [DoesNotReturn]
  public static void ParanoiaException(string reason) => throw new InvalidOperationException("This exception should never be thrown, because " + reason);
}