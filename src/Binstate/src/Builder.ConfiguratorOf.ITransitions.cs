using System;
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
      ITransitions AddTransition(TEvent @event, TState targetState, Transition<Unit, Unit>.Action<TState, TEvent>? action = null);

      /// <inheritdoc cref="ITransitions{TStateArgument}.AddTransition{TEventArgument}"/>
      ITransitions AddTransition<TEventArgument>(TEvent @event, TState targetState, Transition<Unit, TEventArgument>.Action<TState, TEvent> action);

      /// <inheritdoc cref="ITransitions{TStateArgument}.AddConditionalTransition{TEventArgument}"/>
      ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                                   @event,
        TState                                                   targetState,
        Transition<Unit, TEventArgument>.Guard                   guard,
        Transition<Unit, TEventArgument>.Action<TState, TEvent>? action = null);

//#pragma warning disable ,1584,1581,1580
#pragma warning disable CS0419, 1574
      /// <inheritdoc cref="AddConditionalTransition{TEventArgument}"/>
      ITransitions AddConditionalTransition(
        TEvent                                         @event,
        TState                                         targetState,
        Transition<Unit, Unit>.Guard                   guard,
        Transition<Unit, Unit>.Action<TState, TEvent>? action = null);
#pragma warning restore CS0419, 1574

#pragma warning disable CS0419, 1574
      /// <inheritdoc cref="AddConditionalTransition{TEventArgument}"/>
#pragma warning restore CS0419, 1574
      ITransitions AddConditionalTransition(TEvent @event, TState targetState, Func<bool> guard, Transition<Unit, Unit>.Action<TState, TEvent>? action = null);

      /// <inheritdoc cref="ITransitions{TStateArgument}.AddConditionalTransition{TEventArgument}"/>
      ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                                   @event,
        TState                                                   targetState,
        Func<bool>                                               guard,
        Transition<Unit, TEventArgument>.Action<TState, TEvent>? action = null);

#pragma warning disable CS0419, 1574
      /// <inheritdoc cref="AddDynamicTransition{TEventArgument}"/>
      ITransitions AddDynamicTransition(
        TEvent                                               @event,
        Transition<Unit, Unit>.StateSelector<TState, TEvent> selectState,
        Transition<Unit, Unit>.Action<TState, TEvent>?       action = null);
#pragma warning restore CS0419, 1574

#pragma warning disable 0419, 1574, 1580
      /// <remarks>
      /// Use this overload if you use a ValueType (e.g., enum) as a <typeparamref name="TState" /> and the default value of the value type as a valid State id.
      /// Otherwise, consider using
      /// <see cref="AddDynamicTransition{TEventArgument}(TEvent, Transition{TStateArgument,TEventArgument}.Selector{TState,TEvent}, Transition{TStateArgument,TEventArgument}.Action{TState,TEvent})" />
      /// method as more simple.
      /// </remarks>
      ITransitions AddDynamicTransition<TEventArgument>(
        TEvent                                                         @event,
        Transition<Unit, TEventArgument>.StateSelector<TState, TEvent> selectState,
        Transition<Unit, TEventArgument>.Action<TState, TEvent>        action);
#pragma warning restore CS0419, 1574, 1580

#pragma warning disable CS0419, 1574, 1580
      /// <inheritdoc cref="AddDynamicTransition{TEventArgument}(TEvent, Func{TState?}, Transition{TStateArgument,TEventArgument}.Action{TState,TEvent})"/>
#pragma warning restore CS0419, 1574, 1580
      ITransitions AddDynamicTransition(TEvent @event, Func<TState?> selectState, Transition<Unit, Unit>.Action<TState, TEvent>? action = null);

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
      ITransitions AddDynamicTransition<TEventArgument>(
        TEvent                                                  @event,
        Func<TState?>                                           selectState,
        Transition<Unit, TEventArgument>.Action<TState, TEvent> action);

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
#pragma warning restore CS0419, 1574
}