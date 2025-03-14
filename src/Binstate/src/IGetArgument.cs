using System;
using System.Collections.Generic;

namespace Binstate;

internal interface IState : IArgumentProvider
{
  IState? ParentState { get; }
  int     DepthInTree { get; }

  /// <summary>
  /// This property is set from protected by lock part of the code so it's no need synchronization
  /// see <see cref="StateMachine{TState,TEvent}.ActivateStateNotGuarded" /> implementation for details.
  /// </summary>
  bool IsActive { get; set; }

  void EnterSafe<TEvent>(IStateController<TEvent> stateController, Action<Exception> onException);

  /// <summary>
  /// <see cref="State{TState,TEvent,TArgument}.ExitSafe" /> can be called earlier then <see cref="Config{TState,TEvent}.Enter" /> of the activated state,
  /// see <see cref="StateMachine{TState,TEvent}.PerformTransition" /> implementation for details.
  /// In this case it should wait till <see cref="Config{TState,TEvent}.Enter" /> be called and exited, before call exit action
  /// </summary>
  void ExitSafe(Action<Exception> onException);

  void CallTransitionActionSafe(ITransition transition, Action<Exception> onException);
}

internal interface IState<TState, TEvent> : IState
{
  TState Id { get; }

  new IState<TState, TEvent>? ParentState { get; }

  Dictionary<TEvent, Transition<TState, TEvent>> Transitions { get; }

  bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent>? transition);
}