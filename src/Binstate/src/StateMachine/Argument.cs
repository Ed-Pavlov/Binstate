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

  private static readonly MethodInfo PropagateArgumentMethodFactory
    = typeof(Argument).GetMethod(nameof(PropagateArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

  private static readonly MethodInfo SetTupleArgumentMethodFactory
    = typeof(Argument).GetMethod(nameof(SetTupleArgument), BindingFlags.NonPublic | BindingFlags.Static)!;

  private static readonly Type TupleInterfaceTypeDefinition = typeof(ITuple<,>);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Type GetArgumentType(this IArgumentAware argumentAware)
    => argumentAware.GetArgumentTypeSafe() ?? throw Paranoia.GetException("this method should be called only for states with arguments");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsRequireArgument(this IState state) => state.GetArgumentTypeSafe() is not null;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CanAcceptArgumentFrom(this IArgumentReceiver receiver, IArgumentProvider provider)
    => receiver.GetArgumentType().IsAssignableFrom(provider.GetArgumentTypeSafe());

  private static void SetArgument<T>(ISetArgument<T> target, T argument) => target.Argument = argument;

  private static void PropagateArgument<T>(ISetArgument<T> target, IGetArgument<T> source) => target.Argument = source.Argument;

  private static void SetTupleArgument<TX, TY>(ISetArgument<ITuple<TX, TY>> target, IGetArgument<TX> providerX, IGetArgument<TY> providerY)
    => target.Argument = new Tuple<TX, TY>(providerX.Argument, providerY.Argument);

  /// <summary>
  /// Validation of the argument and target type should be performed on the caller side
  /// </summary>
  public static void SetArgumentByReflectionUnsafe(IArgumentReceiver receiver, Type argumentType, object? argument)
  {
    var setArgumentMethod = SetArgumentMethodFactory.MakeGenericMethod(argumentType);
    setArgumentMethod.Invoke(null, [receiver, argument]);
  }

  private static void SetArgumentByReflection(IArgumentReceiver receiver, ITuple<IArgumentProvider, IArgumentProvider?> argumentsTuple)
  {
    var receiverArgumentType = receiver.GetArgumentType();
    var passedArgumentType   = argumentsTuple.ItemX.GetArgumentType();

    // target argument can accept PassedArgument, so should be set, and we are good
    if(receiverArgumentType.IsAssignableFrom(passedArgumentType))
    {
      if(argumentsTuple.ItemY is not null) throw Paranoia.GetInvalidTargetException(receiver);

      var passArgumentMethod = PropagateArgumentMethodFactory.MakeGenericMethod(receiverArgumentType);
      passArgumentMethod.Invoke(null, [receiver, argumentsTuple.ItemX]);
      return;
    }

    // the receiver argument type is ITuple, pass both providers
    if(receiverArgumentType.IsTuple(out var typeX, out var typeY))
    {
      var stateArgumentType = argumentsTuple.ItemY?.GetArgumentType();
      if(stateArgumentType is null) throw Paranoia.GetInvalidTargetException(receiver);

      var passTupleArgumentMethod = SetTupleArgumentMethodFactory.MakeGenericMethod(typeX, typeY);
      passTupleArgumentMethod.Invoke(null, [receiver, argumentsTuple.ItemX, argumentsTuple.ItemY!]);
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