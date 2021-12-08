using System;
using System.Collections.Generic;

namespace Binstate;

internal interface IState : IArgumentProvider{ }

internal interface IState<TState, TEvent> : IState
{
  TState Id { get; }

  int DepthInTree { get; }

  /// <summary>
  ///   This property is set from protected by lock part of the code so it's no need synchronization
  ///   see <see cref="StateMachine{TState,TEvent}.ActivateStateNotGuarded" /> implementation for details.
  /// </summary>
  bool IsActive { get; set; }

  IState<TState, TEvent>? ParentState { get; }

  Dictionary<TEvent, Transition<TState, TEvent>> Transitions { get; }


  void EnterSafe(IStateController<TEvent> stateController, Action<Exception> onException);

  /// <summary>
  ///   <see cref="State{TState,TEvent,TArgument}.ExitSafe" /> can be called earlier then <see cref="Config{TState,TEvent}.Enter" /> of the activated state,
  ///   see <see cref="StateMachine{TState,TEvent}.PerformTransition" /> implementation for details.
  ///   In this case it should wait till <see cref="Config{TState,TEvent}.Enter" /> will be called and exited, before call exit action
  /// </summary>
  void ExitSafe(Action<Exception> onException);

  bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent>? transition);

  void CallTransitionActionSafe(Transition<TState, TEvent> transition, Action<Exception> onException);
}