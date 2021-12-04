namespace Binstate;

internal interface IActionInvoker
{
  void Invoke();
}

internal interface IActionInvoker<in T> : IActionInvoker
{
  void Invoke(T argument);
}