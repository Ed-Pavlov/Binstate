using System;

namespace BeatyBit.Binstate;

internal interface IArgumentBag
{
  void Add(IState state, Action<IState> setArgument);

  Action<IState>? GetValueSafe(IState state);
}