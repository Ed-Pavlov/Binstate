using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal partial class StateMachine<TState, TEvent>
{
  /// <summary>
  /// See usage for details
  /// </summary>
  internal class VirtualRootState(TState targetStateId) : IState<TState, TEvent>
  {
    private readonly Transition<Unit, Unit>  _fakeTransition = new Transition<Unit, Unit>(default!, targetStateId, null);

    public  IState<TState, TEvent>? ParentState           => null;
    public  int                     DepthInTree           => 0;
    public  bool                    IsActive              { get; set; }
    public  Type?                   GetArgumentTypeSafe() => null;

    public ITransition<TState, TEvent> FakeTransition => _fakeTransition;

#pragma warning disable CS1574
    /// <summary>
    /// Returns transition to the <see cref="targetStateId"/> no matter what <paramref name="event"/> is passed
    /// </summary>
#pragma warning restore CS1574
    public bool FindTransitionTransitive(TEvent @event, [NotNullWhen(true)] out ITransition<TState, TEvent>? transition)
    {
      transition = _fakeTransition;
      return true;
    }

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
    public IReadOnlyDictionary<TEvent, ITransition<TState, TEvent>> Transitions => throw Paranoia.GetException("this method should not be called ever.");
    IState? IState.                                       ParentState => ParentState;

    public void EnterSafe(IStateController<TEvent> stateController, Action<Exception> onException)
      => throw Paranoia.GetException("this method should not be called ever.");

    public Maybe<object?> GetArgumentAsObject() => throw Paranoia.GetException("this method should not be called ever.");

    #endregion

  }
}