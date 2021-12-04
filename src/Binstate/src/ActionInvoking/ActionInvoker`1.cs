using System;

namespace Binstate;

/// <summary>
/// Generic version of the invoker of an action introduced to avoid boxing in case of Value Type parameter
/// </summary>
internal class ActionInvoker<TArgument> : IActionInvoker<TArgument>
{
  private readonly Action<TArgument> _action;

  public ActionInvoker(Action<TArgument> action) => _action = action;

  public void Invoke(TArgument argument) => _action(argument);
  public void Invoke()                   => throw new NotSupportedException();
}