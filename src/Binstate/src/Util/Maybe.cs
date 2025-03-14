using System;

namespace BeatyBit.Binstate;

internal readonly struct Maybe<T>
{
  public static readonly Maybe<T> Nothing = new Maybe<T>();

  public Maybe(T value)
  {
    Value   = value;
    HasValue = true;
  }

  public bool HasValue { get; }

  public T Value => HasValue ? field : throw new InvalidOperationException("No value");
}

internal static class Maybe
{
  public static Maybe<T> ToMaybe<T>(this T value) => new Maybe<T>(value);
}