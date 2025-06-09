using System;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

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
      /// <inheritdoc cref="AddTransition{TEventArgument}"/>
      ITransitions AddTransition(TEvent @event, TState targetState, Transition<Unit, Unit>.Action? action = null);

      /// <inheritdoc cref="ITransitions{TStateArgument}.AddTransition{TEventArgument}"/>
      ITransitions AddTransition<TEventArgument>(TEvent @event, TState targetState, Transition<Unit, TEventArgument>.Action action);

      /// <inheritdoc cref="ITransitions{TStateArgument}.AddConditionalTransition{TEventArgument}"/>
      ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                  @event,
        TState                                  targetState,
        Transition<Unit, TEventArgument>.Guard  guard,
        Transition<Unit, TEventArgument>.Action action);

#pragma warning disable 1574,1584,1581,1580
      /// <inheritdoc cref="AddConditionalTransition{TEventArgument}"/>
#pragma warning restore 1574,1584,1581,1580
      ITransitions AddConditionalTransition(
        TEvent                        @event,
        TState                        targetState,
        Transition<Unit, Unit>.Guard  guard,
        Transition<Unit, Unit>.Action action);

#pragma warning disable 1574,1584,1581,1580
      /// <inheritdoc cref="AddConditionalTransition{TEventArgument}"/>
#pragma warning restore 1574,1584,1581,1580
      ITransitions AddConditionalTransition(TEvent @event, TState targetState, Func<bool> guard, Transition<Unit, Unit>.Action? action = null);

      /// <inheritdoc cref="ITransitions{TStateArgument}.AddConditionalTransition{TEventArgument}"/>
      ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                  @event,
        TState                                  targetState,
        Func<bool>                              guard,
        Transition<Unit, TEventArgument>.Action action);

#pragma warning disable 1574,1584,1581,1580
      /// <inheritdoc cref="AddDynamicTransition{TEventArgument}(TEvent, Func{TState?}, Transition{TStateArgument,TEventArgument}.Action)"/>
#pragma warning restore 1574,1584,1581,1580
      ITransitions AddDynamicTransition(TEvent @event, Func<TState?> selectState, Transition<Unit, Unit>.Action? action = null);

      /// <summary>
      /// Defines a dynamic transition from the currently configured state to the state returned by <paramref name="selectState"/>
      /// when the <paramref name="event"/> is raised.
      /// If <paramref name="selectState"/> returns false, the transition is canceled.
      /// </summary>
      /// <param name="event">The event that triggers the transition.</param>
      /// <param name="selectState">A delegate that returns the ID of the state to transit to via out parameter, or false to cancel the transition.</param>
      /// <param name="action">An action to be executed upon the transition after the 'exit' action of the current state
      /// and before the 'enter' action of the target state.</param>
      /// <returns>The <see cref="ITransitions"/> instance for chaining configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="selectState"/> is null.</exception>
      ITransitions AddDynamicTransition<TEventArgument>(TEvent @event, Func<TState?> selectState, Transition<Unit, TEventArgument>.Action action);

      /// <inheritdoc cref="AllowReentrancy"/>
      void AllowReentrancy(TEvent @event, Action? action = null);

      /// <summary>
      /// Defines a re-entrant transition from the current state to itself when the <paramref name="event"/> is raised.
      /// In such transition, no 'enter' nor 'exit' actions are executed.
      /// Use <see cref="AddTransition"/> and specify "its own" State ID as a target state
      /// to create reentrancy transition which calls 'enter' and 'exit' actions.
      /// </summary>
      /// <param name="event">The event that triggers the re-entrant transition.</param>
      /// <param name="action"> </param>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
      void AllowReentrancy<TEventArgument>(TEvent @event, Action<TEventArgument> action);
    }
  }
#pragma warning restore 1574,1584,1581,1580
}