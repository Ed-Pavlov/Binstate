using System;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    /// <summary>
    /// This interface is used to configure which transitions allowed from the currently configured state in case the configured state requires an argument.
    /// </summary>
    public interface ITransitions<TStateArgument>
    {
      /// <inheritdoc cref="AddTransition{TEventArgument}"/>
      ITransitions<TStateArgument> AddTransition(TEvent @event, TState stateId, Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null);

      /// <summary>
      /// Defines a transition from the currently configured state to the specified <paramref name="targetState"/>
      /// when the provided <paramref name="event"/> is raised.
      /// </summary>
      /// <param name="event">The event that triggers the state transition.</param>
      /// <param name="targetState">The target state to which the transition occurs.</param>
      /// <param name="action">An action to be executed during the transition after exiting the current state and
      /// before entering the target state. </param>
      /// <returns>The <see cref="ITransitions"/> instance, allowing further configuration chaining.</returns>
      /// <exception cref="ArgumentNullException"> Thrown when <paramref name="event"/>, <paramref name="targetState"/> or <paramref name="action"/> is null. </exception>
      ITransitions<TStateArgument> AddTransition<TEventArgument>(
        TEvent                                                            @event,
        TState                                                            targetState,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action);

      /// <inheritdoc cref="AddConditionalTransition{TEventArgument}"/>
      ITransitions<TStateArgument> AddConditionalTransition(
        TEvent                                                   @event,
        TState                                                   targetState,
        Transition<TStateArgument, Unit>.Guard                   guard,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null);

      /// <summary>
      /// Defines a conditional transition from the currently configured state to the state specified by <paramref name="targetState"/>
      /// when the <paramref name="event"/> is raised.
      /// If <paramref name="guard"/> returns false, the transition will not be performed.
      /// </summary>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="targetState">The ID of the state to transit to.</param>
      /// <param name="guard">A delegate that returns the ID of the state to transition to, or null to cancel the transition.</param>
      /// <param name="action">An action to be executed upon the transition after the 'exit' action of the current state
      /// and before the 'enter' action of the target state.</param>
      /// <returns>The <see cref="ITransitions"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="guard"/> is null.</exception>
      ITransitions<TStateArgument> AddConditionalTransition<TEventArgument>(
        TEvent                                                            @event,
        TState                                                            targetState,
        Transition<TStateArgument, TEventArgument>.Guard                  guard,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action);

#pragma warning disable CS0419, 1574
      /// <inheritdoc cref="AddDynamicTransition{TEventArgument}"/>
      ITransitions<TStateArgument> AddDynamicTransition(
        TEvent                                                         @event,
        Transition<TStateArgument, Unit>.StateSelector<TState, TEvent> stateSelector,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>?       action = null);
#pragma warning restore CS0419, 1574

#pragma warning disable CS0419, 1574, 1580
      /// <inheritdoc cref="AddDynamicTransition{TEventArgument}(TEvent, Func{TState}, TransitionAction{TState,TEvent,TStateArgument,TEventArgument}?)"/>
      /// <remarks>
      /// Use this overload if you use a ValueType (e.g., enum) as a <typeparamref name="TState" /> and the default value of the value type as a valid State id.
      /// Otherwise, consider using
      /// <see cref="AddDynamicTransition{TEventArgument}(TEvent, System.Func{TState}, TransitionAction{TState,TEvent,TStateArgument,TEventArgument})" />
      /// method as more simple.
      /// </remarks>
      ITransitions<TStateArgument> AddDynamicTransition<TEventArgument>(
        TEvent                                                                   @event,
        Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> stateSelector,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>        action);
#pragma warning restore CS0419, 1574, 1580

#pragma warning disable CS0419, 1574, 1580
      /// <inheritdoc cref="AddDynamicTransition{TEventArgument}(TEvent, Func{TState}, TransitionAction{TState,TEvent,TStateArgument,TEventArgument}?)"/>
      ITransitions<TStateArgument> AddDynamicTransition(
        TEvent                                                   @event,
        Func<TState?>                                            stateSelector,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null);
#pragma warning restore CS0419, 1574, 1580

      /// <summary>
      /// Defines a dynamic transition from the currently configured state to the state returned by <paramref name="stateSelector"/>
      /// when the <paramref name="event"/> is raised.
      /// If <paramref name="stateSelector"/> returns false, the transition is canceled.
      /// </summary>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="stateSelector">A delegate that returns the ID of the state to transit to via out parameter, or false to cancel the transition.</param>
      /// <param name="action">An action to be executed upon the transition after the 'exit' action of the current state
      /// and before the 'enter' action of the target state.</param>
      /// <returns>The <see cref="ITransitions"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="stateSelector"/> is null.</exception>
      ITransitions<TStateArgument> AddDynamicTransition<TEventArgument>(
        TEvent                                                            @event,
        Func<TState?>                                                     stateSelector,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action);

      /// <summary>
      /// Defines a re-entrant transition from the current state to itself when the <paramref name="event"/> is raised.
      /// In such transition, no actions are executed.
      /// Use <see cref="AddTransition{TEventArgument}"/> and specify "its own" State ID as a target state to create
      /// reentrancy transition which calls 'enter', 'exit', and 'transition' actions if any.
      /// </summary>
      /// <param name="event">The event that triggers the re-entrant transition.</param>
      /// <param name="action"> </param>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
      void AllowReentrancy(TEvent @event, Action? action = null);

      /// <summary>
      ///
      /// </summary>
      void AllowReentrancy<TEventArgument>(TEvent @event, Action<TEventArgument> action);
    }
  }
#pragma warning restore CS0419, 1574
}