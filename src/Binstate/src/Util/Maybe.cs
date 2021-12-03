using System;

namespace Binstate;

internal readonly struct Maybe<T>
{
  public static readonly Maybe<T> Nothing = new();

  private readonly T _value;

  public Maybe(T value)
  {
    _value   = value;
    HasValue = true;
  }

  public bool HasValue { get; }

  public T Value => HasValue ? _value : throw new InvalidOperationException("No value");
}

internal static class Maybe
{
  public static Maybe<T> ToMaybe<T>(this T value) => new(value);
}