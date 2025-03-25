using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal static class Fake
{
  public static IState<TState, TEvent> CreateFakeInitialState<TState, TEvent>(TState realActiveState) where TState : notnull where TEvent : notnull
    => new State<TState, TEvent, Unit>(realActiveState);

  /// <summary>
  /// Must have <typeparamref name="TArgument"/> generic parameter to reflection machinery works
  /// </summary>
  private class State<TState, TEvent, TArgument>(TState targetStateId) : IState<TState, TEvent>
  {
#pragma warning disable CS1574
    /// <summary>
    /// Returns transition to the <see cref="targetStateId"/> no matter what <paramref name="event"/> is passed
    /// </summary>
#pragma warning restore CS1574
    public bool FindTransitionTransitive(TEvent @event, [NotNullWhen(true)] out Transition<TState, TEvent>? transition)
    {
      transition = new Transition<TState, TEvent>(default!, Builder.CreateStaticGetState(targetStateId), true, null);
      return true;
    }

    public IState<TState, TEvent>? ParentState           => null;
    public Type?                   GetArgumentTypeSafe() => null;

    public void ExitSafe(Action<Exception> onException)
    {
      // do nothing
    }

    public void CallTransitionActionSafe(ITransition transition, Action<Exception> onException)
    {
      // do nothing
    }

    #region Not implemented

    public TState                                         Id          => throw Paranoia.GetException("this method should not be called ever.");
    public Dictionary<TEvent, Transition<TState, TEvent>> Transitions => throw Paranoia.GetException("this method should not be called ever.");
    IState? IState.                                       ParentState => ParentState;

    public int  DepthInTree => 0;
    public bool IsActive    { get; set; }

    public void EnterSafe<TEvent1>(IStateController<TEvent1> stateController, Action<Exception> onException)
      => throw Paranoia.GetException("this method should not be called ever.");

    public object? GetArgumentAsObject() => throw Paranoia.GetException("this method should not be called ever.");

    #endregion

  }
}