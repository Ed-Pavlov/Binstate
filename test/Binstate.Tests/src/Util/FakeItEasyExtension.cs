using System;
using System.Linq.Expressions;
using BeatyBit.Binstate;
using FakeItEasy;
using FakeItEasy.Configuration;

namespace Binstate.Tests.Util;

public static class FakeItEasyExtension
{
  public static UnorderedCallAssertion MustHaveHappenedOnceAndOnly(this IVoidArgumentValidationConfiguration configuration)
  {
    // once exactly call with specified and with any arguments means only call with specified arguments was performed
    configuration.MustHaveHappenedOnceExactly();
    return configuration.WithAnyArguments().MustHaveHappenedOnceExactly();
  }

  public static UnorderedCallAssertion MustHaveHappenedOnceAndOnly<TInterface>(this IReturnValueArgumentValidationConfiguration<TInterface> configuration)
  {
    // once exactly call with specified and with any arguments means only call with specified arguments was performed
    configuration.MustHaveHappenedOnceExactly();
    return configuration.WithAnyArguments().MustHaveHappenedOnceExactly();
  }

  public static Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> AsTransitionAction<TState, TEvent, TStateArgument, TEventArgument>(
    this Action fakeAction)
    => _ => fakeAction();

  public static Transition<TStateArgument, TEventArgument>.Action<TState, TEvent> AsTransitionAction<TState, TEvent, TStateArgument, TEventArgument>(
    this Action<TStateArgument> fakeAction)
    => _ => fakeAction(_.Arguments.ItemX);

  public static Action WithArguments<TState, TEvent, TStateArgument, TEventArgument>(
    this Transition<TStateArgument, TEventArgument>.Action<TState, TEvent>                     action,
    Expression<Func<Transition<TStateArgument, TEventArgument>.Context<TState, TEvent>, bool>> predicate)
  {
    return () => action(A<Transition<TStateArgument, TEventArgument>.Context<TState, TEvent>>.That.Matches(predicate));
  }
}