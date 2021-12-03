namespace Binstate;

/// <summary>
/// Interface is used to make argument types invariant in order to pass arguments of compatible types
/// </summary>
/// <typeparam name="TPassed">Type of argument passed to <see cref="IStateMachine{TState,TEvent}.Raise{T}"/> method </typeparam>
/// <typeparam name="TRelay">Type of the argument attached to one of the currently active states
/// and passed to <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}()"/> method.
/// </typeparam>
public interface ITuple<out TPassed, out TRelay>
{
  /// <summary>
  /// Passed argument value
  /// </summary>
  TPassed PassedArgument { get; }

  /// <summary>
  /// Relayed argument value
  /// </summary>
  TRelay RelayedArgument { get; }
}

/// <inheritdoc />
internal class Tuple<TPassed, TRelay> : ITuple<TPassed, TRelay>
{
  public Tuple(TPassed passedArgument, TRelay relayedArgument)
  {
    PassedArgument  = passedArgument;
    RelayedArgument = relayedArgument;
  }

  /// <inheritdoc />
  public TPassed PassedArgument { get; }

  /// <inheritdoc />
  public TRelay RelayedArgument { get; }
}