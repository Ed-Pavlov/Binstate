using System;

namespace Binstate;

internal static class ExitActionInvokerFactory
{
  public static NoParameterExitActionActionInvoker Create(Action             action) => new NoParameterExitActionActionInvoker(action);
  public static ExitActionInvoker<TArg>            Create<TArg>(Action<TArg> action) => new ExitActionInvoker<TArg>(action);
}