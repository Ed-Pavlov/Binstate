using System;

namespace Binstate;

internal class ActionInvoker : IActionInvoker
{
  private readonly Action _action;

  public ActionInvoker(Action action) => _action = action;

  public void Invoke() => _action();
}