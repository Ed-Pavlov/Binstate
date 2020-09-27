using System.Collections.Generic;

namespace Binstate
{
  internal static class Extension
  {
    public static bool IsNull<T>(this T self) => EqualityComparer<T>.Default.Equals(self, default);
    public static bool IsNotNull<T>(this T self) => !EqualityComparer<T>.Default.Equals(self, default);

    public static Tuple<TA, TP> ToTupleUnsafe<TA, TP>(this MixOf<TA, TP> mixOf) => new Tuple<TA, TP>(mixOf.PassedArgument.Value, mixOf.RelayedArgument.Value);
  }
}