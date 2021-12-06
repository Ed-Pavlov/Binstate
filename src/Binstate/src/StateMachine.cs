using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Binstate;

/// <summary>
///   The state machine. Use <see cref="Builder{TState, TEvent}" /> to configure and build a state machine.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public partial class StateMachine<TState, TEvent> : IStateMachine<TState, TEvent>
{

  private readonly AutoResetEvent    _lock = new AutoResetEvent(true);
  private readonly Action<Exception> _onException;

  /// <summary>
  ///   The map of all defined states
  /// </summary>
  private readonly Dictionary<TState, IState<TState, TEvent>> _states;

  private volatile IState<TState, TEvent> _activeState;

  internal StateMachine(Dictionary<TState, IState<TState, TEvent>> states, Action<Exception> onException, TState initialStateId)
  {
    _states      = states;
    _onException = onException;
    _activeState = GetStateById(initialStateId);
  }

  /// <inheritdoc />
  public bool Raise(TEvent @event)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionSync(@event, Unit.Default, false);
  }

  /// <inheritdoc />
  public bool Raise<T>(TEvent @event, T argument)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionSync(@event, argument, true);
  }

  /// <inheritdoc />
  public Task<bool> RaiseAsync(TEvent @event)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionAsync<Unit>(@event, default, false);
  }

  /// <inheritdoc />
  public Task<bool> RaiseAsync<T>(TEvent @event, T argument)
  {
    if(@event is null) throw new ArgumentNullException(nameof(@event));

    return PerformTransitionAsync(@event, argument, true);
  }

  internal void EnterInitialState<T>(T initialStateArgument)
  {
    var argumentsBag = new ArgumentsBag { { _activeState, () => ( (IState<T>)_activeState ).Argument = initialStateArgument }, };
    var enterAction  = ActivateStateNotGuarded(_activeState, argumentsBag);
    try
    {
      enterAction();
    }
    catch(Exception exception)
    {
      _onException(exception);
    }
  }

  /// <summary>
  ///   Tell the state machine that it should get an argument attached to the currently active state (or any of parents) and pass it to the newly activated state
  /// </summary>
  /// <typeparam name="TRelay">
  ///   The type of the argument. Should be exactly the same as the generic type passed into
  ///   <see cref="Config{TState,TEvent}.Enter.OnEnter{T}(Action{T})" /> or one of it's overload when configured currently active state (of one of it's parent).
  /// </typeparam>
  /// <param name="relayArgumentIsRequired">
  ///   If there is no active state with argument for relaying:
  ///   true: Raise method throws an exception
  ///   false: state machine will pass default(TRelay) as an argument
  /// </param>
  [Obsolete(
    "Since version 1.2 relaying arguments from the currently active states to states require them performs automatically."
  + "This method is not needed and adds nothing to the behaviour of the state machine."
  )]
  public IStateMachine<TState, TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true) => this;

  private bool PerformTransitionSync<TArgument>(TEvent @event, TArgument argument, bool argumentHasPriority)
  {
    var data = PrepareTransition(@event, argument, argumentHasPriority);

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

  private static IState<TState, TEvent>? FindLeastCommonAncestor(IState<TState, TEvent> left, IState<TState, TEvent> right)
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