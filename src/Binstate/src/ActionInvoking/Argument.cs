using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Binstate;

internal static class Argument
{
  private static readonly MethodInfo RelayMethodInfo              = typeof(Argument).GetMethod(nameof(Relay))!;
  private static readonly MethodInfo RelayFromTupleArgumentMethod = typeof(Argument).GetMethod(nameof(RelayFromTupleArgument))!;
  private static readonly MethodInfo RelayFromTupleRelayMethod    = typeof(Argument).GetMethod(nameof(RelayFromTupleRelay))!;
  private static readonly Type       TupleType                    = typeof(ITuple);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSpecified<T>() => typeof(Unit) != typeof(T);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRequireArgument(this IState state) => state.GetArgumentTypeSafe() is not null;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Type? GetArgumentTypeSafe(this IState state)
  {
    var type = state.GetType().GetGenericArguments()[2];
    return type != typeof(Unit) ? type : null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Type GetArgumentType(this IState state) => state.GetArgumentTypeSafe() ?? throw new ArgumentException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CanAcceptArgumentFrom(this IState argumentTarget, IState argumentSource)
    => argumentTarget.GetArgumentType().IsAssignableFrom(argumentSource.GetArgumentType());

  public static bool IsTuple(this Type type, out Type? argumentType, out Type? relayType)
  {
    if(! TupleType.IsAssignableFrom(type))
    {
      argumentType = null;
      relayType    = null;
      return false;
    }

    var genericArguments = type.GetGenericArguments();
    argumentType = genericArguments[0];
    relayType    = genericArguments[1];
    return true;
  }

  public static void SetArgument<T>(IState state, T argument) => ( (IState<T>)state ).Argument = argument;

  public static void SetArgumentByReflection(IState source, IState target)
  {
    var targetArgumentType = target.GetArgumentType();
    var sourceArgumentType = source.GetArgumentType();

    if(targetArgumentType.IsAssignableFrom(sourceArgumentType))
    {
      var method = RelayMethodInfo.MakeGenericMethod(targetArgumentType);
      method.Invoke(null, new object[] { source, target });
    }
    else if(sourceArgumentType.IsTuple(out var tupleArg, out var tupleRelay))
    {
      if(targetArgumentType.IsAssignableFrom(tupleArg))
      {
        var relayFromTupleMethod = RelayFromTupleArgumentMethod.MakeGenericMethod(tupleArg, tupleRelay);
        relayFromTupleMethod.Invoke(null, new object[] { source, target });
      }
      else if(targetArgumentType.IsAssignableFrom(tupleRelay))
      {
        var relayFromTupleMethod = RelayFromTupleRelayMethod.MakeGenericMethod(tupleArg, tupleRelay);
        relayFromTupleMethod.Invoke(null, new object[] { source, target });
      }
      else
        throw new ArgumentOutOfRangeException();
    }
    else
      throw new ArgumentOutOfRangeException(nameof(source), $"Argument from {source} should be suitable for target in any way");
  }

  private static void Relay<T>(IState<T> source, IState<T> target) => target.Argument = source.Argument;

  private static void RelayFromTupleArgument<TA, TR>(IState source, IState<TA> target)
  {
    var typedTuple = ( (IState<ITuple<TA, TR>>)source ).Argument;
    target.Argument = typedTuple.PassedArgument;
  }

  private static void RelayFromTupleRelay<TA, TR>(IState source, IState<TR> target)
  {
    var typedTuple = ( (IState<ITuple<TA, TR>>)source ).Argument;
    target.Argument = typedTuple.RelayedArgument;
  }

  public class WithCache
  {
    private readonly Dictionary<Type, IState> _argumentSourceCache = new();

    public bool GetArgumentSource<TState, TEvent>(IState<TState, TEvent> sourceRoot, Type argumentType, [NotNullWhen(true)] out IState? source)
    {
      if(_argumentSourceCache.TryGetValue(argumentType, out source))
        return true;

      source = null;
      var sourceState = sourceRoot;
      while(sourceState is not null)
      {
        var sourceType = sourceState.GetArgumentTypeSafe();

        if(sourceType is not null
        && ( argumentType.IsAssignableFrom(sourceType)
           ||
             ( sourceType.IsTuple(out var tupleArg, out var tupleRelay)
            && (
                 argumentType.IsAssignableFrom(tupleArg)
              || argumentType.IsAssignableFrom(tupleRelay)
               )
             )
           )
          )
        {
          _argumentSourceCache.Add(argumentType, sourceRoot);
          source = sourceState;
          return true;
        }

        sourceState = sourceState.ParentState;
      }

      return false;
    }
  }
}