using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BeatyBit.Binstate;

/// <summary>
/// This class contains tools and logic for passing arguments to their consumers.
/// </summary>
internal static partial class Argument
{
  private static readonly string IGetArgumentInterfaceName = typeof(IGetArgument<>).Name;

  private static readonly MethodInfo PassArgumentMethodFactory
    = typeof(Argument).GetMethod(nameof(PassArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

  private static readonly MethodInfo PassTupleArgumentMethodFactory
    = typeof(Argument).GetMethod(nameof(PassTupleArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

  private static readonly MethodInfo GetArgumentFactory = typeof(IGetArgument<>).GetProperty(nameof(IGetArgument<object>.Argument))!.GetGetMethod();

  private static readonly Type TupleInterfaceTypeDefinition = typeof(ITuple<,>);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Type GetArgumentType(this IState state) => state.GetArgumentTypeSafe() ?? throw new ArgumentException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRequireArgument(this IState state) => state.GetArgumentTypeSafe() is not null;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CanAcceptArgumentFrom(this IState argumentTarget, IState argumentSource)
    => argumentTarget.GetArgumentType().IsAssignableFrom(argumentSource.GetArgumentTypeSafe());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Type GetArgumentType(this IArgumentProvider argumentProvider)
    => argumentProvider.GetType().GetInterface(IGetArgumentInterfaceName).GetGenericArguments()[0];

  public static bool GetArgumentByReflection(this IState target, out object? argument)
  {
    argument = null;

    var targetArgumentType = target.GetArgumentTypeSafe();
    if(targetArgumentType is null)
      return false;

    argument = GetArgumentFactory.MakeGenericMethod(targetArgumentType).Invoke(target, null);
    return true;
  }

  /// <summary>
  /// Validation of the argument should be performed on the caller side
  /// </summary>
  public static void SetArgumentByReflectionUnsafe(IState target, object? argument)
  {
    var targetArgumentType = target.GetArgumentTypeSafe();
    var passArgumentMethod = PassArgumentMethodFactory.MakeGenericMethod(targetArgumentType);
    passArgumentMethod.Invoke(null, [target, argument]);
  }

  private static void SetArgumentByReflection(IState target, ITuple<IArgumentProvider, IArgumentProvider?> tuple)
  {
    var targetArgumentType  = target.GetArgumentType();
    var source1ArgumentType = tuple.ItemX.GetArgumentType();

    // target argument can accept PassedArgument, so should be set, and we are good
    if(targetArgumentType.IsAssignableFrom(source1ArgumentType))
    {
      if(tuple.ItemY is not null) Throw.ParanoiaException(target);

      var passArgumentMethod = PassArgumentMethodFactory.MakeGenericMethod(targetArgumentType);
      passArgumentMethod.Invoke(null, [target, tuple.ItemX]);
      return;
    }

    // target argument is ITuple, pass both providers
    if(targetArgumentType.IsTuple(out var typeX, out var typeY))
    {
      var source2ArgumentType = tuple.ItemY?.GetArgumentType();
      if(source2ArgumentType is null) Throw.ParanoiaException(target);

      var passTupleArgumentMethod = PassTupleArgumentMethodFactory.MakeGenericMethod(typeX, typeY);
      passTupleArgumentMethod.Invoke(null, [target, tuple.ItemX, tuple.ItemY!]);
    }
  }

  private static void PassArgument<T>(ISetArgument<T> target, IGetArgument<T> source) => target.Argument = source.Argument;

  private static void PassTupleArgument<TX, TY>(ISetArgument<ITuple<TX, TY>> target, IGetArgument<TX> providerX, IGetArgument<TY> providerY)
    => target.Argument = new Tuple<TX, TY>(providerX.Argument, providerY.Argument);

  private static bool IsTuple(this Type type, [NotNullWhen(true)] out Type? typeX, [NotNullWhen(true)] out Type? typeY)
  {
    if(type.IsGenericType && type.GetGenericTypeDefinition() == TupleInterfaceTypeDefinition)
    {
      var genericArguments = type.GetGenericArguments();
      typeX = genericArguments[0];
      typeY = genericArguments[1];
      return true;
    }

    typeX = null;
    typeY = null;
    return false;
  }
}