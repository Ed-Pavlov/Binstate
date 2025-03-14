using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Binstate.Tests;

public abstract class StateMachineTestBase
{
  protected const string Initial    = nameof(Initial);
  protected const string StateX     = nameof(StateX);
  protected const string StateY     = nameof(StateY);
  protected const string Final      = nameof(Final);
  protected const string Root       = nameof(Root);
  protected const string Parent     = nameof(Parent);
  protected const string Child      = nameof(Child);
  protected const int    GoToStateX = 1;
  protected const int    GoToStateY = 2;
  protected const int    GoToParent  = 9;
  protected const int    GoToChild  = 3;

  protected static void OnException(Exception exception) => Assert.Fail(exception.Message);

  public static IEnumerable<RaiseWay> RaiseWays() => new[] { RaiseWay.Raise, RaiseWay.RaiseAsync, };

  public static void OnEnter<T>(Builder<string, int>.ConfiguratorOf.IEnterAction state, Action<T> action) => state.OnEnter(action);
  public static void OnExit<T>(Builder<string, int>.ConfiguratorOf.IEnterAction  state, Action<T> action) => state.OnExit(action);

  public static IEnumerable EnterExit()
  {
    yield return new Action<Builder<string, int>.ConfiguratorOf.IEnterAction, Action<string>>(OnEnter);
    yield return new Action<Builder<string, int>.ConfiguratorOf.IEnterAction, Action<string>>(OnExit);
  }
}

public enum RaiseWay { Raise, RaiseAsync, }

public static class Extension
{
  public static bool Raise<TEvent>(this IStateMachine<TEvent> stateMachine, RaiseWay way, TEvent @event)
    => Call(way, () => stateMachine.Raise(@event), () => stateMachine.RaiseAsync(@event).Result);

  public static bool Raise<TEvent, TA>(this IStateMachine<TEvent> stateMachine, RaiseWay way, TEvent @event, TA arg, bool argumentIsFallback = false)
    => Call(way, () => stateMachine.Raise(@event, arg, argumentIsFallback), () => stateMachine.RaiseAsync(@event, arg, argumentIsFallback).Result);

  private static bool Call(RaiseWay way, Func<bool> syncAction, Func<bool> asyncAction)
    => way switch
    {
      RaiseWay.Raise      => syncAction(),
      RaiseWay.RaiseAsync => asyncAction(),
      _                   => throw new InvalidOperationException(),
    };
}