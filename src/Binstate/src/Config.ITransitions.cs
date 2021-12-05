using System;

namespace Binstate;

public static partial class Config<TState, TEvent>
{

  /// <summary>
    ///   This interface is used to configure which transitions allowed from the currently configured state.
    /// </summary>
    public interface ITransitions
  {
      /// <summary>
      ///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref> when <paramref name="event"> event is raised </paramref>
      /// </summary>
      ITransitions AddTransition(TEvent @event, TState stateId, Action? action = null);


#pragma warning disable 1574,1584,1581,1580
      /// <summary>
      ///   Defines transition from the currently configured state to the state calculated dynamically depending on other application state.
      /// </summary>
      /// <param name="event"> </param>
      /// <param name="getState"> If getState returns false no transition performed. </param>
      /// <remarks>
      ///   Use this overload if you use a value type (e.g. enum) as a <typeparamref name="TState" /> and the default value of the value type as a valid State id.
      ///   Otherwise consider using <see cref="Transitions.AddTransition(TEvent,System.Func{TState?})" /> method as more simple.
      /// </remarks>
#pragma warning restore 1574,1584,1581,1580
    ITransitions AddTransition(TEvent @event, GetState<TState> getState);

#pragma warning disable 1574,1584,1581,1580
      /// <summary>
      ///   Defines transition from the currently configured state to the state calculated dynamically depending on other application state.
      /// </summary>
      /// <param name="event"> </param>
      /// <param name="getState"> If getState returns 'default' value of the type used as <typeparamref name="TState" /> no transition performed. </param>
      /// <remarks>
      ///   Use this overload if you use a reference type (class) as a <typeparamref name="TState" /> or the default value of the value type doesn't represent
      ///   a valid State id. If you use a value type (e.g. enum) as a <typeparamref name="TState" /> and the default value of the value type is a valid State id
      ///   you must use <see cref="Transitions.AddTransition(TEvent,Binstate.GetState{TState})" /> method.
      /// </remarks>
#pragma warning restore 1574,1584,1581,1580
    ITransitions AddTransition(TEvent @event, Func<TState?> getState);

      /// <summary>
      ///   Defines transition from the state to itself when
      ///   <param name="event"> is raised. Exit and enter actions are called in case of such transition. </param>
      /// </summary>
      void AllowReentrancy(TEvent @event);
  }

  /// <summary>
  ///
  /// </summary>
  public interface ITransitionsEx : ITransitions
  {
    /// <summary>
    ///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref>
    ///   when <paramref name="event"> event is raised </paramref>
    /// </summary>
    ITransitions<T> AddTransition<T>(TEvent @event, TState stateId, Action<T> action);
  }

  /// <summary>
  ///   This interface is used to configure which transitions allowed from the currently configured state.
  /// </summary>
  public interface ITransitions<out T> : ITransitions
  {
    /// <summary>
    ///   Defines transition from the currently configured state to the <paramref name="stateId"> specified state </paramref>
    ///   when <paramref name="event"> event is raised </paramref>
    /// </summary>
    ITransitions<T> AddTransition(TEvent @event, TState stateId, Action<T> action);
  }
}