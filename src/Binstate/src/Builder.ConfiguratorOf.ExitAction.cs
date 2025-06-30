using System;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class ExitAction : Transitions, IExitAction
    {
      protected ExitAction(StateConfig<Unit> stateConfig) : base(stateConfig) { }

      public ITransitions OnExit(Action exitAction)
      {
        if(exitAction is null) throw new ArgumentNullException(nameof(exitAction));
        StateConfig.ExitAction = State<TState, TEvent, Unit>.ExitAction.Create(exitAction);
        return this;
      }
    }

    internal class ExitAction<TStateArgument> : Transitions<TStateArgument>, IExitAction<TStateArgument>
    {
      public ExitAction(StateConfig<TStateArgument> stateConfig) : base(stateConfig) { }

      public ITransitions<TStateArgument> OnExit(Action exitAction)
      {
        if(exitAction is null) throw new ArgumentNullException(nameof(exitAction));
        StateConfig.ExitAction = State<TState, TEvent, TStateArgument>.ExitAction.Create(exitAction);
        return this;
      }

      public ITransitions<TStateArgument> OnExit(Action<TStateArgument> exitAction)
      {
        if(exitAction is null) throw new ArgumentNullException(nameof(exitAction));
        StateConfig.ExitAction = State<TState, TEvent, TStateArgument>.ExitAction.Create(exitAction);
        return this;
      }
    }
  }
}