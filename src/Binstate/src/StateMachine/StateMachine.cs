﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

/// <summary>
/// The state machine implementation. Use <see cref="Builder{TState, TEvent}" /> to configure and build a state machine.
/// </summary>
internal partial class StateMachine<TState, TEvent> : IStateMachine<TEvent>
  where TState : notnull
  where TEvent : notnull
{
  private readonly string?           _persistenceSignature;
  private readonly Action<Exception> _onException;
  private readonly AutoResetEvent    _lock = new AutoResetEvent(true);

  /// <summary>
  /// The map of all defined states
  /// </summary>
  private readonly IReadOnlyDictionary<TState, IState<TState, TEvent>> _states;

  private volatile IState<TState, TEvent> _activeState;

  internal StateMachine(
    IReadOnlyDictionary<TState, IState<TState, TEvent>> states,
    Action<Exception> onException,
    TState initialStateId,
    string? persistenceSignature)
  {
    _states               = states      ?? throw new ArgumentNullException(nameof(states));
    _onException          = onException ?? throw new ArgumentNullException(nameof(onException));
    _persistenceSignature = persistenceSignature;
    _activeState          = GetStateById(initialStateId);
  }

  /// <inheritdoc />
  public bool Raise(TEvent @event)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionSync(@event, Unit.Default, true);
  }

  /// <inheritdoc />
  public bool Raise<T>(TEvent @event, T argument, bool argumentIsFallback = false)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionSync(@event, argument, argumentIsFallback);
  }

  /// <inheritdoc />
  public Task<bool> RaiseAsync(TEvent @event)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionAsync<Unit>(@event, default, false);
  }

  /// <inheritdoc />
  public Task<bool> RaiseAsync<T>(TEvent @event, T argument, bool argumentIsFallback = false)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionAsync(@event, argument, argumentIsFallback);
  }


  internal void EnterInitialState()
  {
    var restoredActiveState = _activeState;
    var fakeRootState = new VirtualRootState(_activeState.Id);
    _activeState = fakeRootState;

    // hack TransitionData to perform activation of restored states
    var transitionData = new TransitionData(fakeRootState, fakeRootState.FakeTransition, restoredActiveState, null, new Argument.Bag());
    PerformTransition(transitionData);
  }

  /// <summary>
  /// This is implemented as a separate method rather than constructor logic to be able specifying generic argument <typeparamref name="T"/>.
  /// While the <see cref="_activeState"/> is set in constructor to use not nullable type for it.
  /// </summary>
  /// <param name="initialStateArgument">Argument for the initial state</param>
  internal void EnterInitialState<T>(T initialStateArgument)
  {
    var fake = new VirtualRootState(_activeState.Id);
    _activeState = fake;

    // reuse the common logic to bootstrap the state machine to the initial state
    PerformTransitionSync(default!, initialStateArgument, false);
  }

  private bool PerformTransitionSync<TArgument>(TEvent @event, TArgument argument, bool argumentIsFallback)
  {
    var data = PrepareTransition(@event, argument, argumentIsFallback);

    return data != null && PerformTransition(data.Value);
  }

  private Task<bool> PerformTransitionAsync<TArgument>(TEvent @event, TArgument argument, bool argumentHasPriority)
  {
    var data = PrepareTransition(@event, argument, argumentHasPriority);

    return data is null
             ? Task.FromResult(false)
             : Task.Run(() => PerformTransition(data.Value));
  }

  private IState<TState, TEvent> GetStateById(TState state)
    => _states.TryGetValue(state, out var result) ? result : throw new TransitionException($"State '{state}' is not defined");

  private static IState? FindLeastCommonAncestor(IState left, IState right)
  {
    if(ReferenceEquals(left, right)) return null; // no common ancestor with itself

    var l = left;
    var r = right;

    var lDepth = l.DepthInTree;
    var rDepth = r.DepthInTree;

    while(lDepth != rDepth)
      if(lDepth > rDepth)
      {
        lDepth--;
        l = l!.ParentState;
      }
      else
      {
        rDepth--;
        r = r!.ParentState;
      }

    while(! ReferenceEquals(l, r))
    {
      l = l!.ParentState;
      r = r!.ParentState;
    }

    return l;
  }
}