using System;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    private static Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> FuncToSelector<TStateArgument, TEventArgument>( Func<TState?> selector)
      => (Transition<TStateArgument, TEventArgument>.Context<TState, TEvent> _, [NotNullWhen(true)] out TState? state) =>
      {
        state = selector();
        return state is not null;
      };

    internal class Transitions : ITransitions
    {
      protected readonly StateConfig StateConfig;

      protected Transitions(StateConfig stateConfig) => StateConfig = stateConfig;

      public ITransitions AddTransition(TEvent @event, TState targetState, Transition<Unit, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, targetState, null, action);
        return this;
      }

      public ITransitions AddTransition<TEventArgument>(TEvent @event, TState targetState, Transition<Unit, TEventArgument>.Action<TState, TEvent> action)
      {
        StateConfig.AddTransition(@event, targetState, null, action);
        return this;
      }

      public ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                                   @event,
        TState                                                   targetState,
        Transition<Unit, TEventArgument>.Guard                   guard,
        Transition<Unit, TEventArgument>.Action<TState, TEvent>? action)
      {
        StateConfig.AddTransition(@event, targetState, guard, action);
        return this;
      }

      public ITransitions AddConditionalTransition(
        TEvent                                         @event,
        TState                                         targetState,
        Transition<Unit, Unit>.Guard                   guard,
        Transition<Unit, Unit>.Action<TState, TEvent>? action)
      {
        StateConfig.AddTransition(@event, targetState, guard, action);
        return this;
      }

      public ITransitions AddConditionalTransition(
        TEvent                                         @event,
        TState                                         targetState,
        Func<bool>                                     guard,
        Transition<Unit, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, targetState, _ => guard(), action);
        return this;
      }

      public ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                                   @event,
        TState                                                   targetState,
        Func<bool>                                               guard,
        Transition<Unit, TEventArgument>.Action<TState, TEvent>? action)
      {
        StateConfig.AddTransition(@event, targetState, _ => guard(), action);
        return this;
      }

      public ITransitions AddDynamicTransition(
        TEvent                                               @event,
        Transition<Unit, Unit>.StateSelector<TState, TEvent> selector,
        Transition<Unit, Unit>.Action<TState, TEvent>?       action = null)
      {
        StateConfig.AddTransition(@event, selector, action);
        return this;
      }

      public ITransitions AddDynamicTransition<TEventArgument>(
        TEvent                                                         @event,
        Transition<Unit, TEventArgument>.StateSelector<TState, TEvent> selector,
        Transition<Unit, TEventArgument>.Action<TState, TEvent>        action)
      {
        StateConfig.AddTransition(@event, selector, action);
        return this;
      }

      public ITransitions AddDynamicTransition(TEvent @event, Func<TState?> selector, Transition<Unit, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, FuncToSelector<Unit, Unit>(selector), action);
        return this;
      }

      public ITransitions AddDynamicTransition<TEventArgument>(
        TEvent                                                  @event,
        Func<TState?>                                           selector,
        Transition<Unit, TEventArgument>.Action<TState, TEvent> action)
      {
        StateConfig.AddTransition(@event, FuncToSelector<Unit, TEventArgument>(selector), action);
        return this;
      }

      public void AllowReentrancy(TEvent @event, Action? action = null)
        => StateConfig.AddReentrantTransition<Unit>(@event, action is null ? null : _ => action());

      public void AllowReentrancy<TEventArgument>(TEvent @event, Action<TEventArgument> action) => StateConfig.AddReentrantTransition(@event, action);
    }
  }
}