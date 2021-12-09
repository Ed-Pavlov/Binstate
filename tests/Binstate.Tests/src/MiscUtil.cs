namespace Binstate.Tests;

public class Tuple
{
  public static Tuple<TA, TR> Of<TA, TR>(TA arg, TR relay) => new Tuple<TA, TR>(arg, relay);
}