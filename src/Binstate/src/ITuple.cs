using JetBrains.Annotations;

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
    [CanBeNull]
    TArgument PassedArgument { get; }
    
    /// <summary>
    /// Relayed argument value
    /// </summary>
    [CanBeNull]
    TRelay RelayedArgument{ get; }
  }

  /// <inheritdoc />
  internal class Tuple<TArgument, TRelay> : ITuple<TArgument, TRelay>
  {
    public Tuple([CanBeNull] TArgument passedArgument, [CanBeNull] TRelay relayedArgument)
    {
      PassedArgument = passedArgument;
      RelayedArgument = relayedArgument;
    }

    /// <inheritdoc />
    public TArgument PassedArgument { get; }
    
    /// <inheritdoc />
    public TRelay RelayedArgument { get; }
  }

  /// <summary>
  /// Syntax sugar for creating <see cref="ITuple{TArgument,TRelay}"/> without specifying generic arguments
  /// </summary>
  public static class Pair
  {
    /// <summary>
    /// Creates <see cref="ITuple{TArgument,TRelay}"/>
    /// </summary>
    public static ITuple<TArgument, TRelay> Of<TArgument, TRelay>(TArgument passedArgument, TRelay relayedArgument) => new Tuple<TArgument,TRelay>(passedArgument, relayedArgument);
  }
}