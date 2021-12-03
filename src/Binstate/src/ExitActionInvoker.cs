using System;

namespace Binstate;

internal class NoParameterExitActionActionInvoker : IExitActionInvoker
{
  private readonly Action _action;

  public NoParameterExitActionActionInvoker(Action action) => _action = action;

  public void Invoke() => _action();
}
  
/// <summary>
/// Generic version of the invoker of exit action introduced to avoid boxing in case of Value Type parameter
/// </summary>
internal class ExitActionInvoker<TArgument> : IExitActionInvoker
{
  private readonly Action<TArgument> _action;

  public ExitActionInvoker(Action<TArgument> action) => _action = action;

  public void Invoke(TArgument argument) => _action(argument);
}