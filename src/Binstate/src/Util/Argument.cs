using System;
using System.Runtime.CompilerServices;

namespace Binstate;

internal static class Argument
{
  public static bool IsSpecified<T>()                     => typeof(Unit) != typeof(T);
  public static bool IsRequireArgument(this IState state) => state.GetArgumentTypeSafe() is not null;

  public static bool CanAcceptArgumentFrom(this IState argumentTarget, IState argumentSource)
    => argumentTarget.GetArgumentType().IsAssignableFrom(argumentSource.GetArgumentType());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Type? GetArgumentTypeSafe(this IState state)
  {
    var type = state.GetType().GetGenericArguments()[2];
    return type != typeof(Unit) ? type : null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Type GetArgumentType(this IState state) => state.GetArgumentTypeSafe() ?? throw new ArgumentException();
}