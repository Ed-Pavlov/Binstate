using System;
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
  private readonly AutoResetEvent    _lock = new AutoResetEvent(true);
  private readonly Action<Exception> _onException;

  /// <summary>
  /// The map of all defined states
  /// </summary>
  private readonly Dictionary<TState, IState<TState, TEvent>> _states;

  private volatile IState<TState, TEvent> _activeState;

  internal StateMachine(Dictionary<TState, IState<TState, TEvent>> states, Action<Exception> onException, TState initialStateId)
  {
    _states      = states      ?? throw new ArgumentNullException(nameof(states));
    _onException = onException ?? throw new ArgumentNullException(nameof(onException));
    _activeState = GetStateById(initialStateId);
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

  /// <summary>
  /// This is implemented as a separate method rather than constructor logic to be able specifying generic argument <typeparamref name="T"/>.
  /// On the other hand, the <see cref="_activeState"/> is set in constructor to use not nullable type for it.
  /// </summary>
  /// <param name="initialStateArgument"></param>
  /// <param name="callEnterActions">Specifies if 'enter' action of all activated states should be called.</param>
  internal void EnterInitialState<T>(T initialStateArgument, bool callEnterActions = true)
  {
    var argumentType      = typeof(T);
    var argumentsBag      = new Argument.Bag();
    var initializeActions = new List<Action>();

    try
    {
      // activate all parent states of the initial state
      var state = _activeState;
      while(state is not null)
      {
        var stateCopy         = state;
        var stateArgumentType = state.GetArgumentTypeSafe();
        if(stateArgumentType is not null) // requires argument
        {
          if(! stateArgumentType.IsAssignableFrom(argumentType)) // but we have not suia
            Throw.NoArgument(state);
          argumentsBag.Add(state, () => ( (ISetArgument<T>)stateCopy ).Argument = initialStateArgument);
        }

        initializeActions.Add(ActivateStateNotGuarded(state, argumentsBag, callEnterActions));

        state = state.ParentState;
      }

      CallEnterActionsInReverseOrder(initializeActions);
    }
    catch(Exception exception)
    {
      _onException(exception);
      throw;
    }
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