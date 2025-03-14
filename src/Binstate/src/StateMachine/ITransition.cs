using System;

namespace BeatyBit.Binstate;

internal interface ITransition
{
  void InvokeActionSafe<TArgument>(TArgument argument, Action<Exception> onException);
}