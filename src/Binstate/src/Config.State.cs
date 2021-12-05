using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  internal class State : Enter, IState
  {
    internal State(StateConfig stateConfig) : base(stateConfig) { }

    public IEnter AsSubstateOf(TState parentStateId)
    {
      if(parentStateId is null) throw new ArgumentNullException(nameof(parentStateId));
      StateConfig.ParentStateId = parentStateId.ToMaybe();
      return this;
    }
  }
}