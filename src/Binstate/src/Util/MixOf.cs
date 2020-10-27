using System;

namespace Binstate
{
  /// <summary>
  /// Internal data structure for (de)composing passed and relayed arguments 
  /// </summary>
  internal readonly struct MixOf<TArgument, TRelay>
  {
    public static readonly MixOf<TArgument, TRelay> Empty = new MixOf<TArgument, TRelay>();
    
    public MixOf(Maybe<TArgument> passedArgument, Maybe<TRelay> relayedArgument)
    {
      PassedArgument = passedArgument;
      RelayedArgument = relayedArgument;
      HasAnyArgument = passedArgument.HasValue || relayedArgument.HasValue;
    }

    public readonly Maybe<TArgument> PassedArgument;
    public readonly Maybe<TRelay> RelayedArgument;
    public readonly bool HasAnyArgument;

    // ReSharper disable once SimplifyConditionalTernaryExpression
    /// <summary>
    /// Checks if any of specified argument can be passed into the state's enter action represented by <paramref name="parameterType"/>  
    /// </summary>
    public bool IsMatch(Type parameterType) =>
      parameterType.IsAssignableFrom(typeof(ITuple<TArgument, TRelay>)) ? PassedArgument.HasValue && RelayedArgument.HasValue :
      parameterType.IsAssignableFrom(typeof(TArgument)) ? PassedArgument.HasValue : // check passed argument first as it hase a priority 
      parameterType.IsAssignableFrom(typeof(TRelay)) ? RelayedArgument.HasValue : 
      false;
  }
}