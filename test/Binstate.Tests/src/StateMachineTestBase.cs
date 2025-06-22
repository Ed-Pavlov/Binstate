using System;
using System.Collections;
using System.Collections.Generic;
using BeatyBit.Binstate;
using FakeItEasy;
using NUnit.Framework;

namespace Binstate.Tests;

public abstract class StateMachineTestBase
{
  protected const string Initial     = nameof(Initial);
  protected const string StateX      = nameof(StateX);
  protected const string StateY      = nameof(StateY);
  protected const string StateZ      = nameof(StateZ);
  protected const string Final       = nameof(Final);
  protected const string Root        = nameof(Root);
  protected const string Parent      = nameof(Parent);
  protected const string Child       = nameof(Child);
  protected const int    GoToX       = 1;
  protected const int    GoToY       = GoToX      + 1;
  protected const int    GoToZ       = GoToY      + 1;
  protected const int    GoToParent  = GoToZ      + 1;
  protected const int    GoToChild   = GoToParent + 1;
  protected const int    GoToRoot    = GoToChild  + 1;
  protected const int    GoToInitial = GoToRoot   + 1;

  protected readonly Action InitialEnterAction = A.Fake<Action>();
  protected readonly Action XEnterAction       = A.Fake<Action>();
  protected readonly Action YEnterAction       = A.Fake<Action>();
  protected readonly Action ZEnterAction       = A.Fake<Action>();
  protected readonly Action RootEnterAction    = A.Fake<Action>();
  protected readonly Action ChildEnterAction   = A.Fake<Action>();
  protected readonly Action FinalEnterAction   = A.Fake<Action>();

  [SetUp]
  public void Setup() => ClearRecordedCalls();

  protected void ClearRecordedCalls(params object[] actions)
  {
    Fake.ClearRecordedCalls(InitialEnterAction);
    Fake.ClearRecordedCalls(XEnterAction);
    Fake.ClearRecordedCalls(YEnterAction);
    Fake.ClearRecordedCalls(ZEnterAction);
    Fake.ClearRecordedCalls(RootEnterAction);
    Fake.ClearRecordedCalls(ChildEnterAction);
    Fake.ClearRecordedCalls(FinalEnterAction);

    foreach(var action in actions)
      Fake.ClearRecordedCalls(action);
  }

  protected static Builder<string, int> CreateBaseBuilder(Builder.Options options = default)
  {
    var builder = new Builder<string, int>(OnException, options);
    builder
     .DefineState(Initial)
     .AddTransition(GoToX,     StateX)
     .AddTransition(GoToChild, Child);

    builder
     .DefineState(StateX)
     .AddTransition(GoToZ, StateZ)
     .AddTransition(GoToY, StateY);

    builder
     .DefineState(StateY)
     .AddTransition(GoToZ, StateZ);

    builder
     .DefineState(StateZ)
     .AddTransition(GoToX, StateX);

    builder
     .DefineState(Root)
     .AddTransition(GoToX, StateX);

    builder
     .DefineState(Child)
     .AsSubstateOf(Root)
     .AddTransition(GoToInitial, Initial);

    builder
     .DefineState(Final);

    return builder;
  }

  protected static void OnException(Exception exception)
  {
    Console.WriteLine(exception);
    Assert.Fail(exception.Message);
  }

  public static IEnumerable<RaiseWay> RaiseWays() => [RaiseWay.Raise, RaiseWay.RaiseAsync];

  public static void OnEnter<T>(Builder<string, int>.ConfiguratorOf.IEnterAction<T> state, Action<T> action) => state.OnEnter(action);
  public static void OnExit<T>(Builder<string, int>.ConfiguratorOf.IEnterAction<T>  state, Action<T> action) => state.OnExit(action);

  public static IEnumerable EnterExit()
  {
    yield return new Action<Builder<string, int>.ConfiguratorOf.IEnterAction<string>, Action<string>>(OnEnter);
    yield return new Action<Builder<string, int>.ConfiguratorOf.IEnterAction<string>, Action<string>>(OnExit);
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