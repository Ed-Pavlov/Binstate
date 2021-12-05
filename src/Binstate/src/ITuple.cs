using System.Collections.Generic;

namespace Binstate;

/// <summary>
///   Interface is used to make argument types invariant in order to pass arguments of compatible types
/// </summary>
/// <typeparam name="TPassed"> Type of argument passed to <see cref="IStateMachine{TState,TEvent}.Raise{T}" /> method </typeparam>
/// <typeparam name="TRelay">
///   Type of the argument attached to one of the currently active states
///   and passed to <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}()" /> method.
/// </typeparam>
public interface ITuple<out TPassed, out TRelay>
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

  private bool Equals(ITuple<TPassed, TRelay>? other) => other is not null
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