using System.Collections.Generic;

namespace Binstate;


/// <summary>
///   This data structure is used if a state needs to accept two arguments at once,
///   usually one is passed to <see cref="IStateMachine{TEvent}.Raise{T}" /> method and the second one is obtained from the previously active
///   states during transition automatically. But they could be both from the active states.
///
///   Interface is used to make argument types invariant in order to pass arguments of compatible types.
/// </summary>
public interface ITuple<out TX, out TY>
{
  /// <summary />
  TX ItemX { get; }

  /// <summary />
  TY ItemY { get; }
}

/// <inheritdoc />
public class Tuple<TX, TY> : ITuple<TX, TY>
{
  /// <summary />
  public Tuple(TX x, TY y)
  {
    ItemX  = x;
    ItemY = y;
  }

  /// <inheritdoc />
  public TX ItemX { get; }

  /// <inheritdoc />
  public TY ItemY { get; }

  private bool Equals(ITuple<TX, TY>? other)
    => other is not null
    && EqualityComparer<TX>.Default.Equals(ItemX, other.ItemX)
    && EqualityComparer<TY>.Default.Equals(ItemY, other.ItemY);

  /// <remarks> Equals doesnt check exact type of other object, only if it can be cast to <see cref="ITuple{TPassed,TRelay}" /> </remarks>
  public override bool Equals(object? obj)
  {
    if(ReferenceEquals(null, obj)) return false;
    if(ReferenceEquals(this, obj)) return true;

    return Equals(obj as ITuple<TX, TY>);
  }

  /// <summary />
  public override int GetHashCode()
  {
    unchecked
    {
      return ( EqualityComparer<TX>.Default.GetHashCode(ItemX) * 397 ) ^ EqualityComparer<TY>.Default.GetHashCode(ItemY);
    }
  }
}