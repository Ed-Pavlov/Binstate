using System;
using System.Collections.Generic;

namespace Binstate;

/// <summary>
/// </summary>
internal interface IState { }

/// <summary>
///
/// </summary>
/// <typeparam name="TArgument"></typeparam>
internal interface IState<TArgument>
{
  TArgument Argument { get; set; }
}

/// <summary>
/// </summary>
/// <typeparam name="TState"> </typeparam>
/// <typeparam name="TEvent"> </typeparam>
internal interface IState<TState, TEvent> : IState
{
  /// <summary>
  /// </summary>
  int DepthInTree { get; }

  /// <summary>
  ///   This property is set from protected by lock part of the code so it's no need synchronization
  ///   see <see cref="StateMachine{TState,TEvent}.ActivateStateNotGuarded" /> implementation for details.
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
  /// </summary>
  /// <param name="stateController"> </param>
  /// <param name="onException"> </param>
  void EnterSafe(IStateController<TEvent> stateController, Action<Exception> onException);

  /// <summary>
  ///   <see cref="State{TState,TEvent,TArgument}.ExitSafe" /> can be called earlier then <see cref="Config{TState,TEvent}.Enter" /> of the activated state,
  ///   see <see cref="StateMachine{TState,TEvent}.PerformTransition" /> implementation for details.
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
internal interface IState<TState, TEvent, TArgument> : IState<TState, TEvent>, IState<TArgument>
{
}