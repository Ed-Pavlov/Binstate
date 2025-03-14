using System;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class ExitAction : TransitionsEx, IExitActionEx
    {
      protected ExitAction(StateData stateData) : base(stateData) { }

      public ITransitionsEx OnExit(Action exitAction)
      {
        StateData.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }

      public ITransitions<T> OnExit<T>(Action<T> exitAction)
      {
        StateData.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        StateData.Factory    = new StateFactory<T>();
        return new Transitions<T>(StateData);
      }
    }

    internal class ExitAction<T> : Transitions<T>, IExitAction<T>
    {
      public ExitAction(StateData stateData) : base(stateData) { }

      public ITransitions<T> OnExit(Action exitAction)
      {
        StateData.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }

      public ITransitions<T> OnExit(Action<T> exitAction)
      {
        StateData.ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }
    }
  }
}