using System;

namespace Binstate
{
  /// <summary>
  /// Internal data structure for (de)composing passed and relayed arguments 
  /// </summary>
  internal readonly struct MixOf<TA, TP>
  {
    public static readonly MixOf<TA, TP> Empty = new MixOf<TA, TP>();
    
    public MixOf(Maybe<TA> passedArgument, Maybe<TP> relayedArgument)
    {
      PassedArgument = passedArgument;
      RelayedArgument = relayedArgument;
      HasAnyArgument = passedArgument.HasValue || relayedArgument.HasValue;
    }

    public readonly Maybe<TA> PassedArgument;
    public readonly Maybe<TP> RelayedArgument;
    public readonly bool HasAnyArgument;

    // ReSharper disable once SimplifyConditionalTernaryExpression
    /// <summary>
    /// Checks if any of specified argument can be passed into the state's enter action represented by <paramref name="parameterType"/>  
    /// </summary>
    public bool IsMatch(Type parameterType) =>
      parameterType.IsAssignableFrom(typeof(ITuple<TA, TP>)) ? PassedArgument.HasValue && RelayedArgument.HasValue :
      parameterType.IsAssignableFrom(typeof(TA)) ? PassedArgument.HasValue :
      parameterType.IsAssignableFrom(typeof(TP)) ? RelayedArgument.HasValue : 
      false;
  }
}