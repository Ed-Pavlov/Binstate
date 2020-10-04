using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Binstate.Tests
{
  public abstract class StateMachineTestBase
  {
    protected const string Initial = "Initial";
    protected const int Event1 = 1;
    protected const int Event2 = 2;
    protected const string State1 = "state1";
    protected const string State2 = "state2";
    protected const string Root = "Root";
    protected const string Parent = "Parent";
    protected const string Child = "Child";
    
    protected static void OnException(Exception exception) => Assert.Fail(exception.Message);

    public static IEnumerable<RaiseWay> RaiseWays() => new[] {RaiseWay.Raise, RaiseWay.RaiseAsync};
  }

  public enum RaiseWay
  {
    Raise,
    RaiseAsync
  }
  
  public static class Extension
  {
    public static bool Raise<TState, TEvent>(this IStateMachine<TState, TEvent> stateMachine, RaiseWay way, TEvent @event) => 
      Call(way, () => stateMachine.Raise(@event), () => stateMachine.RaiseAsync(@event).Result);

    public static bool Raise<TState, TEvent, TA>(this IStateMachine<TState, TEvent> stateMachine, RaiseWay way, TEvent @event, TA arg) => 
      Call(way, () => stateMachine.Raise(@event, arg), () => stateMachine.RaiseAsync(@event, arg).Result);

    private static bool Call(RaiseWay way, Func<bool> syncAction, Func<bool> asyncAction)
    {
      return way switch
      {
        RaiseWay.Raise => syncAction(),
        RaiseWay.RaiseAsync => asyncAction(),
        _ => throw new InvalidOperationException()
      };
    }
  }
}