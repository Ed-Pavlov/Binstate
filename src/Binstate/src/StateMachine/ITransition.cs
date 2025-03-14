using System;

namespace Binstate;

internal interface ITransition
{
  void InvokeActionSafe<TArgument>(TArgument argument, Action<Exception> onException);
}