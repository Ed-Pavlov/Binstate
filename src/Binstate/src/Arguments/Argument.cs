using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Binstate;

internal static class Argument
{
  private static readonly MethodInfo RelayMethod                  = typeof(Argument).GetMethod(nameof(Relay), BindingFlags.NonPublic | BindingFlags.Static)!;
  private static readonly MethodInfo RelayToTupleArgumentMethod   = typeof(Argument).GetMethod(nameof(RelayToTupleArgument), BindingFlags.NonPublic | BindingFlags.Static)!;
  private static readonly MethodInfo RelayFromTupleArgumentMethod = typeof(Argument).GetMethod(nameof(RelayFromTupleArgument), BindingFlags.NonPublic | BindingFlags.Static)!;
  private static readonly MethodInfo RelayFromTupleRelayMethod    = typeof(Argument).GetMethod(nameof(RelayFromTupleRelay), BindingFlags.NonPublic | BindingFlags.Static)!;
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
  public static Type GetArgumentType(this IArgumentProvider argumentProvider)
    => argumentProvider.GetType().GetInterface(typeof(IGetArgument<>).Name).GetGenericArguments()[0];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CanAcceptArgumentFrom(this IState argumentTarget, IState argumentSource)
    => argumentTarget.GetArgumentType().IsAssignableFrom(argumentSource.GetArgumentType());

  public static bool IsTuple(this Type type) => typeof(ITuple).IsAssignableFrom(type);

  private static bool IsTuple(this Type type, [NotNullWhen(true)] out Type? argumentType, [NotNullWhen(true)] out Type? relayType)
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

  public static void SetArgumentByReflection(IState target, ITuple<IArgumentProvider, IArgumentProvider?> tuple)
  {
    var targetArgumentType  = target.GetArgumentType();
    var source1ArgumentType = tuple.PassedArgument.GetArgumentType();

    // target is simple -> then only PassedArgument should be set and be good
    if(targetArgumentType.IsAssignableFrom(source1ArgumentType))
    {
      if(tuple.RelayedArgument is not null) Throw.ImpossibleException(target);

      var method = RelayMethod.MakeGenericMethod(targetArgumentType);
      method.Invoke(null, new object[] { target, tuple.PassedArgument });
      return;
    }

    // target argument is ITuple, pass both providers
    var source2ArgumentType = tuple.RelayedArgument?.GetArgumentType();
    if(targetArgumentType.IsTuple(out var argType, out var relayType))
    {
      if(source2ArgumentType is null) Throw.ImpossibleException(target);

      var method = RelayToTupleArgumentMethod.MakeGenericMethod(argType, relayType);
      method.Invoke(null, new object[] { target, tuple.PassedArgument, tuple.RelayedArgument! });
      return;
    }

//    if(source1ArgumentType.IsTuple(out var arg, out var relay))
//    {
//      if(targetArgumentType.IsAssignableFrom(arg))
//      {
//        var relayFromTupleMethod = RelayFromTupleArgumentMethod.MakeGenericMethod(arg, relay);
//        relayFromTupleMethod.Invoke(null, new object[] { target, tuple });
//      }
//      else if(targetArgumentType.IsAssignableFrom(relay))
//      {
//        var relayFromTupleMethod = RelayFromTupleRelayMethod.MakeGenericMethod(arg, relay);
//        relayFromTupleMethod.Invoke(null, new object[] { target, tuple });
//      }
//      else
//        Throw.ImpossibleException(target);
//    }
//    else
//      Throw.ImpossibleException(target);

//    SetArgumentByReflection(target, tuple.PassedArgument);
  }

  private static void SetArgumentByReflection(IState target, IArgumentProvider source)
  {
    var targetArgumentType = target.GetArgumentType();
    var sourceArgumentType = source.GetArgumentType();

    if(sourceArgumentType.IsTuple(out var tupleArg, out var tupleRelay))
    {
      if(targetArgumentType.IsAssignableFrom(tupleArg))
      {
        var relayFromTupleMethod = RelayFromTupleArgumentMethod.MakeGenericMethod(tupleArg, tupleRelay);
        relayFromTupleMethod.Invoke(null, new object[] { target, source });
      }
      else if(targetArgumentType.IsAssignableFrom(tupleRelay))
      {
        var relayFromTupleMethod = RelayFromTupleRelayMethod.MakeGenericMethod(tupleArg, tupleRelay);
        relayFromTupleMethod.Invoke(null, new object[] { target, source });
      }
      else
        throw new ArgumentOutOfRangeException();
    }
    else
      throw new ArgumentOutOfRangeException(nameof(source), $"Argument from {source} should be suitable for target in any way");
  }

