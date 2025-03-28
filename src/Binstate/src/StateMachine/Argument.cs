using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BeatyBit.Binstate;

/// <summary>
/// This class contains tools and logic for setting arguments to their consumers.
/// </summary>
internal static partial class Argument
{
  private static readonly MethodInfo SetArgumentMethodFactory
    = typeof(Argument).GetMethod(nameof(SetArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

  private static readonly MethodInfo SetTupleArgumentMethodFactory
    = typeof(Argument).GetMethod(nameof(SetTupleArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

  private static readonly Type TupleInterfaceTypeDefinition = typeof(ITuple<,>);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Type GetArgumentType(this IArgumentProvider argumentProvider)
    => argumentProvider.GetArgumentTypeSafe() ?? throw Paranoia.GetException("this method should be called only for states with arguments");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRequireArgument(this IState state) => state.GetArgumentTypeSafe() is not null;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CanAcceptArgumentFrom(this IState argumentTarget, IState argumentSource)
    => argumentTarget.GetArgumentType().IsAssignableFrom(argumentSource.GetArgumentTypeSafe());

  private static void SetArgument<T>(ISetArgument<T> target, IGetArgument<T> source) => target.Argument = source.Argument;

  private static void SetTupleArgument<TX, TY>(ISetArgument<ITuple<TX, TY>> target, IGetArgument<TX> providerX, IGetArgument<TY> providerY)
    => target.Argument = new Tuple<TX, TY>(providerX.Argument, providerY.Argument);

  /// <summary>
  /// Validation of the argument and target type should be performed on the caller side
  /// </summary>
  public static void SetArgumentByReflectionUnsafe(IState target, Type targetArgumentType, object? argument)
  {
    var setArgumentMethod = SetArgumentMethodFactory.MakeGenericMethod(targetArgumentType);
    setArgumentMethod.Invoke(null, [target, argument]);
  }

  private static void SetArgumentByReflection(IState target, ITuple<IArgumentProvider, IArgumentProvider?> tuple)
  {
    var targetArgumentType  = target.GetArgumentType();
    var source1ArgumentType = tuple.ItemX.GetArgumentType();

    // target argument can accept PassedArgument, so should be set, and we are good
    if(targetArgumentType.IsAssignableFrom(source1ArgumentType))
    {
      if(tuple.ItemY is not null) throw Paranoia.GetInvalidTargetException(target);

      var passArgumentMethod = SetArgumentMethodFactory.MakeGenericMethod(targetArgumentType);
      passArgumentMethod.Invoke(null, [target, tuple.ItemX]);
      return;
    }

    // target argument is ITuple, pass both providers
    if(targetArgumentType.IsTuple(out var typeX, out var typeY))
    {
      var source2ArgumentType = tuple.ItemY?.GetArgumentType();
      if(source2ArgumentType is null) throw Paranoia.GetInvalidTargetException(target);

      var passTupleArgumentMethod = SetTupleArgumentMethodFactory.MakeGenericMethod(typeX, typeY);
      passTupleArgumentMethod.Invoke(null, [target, tuple.ItemX, tuple.ItemY!]);
    }
  }

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