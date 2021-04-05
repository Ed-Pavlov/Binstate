namespace Binstate
{
  internal readonly struct Maybe<T>
  {
    public static readonly Maybe<T> Nothing = new();

    public Maybe(T? value)
    {
      Value    = value;
      HasValue = true;
    }

    public bool HasValue { get; }
    public T?   Value    { get; }
  }

  internal static class Maybe
  {
    public static Maybe<T> ToMaybe<T>(this T? value) => new(value);
  }
}
