using System.Collections.Generic;

namespace Binstate;

/// <summary>
///  //TODO:
/// </summary>
public interface ITuple { }

/// <summary>
///   This data structure is used if a state needs to accept two arguments at once,
///   usually one is passed to <see cref="IStateMachine{TState,TEvent}.Raise{T}" /> method and the second one is obtained from the previously active
///   states during transition automatically. But they could be both from the active states.
///
///   Interface is used to make argument types invariant in order to pass arguments of compatible types.
/// </summary>
public interface ITuple<out TPassed, out TRelay> : ITuple
{
  /// <summary>
  ///   Passed argument value
  /// </summary>
  TPassed PassedArgument { get; }

  /// <summary>
  ///   Relayed argument value
  /// </summary>
  TRelay RelayedArgument { get; }
}

/// <inheritdoc />
public class Tuple<TPassed, TRelay> : ITuple<TPassed, TRelay>
{
  /// <summary />
  public Tuple(TPassed passedArgument, TRelay relayedArgument)
  {
    PassedArgument  = passedArgument;
    RelayedArgument = relayedArgument;
  }

  /// <inheritdoc />
  public TPassed PassedArgument { get; }

  /// <inheritdoc />
  public TRelay RelayedArgument { get; }

  private bool Equals(ITuple<TPassed, TRelay>? other)
    => other is not null
    && EqualityComparer<TPassed>.Default.Equals(PassedArgument, other.PassedArgument)
    && EqualityComparer<TRelay>.Default.Equals(RelayedArgument, other.RelayedArgument);

  /// <remarks> Equals doesnt check exact type of other object, only if it can be cast to <see cref="ITuple{TPassed,TRelay}" /> </remarks>
  public override bool Equals(object? obj)
  {
    if(ReferenceEquals(null, obj)) return false;
    if(ReferenceEquals(this, obj)) return true;

    return Equals(obj as ITuple<TPassed, TRelay>);
  }

  /// <summary />
  public override int GetHashCode()
  {
    unchecked
    {
      return ( EqualityComparer<TPassed>.Default.GetHashCode(PassedArgument) * 397 ) ^ EqualityComparer<TRelay>.Default.GetHashCode(RelayedArgument);
    }
  }
}