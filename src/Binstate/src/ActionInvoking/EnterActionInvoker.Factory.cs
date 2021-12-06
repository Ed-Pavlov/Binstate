using System;
using System.Threading.Tasks;

namespace Binstate;

internal static class EnterActionInvokerFactory<TEvent>
{
  /// <summary>
  ///   Synchronous w/o argument
  /// </summary>
  public static EnterActionInvoker<TEvent> Create(Action<IStateController<TEvent>> action) => new EnterActionInvoker<TEvent>(
    stateMachine =>
    {
      action(stateMachine);
      return null;
    }
  );

  /// <summary>
  ///   Synchronous w/ argument
  /// </summary>
  public static EnterActionInvoker<TEvent, TArg> Create<TArg>(Action<IStateController<TEvent>, TArg> action) => new EnterActionInvoker<TEvent, TArg>(
    (stateMachine, arg) =>
    {
      action(stateMachine, arg);

      return null;
    }
  );

  /// <summary>
  ///   Async w/o argument
  /// </summary>
  public static EnterActionInvoker<TEvent> Create(Func<IStateController<TEvent>, Task?> action) => new EnterActionInvoker<TEvent>(action);

  /// <summary>
  ///   Async w/ argument
  /// </summary>
  public static EnterActionInvoker<TEvent, TArgument> Create<TArgument>(Func<IStateController<TEvent>, TArgument, Task?> action)
    => new EnterActionInvoker<TEvent, TArgument>(action);
}