using System;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal interface IArgumentProvider
{
  Type? GetArgumentTypeSafe();
}

internal interface IGetArgument<out TArgument> : IArgumentProvider
{
  TArgument Argument { get; }
}

internal interface ISetArgument<in TArgument>
{
  TArgument Argument { set; }
}

internal class ArgumentProvider<T> : IGetArgument<T>
{
  public ArgumentProvider(T argument) => Argument = argument;

  public T Argument { get; }

  public Type? GetArgumentTypeSafe()
  {
    var argumentType = typeof(T);
    return argumentType == typeof(Unit) ? null : argumentType;
  }
}

internal class OneTupleItemArgumentProvider<T, TA, TR>(IState state)
  : ArgumentProvider<T>(GetArgument(( (IGetArgument<ITuple<TA, TR>>)state ).Argument)) // base class constructor
{
  private static T GetArgument(ITuple<TA, TR> tuple)
  {
    return tuple switch
    {
      { ItemX: T arg }   => arg,
      { ItemY: T relay } => relay,
      _                  => throw new ArgumentOutOfRangeException(nameof(tuple))
    };
  }
}

internal static class StateTupleArgumentProvider
{
  private static readonly Type GenericDefinition = typeof(OneTupleItemArgumentProvider<,,>);

  public static IArgumentProvider Create(Type t, Type ta, Type tr, IState state)
  {
    var type = GenericDefinition.MakeGenericType(t, ta, tr);
    var ctor = type.GetConstructors()[0];
    return (IArgumentProvider)ctor.Invoke([state]);
  }
}