using BeatyBit.Bits;

namespace BeatyBit.Binstate;

/// <summary>
///
/// </summary>
public static class Is
{
  /// <summary>
  ///
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public static bool Unit<T>() => typeof(T) == typeof(Unit);
}