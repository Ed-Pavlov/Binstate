﻿using System;
using System.Threading.Tasks;

namespace Binstate
{
  internal static class EnterActionInvokerFactory<TEvent>
  {
    public static NoParameterEnterActionInvoker<TEvent> Create(Action<IStateMachine<TEvent>> action) =>
      new NoParameterEnterActionInvoker<TEvent>(stateMachine =>
        {
          action(stateMachine);
          return null;
        });

    public static NoParameterEnterActionInvoker<TEvent> Create(Func<IStateMachine<TEvent>, Task> action) =>
      new NoParameterEnterActionInvoker<TEvent>(action);

    public static EnterActionInvoker<TEvent, TArg> Create<TArg>(Action<IStateMachine<TEvent>, TArg> action) =>
      new EnterActionInvoker<TEvent, TArg>((stateMachine, arg) =>
        {
          action(stateMachine, arg);
          return null;
        });

    public static EnterActionInvoker<TEvent, TArg> Create<TArg>(Func<IStateMachine<TEvent>, TArg, Task> action) =>
      new EnterActionInvoker<TEvent, TArg>(action);
  }
}