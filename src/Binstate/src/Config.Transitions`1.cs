using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  /// It inherits <see cref="Transitions"/> therefore allows all transitions w/o Argument.
  /// But doesn't inherit <see cref="ITransitionsEx"/> therefore doesn't allow changing the type of Argument.
  /// </summary>
  internal class Transitions<T> : Transitions, ITransitions<T>
  {
    public Transitions(StateConfig stateConfig) : base(stateConfig) { }

    /// <summary>
    /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref>
    /// when <paramref name="event"> event is raised </paramref>
    /// </summary>
    public ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T> action)
    {
      if(@event is null) throw new ArgumentNullException(nameof(@event));
      if(stateId is null) throw new ArgumentNullException(nameof(stateId));
      if(action == null) throw new ArgumentNullException(nameof(action));

      AddTransitionToList(@event, StaticGetState(stateId), true, action);
      return this;
    }
  }
}