using System;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class Transitions<TStateArgument> : ITransitions<TStateArgument>
    {
      protected StateConfig<TStateArgument> StateConfig { get; }

      public Transitions(StateConfig<TStateArgument> stateConfig) => StateConfig = stateConfig;

      public ITransitions<TStateArgument> AddTransition(
        TEvent                                                   @event,
        TState                                                   targetState,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, targetState, null, action);
        return this;
      }

      public ITransitions<TStateArgument> AddTransition<TEventArgument>(
        TEvent                                                            @event,
        TState                                                            targetState,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action)
      {
        StateConfig.AddTransition(@event, targetState, null, action);
        return this;
      }

      public ITransitions<TStateArgument> AddConditionalTransition<TEventArgument>(
        TEvent                                                            @event,
        TState                                                            targetState,
        Transition<TStateArgument, TEventArgument>.Guard                  guard,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action)
      {
        StateConfig.AddTransition(@event, targetState, guard, action);
        return this;
      }

      public ITransitions<TStateArgument> AddConditionalTransition(
        TEvent                                                   @event,
        TState                                                   targetState,
        Transition<TStateArgument, Unit>.Guard                   guard,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, targetState, guard, action);
        return this;
      }

      public ITransitions<TStateArgument> AddConditionalTransition(
        TEvent                                                   @event,
        TState                                                   targetState,
        Func<bool>                                               guard,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, targetState, _ => guard(), action);
        return this;
      }

      public ITransitions<TStateArgument> AddConditionalTransition<TEventArgument>(
        TEvent                                                            @event,
        TState                                                            targetState,
        Func<bool>                                                        guard,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action)
      {
        StateConfig.AddTransition(@event, targetState, _ => guard(), action);
        return this;
      }

      public ITransitions<TStateArgument> AddDynamicTransition(
        TEvent                                                         @event,
        Transition<TStateArgument, Unit>.StateSelector<TState, TEvent> selector,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>?       action = null)
      {
        StateConfig.AddTransition(@event, selector, action);
        return this;
      }

      public ITransitions<TStateArgument> AddDynamicTransition<TEventArgument>(
        TEvent                                                                   @event,
        Transition<TStateArgument, TEventArgument>.StateSelector<TState, TEvent> selector,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>        action)
      {
        StateConfig.AddTransition(@event, selector, action);
        return this;
      }

      public ITransitions<TStateArgument> AddDynamicTransition(
        TEvent                                                   @event,
        Func<TState?>                                            selector,
        Transition<TStateArgument, Unit>.Action<TState, TEvent>? action = null)
      {
        StateConfig.AddTransition(@event, FuncToSelector<TStateArgument, Unit>(selector), action);
        return this;
      }

      public ITransitions<TStateArgument> AddDynamicTransition<TEventArgument>(
        TEvent                                                            @event,
        Func<TState?>                                                     selector,
        Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> action)
      {
        StateConfig.AddTransition(@event, FuncToSelector<TStateArgument, TEventArgument>(selector), action);
        return this;
      }

      public void AllowReentrancy(TEvent @event, Action? action = null)
        => StateConfig.AddReentrantTransition<Unit>(@event, action is null ? null : _ => action());

      public void AllowReentrancy<TEventArgument>(TEvent @event, Action<TEventArgument> action) => StateConfig.AddReentrantTransition(@event, action);
    }
  }
}