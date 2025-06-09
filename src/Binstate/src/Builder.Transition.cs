using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  /// <typeparam name="TEventArgument">The type of the event argument.</typeparam>
  /// <typeparam name="TStateArgument">The type of the state argument.</typeparam>
  public static class Transition<TStateArgument, TEventArgument>
  {
    /// <summary>
    /// A delegate to be used with <see cref="Builder{TState,TEvent}.ConfiguratorOf.ITransitions.AddTransition" />.
    /// </summary>
    /// <param name="arguments">The transition arguments containing event and state data.</param>
    /// <returns> Returns false if no transition should be performed. </returns>
    public delegate bool Guard(Arguments arguments);

    /// <summary>
    /// Contains the event and state arguments for a transition.
    /// </summary>
    public struct Arguments
    {
      /// <param name="eventArgument">The argument associated with the event.</param>
      /// <param name="stateArgument">The argument associated with the current state.</param>
      public Arguments(TStateArgument stateArgument, TEventArgument eventArgument)
      {
        StateArgument = stateArgument;
        EventArgument = eventArgument;
      }

      /// <summary>
      /// Argument associated with the current state.
      /// </summary>
      public TStateArgument StateArgument { get; }

      /// <summary>
      /// Argument associated with the event.
      /// </summary>
      public TEventArgument EventArgument { get; }
    }

    /// <summary>
    /// A delegate that selects the target state based on provided arguments.
    /// </summary>
    /// <param name="arguments">The transition arguments containing event and state data.</param>
    /// <param name="state">The selected target state when the delegate returns true.</param>
    /// <returns>Returns true if a valid target state was selected, false otherwise.</returns>
    public delegate bool Selector(Arguments arguments, [NotNullWhen(true)] out TState? state);

    /// <summary>
    /// Represents a delegate to be invoked during a state transition process.
    /// </summary>
    /// <param name="context">The context containing information regarding the state transition.</param>
    public delegate void Action(Context context);

    /// <summary>
    /// Contains the context information for a state transition.
    /// </summary>
    public struct Context
    {
      /// <param name="eventId">The identifier of the event triggering the transition.</param>
      /// <param name="fromStateId">The identifier of the source state.</param>
      /// <param name="toStateId">The identifier of the target state.</param>
      /// <param name="arguments">The arguments associated with the transition.</param>
      public Context(TEvent eventId, TState fromStateId, TState toStateId, Arguments arguments)
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
      public TState ToStateId { get; }

      /// <summary>
      /// Gets the arguments associated with the transition.
      /// </summary>
      public Arguments Arguments { get; }
    }
  }
}