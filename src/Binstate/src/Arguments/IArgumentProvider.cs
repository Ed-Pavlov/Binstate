using System;

namespace Binstate;

internal interface IArgumentProvider;

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
}

internal enum TuplePart { Invalid, Argument, Relay }

internal class OneTupleItemArgumentProvider<T, TA, TR> : IGetArgument<T>
{
  public OneTupleItemArgumentProvider(IState state)
  {
    var tuple = ( (IGetArgument<ITuple<TA, TR>>)state ).Argument;
    Argument = tuple switch
    {
      { ItemX: T arg }   => arg,
      { ItemY: T relay } => relay,
      _                   => throw new ArgumentOutOfRangeException(nameof(tuple))
    };
  }

  public T Argument { get; }
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