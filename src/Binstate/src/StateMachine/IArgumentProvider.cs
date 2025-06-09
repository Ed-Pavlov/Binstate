using System;

namespace BeatyBit.Binstate;

internal interface IArgumentAware
{
  Type? GetArgumentTypeSafe();
}

internal interface IArgumentProvider : IArgumentAware;
internal interface IArgumentReceiver : IArgumentAware;

internal interface IGetArgument<out TArgument> : IArgumentProvider
{
  TArgument Argument { get; }
}

internal interface ISetArgument<in TArgument>
{
  TArgument Argument { set; }
}

internal class ArgumentProviderByValue<T> : IGetArgument<T>
{
  public ArgumentProviderByValue(T argument) => Argument = argument;

  public T Argument { get; }

  public Type GetArgumentTypeSafe() => typeof(T);
}