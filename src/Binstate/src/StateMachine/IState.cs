using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal interface IState : IArgumentReceiver, IArgumentProvider
{
  IState? ParentState { get; }
  int     DepthInTree { get; }

  /// <summary>
  /// This property is set from protected by lock part of the code so it's no need synchronization
  /// see <see cref="StateMachine{TState,TEvent}.CreateActivateStateNotGuardedAction" /> implementation for details.
  /// </summary>
  bool IsActive { get; set; }

  /// <summary>
  /// <see cref="State{TState,TEvent,TArgument}.ExitSafe" /> can be called earlier then <see cref="Builder{TState,TEvent}.ConfiguratorOf.IEnterAction" /> of the activated state,
  /// see <see cref="StateMachine{TState,TEvent}.PerformTransition" /> implementation for details.
  /// In this case it should wait till <see cref="Builder{TState,TEvent}.ConfiguratorOf.IEnterAction" /> be called and exited, before call exit action
  /// </summary>
  void ExitSafe(Action<Exception> onException);

  void CallTransitionActionSafe(ITransition transition, Action<Exception> onException);

  Maybe<object?> GetArgumentAsObject();
}

internal interface IState<TState, TEvent> : IState
{
  TState Id { get; }

  new IState<TState, TEvent>? ParentState { get; }

  IReadOnlyDictionary<TEvent, ITransition<TState, TEvent>> Transitions { get; }

  bool FindTransitionTransitive(TEvent @event, [NotNullWhen(true)] out ITransition<TState, TEvent>? transition);

  void EnterSafe(IStateController<TEvent> stateController, Action<Exception> onException);
}