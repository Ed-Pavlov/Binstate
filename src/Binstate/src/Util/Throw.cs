using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate;

internal static class Throw
{
  [DoesNotReturn]
  public static void NoArgumentForTuple<TState, TEvent>(IState<TState, TEvent> state, Type argType)
    => throw new InvalidOperationException(
         $"State to be activated {state.Id} requires tuple argument '{argType}',"
       + $"but no argument was specified in {nameof(IStateMachine<TEvent>.Raise)} method and no suitable argument was"
       + " found in any currently active state."
       );

  [DoesNotReturn]
  public static void NoArgument<TState, TEvent>(IState<TState, TEvent> state)
    => throw new InvalidOperationException(
         $"The state '{state.Id}' requires argument of type '{state.GetArgumentType()}' but no argument of compatible type has passed to "
       + $"the {nameof(IStateMachine<TEvent>.Raise)} method and no compatible argument is found in the currently active states"
       );

  [DoesNotReturn]
  public static void ImpossibleException(IState targetState)
    => throw new InvalidOperationException("This exception should never be thrown, because all verifications should be performed in the caller part. "
                                         + $"Target state = {targetState}");

}