  private static void Relay<T>(ISetArgument<T> target, IGetArgument<T> source) => target.Argument = source.Argument;

  private static void RelayToTupleArgument<TA, TR>(ISetArgument<ITuple<TA, TR>> target, IGetArgument<TA> sourceA, IGetArgument<TR> sourceR)
    => target.Argument = new Tuple<TA, TR>(sourceA.Argument, sourceR.Argument);

  private static void RelayFromTupleArgument<TA, TR>(ISetArgument<TA> target, IGetArgument<ITuple<TA, TR>> source)
    => target.Argument = source.Argument.PassedArgument;

  private static void RelayFromTupleRelay<TA, TR>(ISetArgument<TR> target, IGetArgument<ITuple<TA, TR>> source)
    => target.Argument = source.Argument.RelayedArgument;

  public class WithCache
  {
    private readonly Dictionary<Type, ITuple<IArgumentProvider, IArgumentProvider?>> _argumentSourcesCache  = new();
    private readonly Dictionary<Type, IArgumentProvider>                             _argumentProviderCache = new();

    public bool GetArgumentProviders<TArgument, TState, TEvent>(
      Type                                                                   targetArgumentType,
      TArgument                                                              argument,
      bool                                                                   argumentHasPriority,
      IState<TState, TEvent>                                                 rootState,
      [NotNullWhen(true)] out ITuple<IArgumentProvider, IArgumentProvider?>? providers)
    {
      if(_argumentSourcesCache.TryGetValue(targetArgumentType, out providers))
        return true;

      var passedArgumentType = typeof(TArgument);

      if(targetArgumentType.IsAssignableFrom(typeof(TArgument)) && argumentHasPriority)
      {
        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(new ArgumentProvider<TArgument>(argument), null);
        _argumentSourcesCache.Add(targetArgumentType, providers);
        return true;
      }

      if(targetArgumentType.IsTuple(out var argArgType, out var argRelayType))
      {
        GetArgumentProviderForSingleArgument(rootState, argArgType,   out var argArgProvider);
        GetArgumentProviderForSingleArgument(rootState, argRelayType, out var argRelayProvider);

        if(argArgProvider is null)
          if(argArgType.IsAssignableFrom(passedArgumentType))
            argArgProvider = new ArgumentProvider<TArgument>(argument);

        if(argRelayProvider is null)
          if(argRelayType.IsAssignableFrom(passedArgumentType))
            argRelayProvider =new ArgumentProvider<TArgument>(argument);

        if(argArgProvider is null || argRelayProvider is null)
          return false;

        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(argArgProvider, argRelayProvider);
        _argumentSourcesCache.Add(targetArgumentType, providers);
        return true;
      }

      if(! GetArgumentProviderForSingleArgument(rootState, targetArgumentType, out var provider))
        if(!targetArgumentType.IsAssignableFrom(typeof(TArgument)))
          return false;
        else // use fallback value but not add it to the cache
        {
          providers = new Tuple<IArgumentProvider, IArgumentProvider?>(new ArgumentProvider<TArgument>(argument), null);
          return true;
        }

      providers = new Tuple<IArgumentProvider, IArgumentProvider?>(provider, null);
      _argumentSourcesCache.Add(targetArgumentType, providers);
      return true;
    }

    private bool GetArgumentProviderForSingleArgument<TState, TEvent>(
      IState<TState, TEvent>                     sourceRoot,
      Type                                       targetArgumentType,
      [NotNullWhen(true)] out IArgumentProvider? provider)
    {
      if(_argumentProviderCache.TryGetValue(targetArgumentType, out provider))
        return true;

      provider = null;
      var state = sourceRoot;
      while(state is not null)
      {
        var stateArgumentType = state.GetArgumentTypeSafe();

        if(stateArgumentType is not null)
        {
          if(targetArgumentType.IsAssignableFrom(stateArgumentType))
          {
            _argumentProviderCache.Add(targetArgumentType, state);
            provider = state;
            return true;
          }

          if(stateArgumentType.IsTuple(out var tupleArg, out var tupleRelay)
          && (
               targetArgumentType.IsAssignableFrom(tupleArg)
            || targetArgumentType.IsAssignableFrom(tupleRelay)
             ))
          {
            provider = StateTupleArgumentProvider.Create(targetArgumentType, tupleArg, tupleRelay, state);
            _argumentProviderCache.Add(targetArgumentType, provider);
            return true;
          }
        }

        state = state.ParentState;
      }

      return false;
    }
  }
}