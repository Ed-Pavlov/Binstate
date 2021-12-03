namespace Binstate;

internal interface IExitActionInvoker
{
}

internal interface IExitActionInvoker<in TArgument> : IExitActionInvoker
{
  void Invoke(TArgument argument);
}