using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  /// This interface is used to configure which transitions allowed from the currently configured state. 
  /// </summary>
  public interface ITransitions<out T> : ITransitions
  {
    /// <summary>
    /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref>
    /// when <paramref name="event"> event is raised</paramref> 
    /// </summary>
    ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T>? action = null);
  }
}