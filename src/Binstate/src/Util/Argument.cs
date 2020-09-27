namespace Binstate
{
  internal static class Argument
  {
    public static bool IsSpecified<T>() => typeof(T) != typeof(Unit);
  }
}