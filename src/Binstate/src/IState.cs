using System;

namespace Binstate
{
  /// <summary>
  /// This interface is used to make <typeparamref name="TArgument"/> contravariant.
  /// </summary>

  // ReSharper disable once UnusedTypeParameter
  internal interface IState<TState, out TEvent, in TArgument>
  {
    void EnterSafe(IStateMachine<TEvent> stateMachine, TArgument? argument, Action<Exception> onException);
  }
}
