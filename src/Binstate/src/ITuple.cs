namespace Binstate
{
  public interface ITuple<out TArgument, out TRelay>
  {
    TArgument PassedArgument { get; }
    TRelay RelayedArgument{ get; }
  }
  
  public class Tuple<TArgument, TPropagate> : ITuple<TArgument, TPropagate>
  {
    public Tuple(TArgument passedArgument, TPropagate relayedArgument)
    {
      PassedArgument = passedArgument;
      RelayedArgument = relayedArgument;
    }

    public TArgument PassedArgument { get; }
    public TPropagate RelayedArgument { get; }
  }
}