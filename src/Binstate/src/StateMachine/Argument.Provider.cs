using System;

namespace BeatyBit.Binstate;

internal static partial class Argument
{
  public static readonly Tuple<IArgumentProvider?, IArgumentProvider?> NoTransitionArguments = new(null, null);

  public static readonly Type ArgumentProviderFromTupleDynamicFactory = typeof(ArgumentProviderFromTupleDynamic<,,>);
  public static readonly Type ArgumentProviderFromTupleXFactory       = typeof(ArgumentProviderFromTupleX<,,>);
  public static readonly Type ArgumentProviderFromTupleYFactory       = typeof(ArgumentProviderFromTupleY<,,>);

  internal class ArgumentProviderFromTupleDynamic<T, TA, TR>(IState state)
    : ArgumentProviderByValue<T>(GetArgument(( (IGetArgument<ITuple<TA, TR>>)state ).Argument)) // base class constructor
  {
    private static T GetArgument(ITuple<TA, TR> argumentsTuple)
    {
      return argumentsTuple switch
      {
        { ItemX: T arg }       => arg,
        { ItemY: T propagate } => propagate,
        _                      => throw new ArgumentOutOfRangeException(nameof(argumentsTuple))
      };
    }
  }

  internal class ArgumentProviderFromTupleX<T, TA, TR>(IState state)
    : ArgumentProviderByValue<T>(GetArgument(( (IGetArgument<ITuple<TA, TR>>)state ).Argument)) // base class constructor
  {
    private static T GetArgument(ITuple<TA, TR> argumentsTuple)
    {
      return argumentsTuple switch
      {
        { ItemX: T arg } => arg,
        _                => throw new ArgumentOutOfRangeException(nameof(argumentsTuple))
      };
    }
  }

  internal class ArgumentProviderFromTupleY<T, TA, TR>(IState state)
    : ArgumentProviderByValue<T>(GetArgument(( (IGetArgument<ITuple<TA, TR>>)state ).Argument)) // base class constructor
  {
    private static T GetArgument(ITuple<TA, TR> argumentsTuple)
    {
      return argumentsTuple switch
      {
        { ItemY: T propagate } => propagate,
        _                      => throw new ArgumentOutOfRangeException(nameof(argumentsTuple))
      };
    }
  }

  public static IArgumentProvider CreateArgumentProvider(this Type factory, Type t, Type ta, Type tp, IState state)
  {
    var argumentProviderType = factory.MakeGenericType(t, ta, tp);
    var ctor                 = argumentProviderType.GetConstructors()[0];
    return (IArgumentProvider)ctor.Invoke([state]);
  }
}