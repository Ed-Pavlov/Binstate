using System;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class ExitAction : Transitions, IExitAction
    {
      protected ExitAction(StateConfig stateConfig) : base(stateConfig) { }

      public ITransitions OnExit(Action? exitAction = null)
      {
        StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }
    }

    internal class ExitAction<TStateArgument> : Transitions<TStateArgument>, IExitAction<TStateArgument>
    {
      public ExitAction(StateConfig stateConfig) : base(stateConfig) { }

      public ITransitions<TStateArgument> OnExit(Action? exitAction = null)
      {
        StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }

      public ITransitions<TStateArgument> OnExit(Action<TStateArgument> exitAction)
      {
        StateConfig.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }
    }
  }
}