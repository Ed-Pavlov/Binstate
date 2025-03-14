using System;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {

    /// <summary>
    /// This interface is used to configure which transitions allowed from the currently configured state.
    /// </summary>
    public interface ITransitions
    {
      /// <summary>
      /// Defines a transition from the currently configured state to the state specified by <paramref name="stateId"/> when the <paramref name="event"/> is raised.
      /// Optionally, specify an action to be executed upon the transition before 'exit' action.
      /// </summary>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="stateId">The ID of the state to transition to.</param>
      /// <param name="action">An optional action to be executed upon the transition.</param>
      /// <returns>The <see cref="ITransitions"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="stateId"/> is null.</exception>
      ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null);

#pragma warning disable 1574,1584,1581,1580
      /// <summary>
      /// Defines a dynamic transition from the currently configured state to the state returned by <paramref name="getState"/> when the <paramref name="event"/> is raised.
      /// If <paramref name="getState"/> returns null, the transition is cancelled.
      /// </summary>
      /// <remarks>
      /// Use this overload if you use a ValueType (e.g. enum) as a <typeparamref name="TState" /> and the default value of the value type as a valid State id.
      /// Otherwise, consider using <see cref="AddTransition(TEvent,System.Func{TState?})" /> method as more simple.
      /// </remarks>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="getState">A delegate that returns the ID of the state to transition to, or null to cancel the transition.</param>
      /// <returns>The <see cref="ITransitions"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="getState"/> is null.</exception>
#pragma warning restore 1574,1584,1581,1580
      ITransitions AddTransition(TEvent @event, GetState<TState> getState);

#pragma warning disable 1574,1584,1581,1580
      /// <summary>
      /// Defines a dynamic transition from the currently configured state to the state returned by <paramref name="getState"/> when the <paramref name="event"/> is raised.
      /// If <paramref name="getState"/> returns null, the transition is cancelled.
      /// </summary>
      /// <remarks>
      /// Use this overload if you use a reference type (class) as a <typeparamref name="TState" /> or the default value of the value type doesn't represent
      /// a valid State id. If you use a value type (e.g. enum) as a <typeparamref name="TState" /> and the default value of the value type is a valid State id
      /// you must use <see cref="AddTransition(TEvent,Binstate.GetState{TState})" /> method.
      /// </remarks>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="getState">A delegate that returns the ID of the state to transition to, or null to cancel the transition.</param>
      /// <returns>The <see cref="ITransitions"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="getState"/> is null.</exception>
#pragma warning restore 1574,1584,1581,1580
      ITransitions AddTransition(TEvent @event, Func<TState?> getState);

      /// <summary>
      /// Defines a re-entrant transition from the current state to itself when the <paramref name="event"/> is raised.
      /// In such transitions, both the exit and enter actions of the state are executed.
      /// </summary>
      /// <param name="event">The event that triggers the re-entrant transition.</param>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
      void AllowReentrancy(TEvent @event);
    }

    /// <summary>
    ///
    /// </summary>
    public interface ITransitionsEx : ITransitions
    {
      /// <summary>
      /// Defines a transition from the currently configured state to the state specified by <paramref name="stateId"/> when the <paramref name="event"/> is raised.
      /// Specifies an action with an argument of type <typeparamref name="TArgument"/> to be executed upon the transition before the 'exit' action.
      /// </summary>
      /// <typeparam name="TArgument">The type of the argument passed to the action.</typeparam>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="stateId">The ID of the state to transition to.</param>
      /// <param name="action">The action to be executed upon the transition, accepting an argument of type <typeparamref name="TArgument"/>.</param>
      /// <returns>The <see cref="ITransitions{T}"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/>, <paramref name="stateId"/>, or <paramref name="action"/> is null.</exception>
      ITransitions<TArgument> AddTransition<TArgument>(TEvent @event, TState stateId, Action<TArgument> action);
    }

    /// <summary>
    /// This interface is used to configure which transitions allowed from the currently configured state.
    /// </summary>
    public interface ITransitions<out T> : ITransitions
    {
      /// <inheritdoc cref="ITransitionsEx.AddTransition{T}(TEvent, TState, System.Action{T})"/>
      ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T> action);
    }
  }
}