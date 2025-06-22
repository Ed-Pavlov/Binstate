using System;
using BeatyBit.Binstate;
using BeatyBit.Bits;

namespace Binstate.Tests.Util;

public static class SimplifyingSyntaxExtension
{
  public static Builder<TState, TEvent>.ConfiguratorOf.ITransitions AddTransitionSimple<TState, TEvent>(
    this Builder<TState, TEvent>.ConfiguratorOf.ITransitions configurator,
    TEvent                                                   @event,
    TState                                                   targetState,
    Action                                                   action) where TState : notnull where TEvent : notnull
    => configurator.AddTransition(@event, targetState, action.AsTransitionAction<TState, TEvent, Unit, Unit>());

  public static Builder<TState, TEvent>.ConfiguratorOf.ITransitions<TStateArgument> AddTransitionSimple<TState, TEvent, TStateArgument>(
    this Builder<TState, TEvent>.ConfiguratorOf.ITransitions<TStateArgument> configurator,
    TEvent                                                                   @event,
    TState                                                                   targetState,
    Action                                                                   action) where TState : notnull where TEvent : notnull
    => configurator.AddTransition(@event, targetState, action.AsTransitionAction<TState, TEvent, TStateArgument, Unit>());

  public static Builder<TState, TEvent>.ConfiguratorOf.ITransitions<TStateArgument> AddTransitionSimple<TState, TEvent, TStateArgument>(
    this Builder<TState, TEvent>.ConfiguratorOf.ITransitions<TStateArgument> configurator,
    TEvent                                                                   @event,
    TState                                                                   targetState,
    Action<TStateArgument>                                                   action) where TState : notnull where TEvent : notnull
    => configurator.AddTransition(@event, targetState, action.AsTransitionAction<TState, TEvent, TStateArgument, Unit>());
}