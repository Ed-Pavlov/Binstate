using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  internal class Exit : Transitions, IExitEx
  {
    protected Exit(StateConfig stateConfig) : base(stateConfig) { }

    public ITransitions OnExit(Action exitAction)
    {
      StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
      return this;
    }

    public ITransitions<T> OnExit<T>(Action<T> exitAction)
    {
      StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
      StateConfig.Factory = new StateFactory<T>();
      return new Transitions<T>(StateConfig);
    }
  }
}