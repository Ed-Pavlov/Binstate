using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

/// <summary>
/// Declarations of types used to configure and perform transitions
/// </summary>
public static class Transition<TStateArgument, TEventArgument>
{
  /// <summary>
  /// A delegate to be used with <see cref="Binstate.Builder{TState,TEvent}.ConfiguratorOf.ITransitions.AddTransition" />.
  /// </summary>
  /// <param name="arguments">The transition arguments containing event and state data.</param>
  /// <returns> Returns false if no transition should be performed. </returns>
  public delegate bool Guard(ITuple<TStateArgument, TEventArgument> arguments);

  /// <summary>
  /// A delegate that selects the target state based on provided arguments.
  /// </summary>
  /// <param name="context">The context containing information regarding the state transition.</param>
  /// <param name="state">The selected target state when the delegate returns true.</param>
  /// <returns>Returns true if a valid target state was selected, false otherwise.</returns>
  public delegate bool StateSelector<TState, TEvent>(Context<TState, TEvent> context, [NotNullWhen(true)] out TState? state);

  /// <summary>
  /// Represents a delegate to be invoked during a state transition process.
  /// </summary>
  /// <param name="context">The context containing information regarding the state transition.</param>
  public delegate void Action<TState, TEvent>(Context<TState, TEvent> context);

  /// <summary>
  /// Contains the context information for a state transition.
  /// </summary>
  public struct Context<TState, TEvent>
  {
    /// <param name="eventId">The identifier of the event triggering the transition.</param>
    /// <param name="fromStateId">The identifier of the source state.</param>
    /// <param name="toStateId">The identifier of the target state.</param>
    /// <param name="arguments">The arguments associated with the transition.</param>
    public Context(TEvent eventId, TState fromStateId, TState? toStateId, ITuple<TStateArgument, TEventArgument> arguments)
    {
      EventId     = eventId;
      FromStateId = fromStateId;
      ToStateId   = toStateId;
      Arguments   = arguments;
    }

    /// <summary>
    /// Gets the identifier of the event triggering the transition.
    /// </summary>
    public TEvent EventId { get; }

    /// <summary>
    /// Gets the identifier of the source state.
    /// </summary>
    public TState FromStateId { get; }

    /// <summary>
    /// Gets the identifier of the target state.
    /// </summary>
    public TState? ToStateId { get; }

    /// <summary>
    /// Gets the arguments associated with the transition.
    /// </summary>
    public ITuple<TStateArgument, TEventArgument> Arguments { get; }
  }
}