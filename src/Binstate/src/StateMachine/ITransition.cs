using System;
using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

internal interface ITransition
{
  bool                IsStatic         { get; }
  bool                IsReentrant      { get; }
  Tuple<Type?, Type?> ArgumentTypes    { get; }
}

// ReSharper disable once TypeParameterCanBeVariant
internal interface ITransition<TState, TEvent> : ITransition
{
  TEvent Event { get; }

  //TODO: this method is used only from Builder, should it be here?
  TState GetTargetStateId();
  bool   GetTargetStateId(TState sourceState, Tuple<IArgumentProvider?, IArgumentProvider?> argumentProviders, [NotNullWhen(true)] out TState? targetStateId);

  void CallActionSafe(TState sourceState, TState targetState, Tuple<IArgumentProvider?, IArgumentProvider?> transitionArgumentsProviders);
}