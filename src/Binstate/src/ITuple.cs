using System.Collections.Generic;

namespace BeatyBit.Binstate;

/// <summary>
/// This data structure is used to pass two arguments where it's necessary,
/// Usually one is passed to <see cref="IStateMachine{TEvent}.Raise{T}" /> method and the second one is obtained from one of the previously active
/// states e.g., to pass them to a transition action and/or selector.
/// But they could be both from the active states, for example, if a state requires two arguments of corresponding types.
///
/// Interface is used to make argument types invariant to pass arguments of compatible types.
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
  public Tuple(TX arg1, TY arg2)
  {
    ItemX = arg1;
    ItemY = arg2;
  }

  /// <inheritdoc />
  public TX ItemX { get; }

  /// <inheritdoc />
  public TY ItemY { get; }

  private bool Equals(ITuple<TX, TY>? other)
    => other is not null
    && EqualityComparer<TX>.Default.Equals(ItemX, other.ItemX)
    && EqualityComparer<TY>.Default.Equals(ItemY, other.ItemY);

  /// <remarks> Equals doesn't check the exact type of the other object, only if it can be cast to <see cref="ITuple{TX,TY}" /> </remarks>
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
      return ( EqualityComparer<TX>.Default.GetHashCode(ItemX) * 397 )
           ^ EqualityComparer<TY>.Default.GetHashCode(ItemY);
    }
  }

  /// <inheritdoc />
  public override string ToString() => $"{{{nameof(ItemX)}: {ItemX}, {nameof(ItemY)}:{ItemY}}}";

  /// <summary>
  /// Deconstructs the tuple into its individual components.
  /// </summary>
  public void Deconstruct(out TX x, out TY y)
  {
    x = ItemX;
    y = ItemY;
  }
}

/// <inheritdoc cref="System.Tuple"/>
public static class ArgumentsTuple
{
  /// <inheritdoc cref="System.Tuple.Create{TX, TY}"/>
  public static Tuple<TX, TY> Create<TX, TY>(TX x, TY y) => new(x, y);
}