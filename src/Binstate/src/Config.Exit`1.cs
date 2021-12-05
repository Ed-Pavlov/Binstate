using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  internal class Exit<T> : Transitions<T>, IExit<T>
  {
    public Exit(StateConfig stateConfig) : base(stateConfig){}

    public ITransitions<T> OnExit(Action exitAction)
    {
      StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
      return this;
    }

    public ITransitions<T> OnExit(Action<T> exitAction)
    {
      StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
      return this;
    }
  }
}