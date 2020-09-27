namespace Binstate
{
  /// <summary>
  /// Interface is used to make argument types invariant in order to pass arguments of compatible types
  /// </summary>
  /// <typeparam name="TArgument">Type of argument passed to <see cref="IStateMachine{TState,TEvent}.Raise{T}"/> method </typeparam>
  /// <typeparam name="TRelay">Type of the argument attached to one of the currently active states ans passed t
  /// o <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}"/> method.
  /// </typeparam>
  public interface ITuple<out TArgument, out TRelay>
  {
    /// <summary>
    /// Passed argument value
    /// </summary>
    TArgument PassedArgument { get; }
    
    /// <summary>
    /// Relayed argument value
    /// </summary>
    TRelay RelayedArgument{ get; }
  }
  
  internal class Tuple<TArgument, TPropagate> : ITuple<TArgument, TPropagate>
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