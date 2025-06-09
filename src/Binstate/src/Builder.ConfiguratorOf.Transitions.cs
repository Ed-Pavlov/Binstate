using System;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    private static Transition<TStateArgument, TEventArgument>.Selector FuncToSelector<TStateArgument, TEventArgument>(Func<TState?> selector)
      => delegate(Transition<TStateArgument, TEventArgument>.Arguments _, out TState? state)
      {
        state = selector();
        return state is not null;
      };

    internal class Transitions : ITransitions
    {
      public readonly StateConfig StateConfig;

      protected Transitions(StateConfig stateConfig) => StateConfig = stateConfig;

      public ITransitions AddTransition(TEvent @event, TState targetState, Transition<Unit, Unit>.Action? action = null)
      {
        StateConfig.AddTransition(@event, targetState, null, action);
        return this;
      }

      public ITransitions AddTransition<TEventArgument>(TEvent @event, TState targetState, Transition<Unit, TEventArgument>.Action action)
      {
        StateConfig.AddTransition(@event, targetState, null, action);
        return this;
      }

      public ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                  @event,
        TState                                  targetState,
        Transition<Unit, TEventArgument>.Guard  guard,
        Transition<Unit, TEventArgument>.Action action)
      {
        StateConfig.AddTransition(@event, targetState, guard, action);
        return this;
      }

      public ITransitions AddConditionalTransition(TEvent @event, TState targetState, Transition<Unit, Unit>.Guard guard, Transition<Unit, Unit>.Action action)
      {
        StateConfig.AddTransition(@event, targetState, guard, action);
        return this;
      }

      public ITransitions AddConditionalTransition(TEvent @event, TState targetState, Func<bool> guard, Transition<Unit, Unit>.Action? action = null)
      {
        StateConfig.AddTransition(@event, targetState, _ => guard(), action);
        return this;
      }

      public ITransitions AddConditionalTransition<TEventArgument>(
        TEvent                                  @event,
        TState                                  targetState,
        Func<bool>                              guard,
        Transition<Unit, TEventArgument>.Action action)
      {
        StateConfig.AddTransition(@event, targetState, _ => guard(), action);
        return this;
      }

      public ITransitions AddDynamicTransition(TEvent @event, Transition<Unit, Unit>.Selector selector, Transition<Unit, Unit>.Action? action = null)
      {
        StateConfig.AddTransition(@event, selector, action);
        return this;
      }

      public ITransitions AddDynamicTransition<TEventArgument>(
        TEvent                                    @event,
        Transition<Unit, TEventArgument>.Selector selector,
        Transition<Unit, TEventArgument>.Action   action)
      {
        StateConfig.AddTransition(@event, selector, action);
        return this;
      }

      public ITransitions AddDynamicTransition(TEvent @event, Func<TState?> selector, Transition<Unit, Unit>.Action? action = null)
      {
        StateConfig.AddTransition(@event, FuncToSelector<Unit, Unit>(selector), action);
        return this;
      }

      public ITransitions AddDynamicTransition<TEventArgument>(TEvent @event, Func<TState?> selector, Transition<Unit, TEventArgument>.Action action)
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