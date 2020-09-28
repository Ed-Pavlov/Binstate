using System.Collections.Generic;

namespace Binstate
{
  internal static class Extension
  {
    public static bool IsNull<T>(this T self) => EqualityComparer<T>.Default.Equals(self, default);
    public static bool IsNotNull<T>(this T self) => !EqualityComparer<T>.Default.Equals(self, default);

    public static Tuple<TArgument, TRelay> ToTupleUnsafe<TArgument, TRelay>(this MixOf<TArgument, TRelay> mixOf) => new Tuple<TArgument, TRelay>(mixOf.PassedArgument.Value, mixOf.RelayedArgument.Value);
  }
}