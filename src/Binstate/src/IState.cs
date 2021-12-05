using System;
using System.Collections.Generic;

namespace Binstate;

/// <summary>
/// </summary>
public interface IState { }

/// <summary>
/// </summary>
/// <typeparam name="TState"> </typeparam>
/// <typeparam name="TEvent"> </typeparam>
public interface IState<TState, TEvent> : IState
{
  /// <summary>
  /// </summary>
  int DepthInTree { get; }

  /// <summary>
  ///   This property is set from protected by lock part of the code so it's no need synchronization
  ///   see <see cref="StateMachine{TState,TEvent}.ActivateStateNotGuarded{TArgument,TRelay}" /> implementation for details.
  /// </summary>
  bool IsActive { get; set; }

  /// <summary>
  /// </summary>
  IState<TState, TEvent>? ParentState { get; }

  /// <summary>
  /// </summary>
  Dictionary<TEvent, Transition<TState, TEvent>> Transitions { get; }

  /// <summary>
  /// </summary>
  TState Id { get; }

  /// <summary>
  ///   <see cref="State{TState,TEvent,TArgument}.ExitSafe" /> can be called earlier then <see cref="Config{TState,TEvent}.Enter" /> of the activated state,
  ///   see <see cref="StateMachine{TState,TEvent}.PerformTransition{TArgument, TRelay}" /> implementation for details.
  ///   In this case it should wait till <see cref="Config{TState,TEvent}.Enter" /> will be called and exited, before call exit action
  /// </summary>
  void ExitSafe(Action<Exception> onException);

  /// <summary>
  /// </summary>
  /// <param name="event"> </param>
  /// <param name="transition"> </param>
  /// <returns> </returns>
  bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent>? transition);

  /// <summary>
  /// </summary>
  /// <param name="transition"> </param>
  /// <param name="onException"> </param>
  void CallTransitionActionSafe(Transition<TState, TEvent> transition, Action<Exception> onException);
}

/// <summary>
///   This interface is used to make <typeparamref name="TArgument" /> contravariant.
/// </summary>
public interface IState<TState, TEvent, in TArgument> : IState<TState, TEvent>
{
  /// <summary>
  /// </summary>
  /// <param name="stateMachine"> </param>
  /// <param name="argument"> </param>
  /// <param name="onException"> </param>
  void EnterSafe(IStateMachine<TEvent> stateMachine, TArgument argument, Action<Exception> onException);
}