using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate;

internal static class Throw
{
  [DoesNotReturn]
  public static void NoArgumentForTuple<TState, TEvent>(IState<TState, TEvent> state, Type argType)
    => throw new InvalidOperationException(
         $"State to be activated {{up.Id}} requires tuple argument with argument part of type '{argType}',"
       + $"but no argument was specified in {nameof(IStateMachine<TState, TEvent>.Raise)} method and no suitable argument was"
       + " found in any currently active state."
       );

  [DoesNotReturn]
  public static void NoArgument<TState, TEvent>(IState<TState, TEvent> state, Type argumentType)
    => throw new InvalidOperationException(
         $"The state '{state.Id}' requires argument of type '{argumentType}' but no argument of compatible type has passed to "
       + $"the {nameof(IStateMachine<TState, TEvent>.Raise)} method and no compatible argument is found in the currently active states"
       );
}