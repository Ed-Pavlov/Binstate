using System;

namespace Binstate
{
  public struct MixOf<TA, TP>
  {
    public static readonly MixOf<TA, TP> Empty = new MixOf<TA, TP>();
    
    public MixOf(Maybe<TA> passedArgument, Maybe<TP> relayedArgument)
    {
      PassedArgument = passedArgument;
      RelayedArgument = relayedArgument;
      IsSpecified = passedArgument.HasValue || relayedArgument.HasValue;
    }

    public readonly Maybe<TA> PassedArgument;
    public readonly Maybe<TP> RelayedArgument;
    public readonly bool IsSpecified;
    
    // ReSharper disable once SimplifyConditionalTernaryExpression
    public bool IsMatch(Type parameterType, bool fullMatch = false) =>
      parameterType.IsAssignableFrom(typeof(ITuple<TA, TP>)) ? PassedArgument.HasValue && RelayedArgument.HasValue :
      parameterType.IsAssignableFrom(typeof(TA)) ? PassedArgument.HasValue :
      parameterType.IsAssignableFrom(typeof(TP)) ? RelayedArgument.HasValue : 
      false;
  }
}