using System;
using System.Collections.Generic;

namespace Binstate
{
  /// <summary>
  /// This class provides syntax-sugar to configure the state machine.
  /// </summary>
  public static partial class Config<TState, TEvent>
  {
    /// <summary>
    /// This class is used to configure which transitions allowed from the currently configured state. 
    /// </summary>
    public class Transitions
    {
      internal readonly TState StateId;

      /// <summary> Protected ctor </summary>
      protected Transitions(TState stateId) => StateId = stateId ?? throw new ArgumentNullException(nameof(stateId));

      internal readonly Dictionary<TEvent, Transition<TState, TEvent>> TransitionList = new();

      /// <summary>
      /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref> when <paramref name="event"> event is raised</paramref> 
      /// </summary>
      public Transitions AddTransition(TEvent @event, TState stateId, Action? action = null)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(stateId is null) throw new ArgumentNullException(nameof(stateId));

        var getStateWrapper = new GetState<TState>(
          (out TState? state) =>
          {
            state = stateId;

            return true;
          });

        return AddTransition(@event, getStateWrapper, true, action);
      }

#pragma warning disable 1574,1584,1581,1580
      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state.
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns false no transition performed.</param>
      /// <remarks>
      /// Use this overload if you use a value type (e.g. enum) as a <typeparamref name="TState"/> and the default value of the value type as a valid State id.
      /// Otherwise consider using <see cref="AddTransition(TEvent,Func{TState})"/> method as more simple.
      /// </remarks>
#pragma warning restore 1574,1584,1581,1580
      public Transitions AddTransition(TEvent @event, GetState<TState> getState)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(getState is null) throw new ArgumentNullException(nameof(getState));

        return AddTransition(@event, getState, false, null);
      }

#pragma warning disable 1574,1584,1581,1580
      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state. 
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns 'default' value of the type used as <typeparamref name="TState"/> no transition performed.</param>
      /// <remarks>
      /// Use this overload if you use a reference type (class) as a <typeparamref name="TState"/> or the default value of the value type doesn't represent a valid State id.
      /// if you use a value type (e.g. enum) as a <typeparamref name="TState"/> and the default value of the value type as a valid State id you must use
      /// <see cref="AddTransition(TEvent,GetState{TState})"/> method.
      /// </remarks>
#pragma warning restore 1574,1584,1581,1580
      public Transitions AddTransition(TEvent @event, Func<TState?> getState)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(getState is null) throw new ArgumentNullException(nameof(getState));

        var getStateWrapper = new GetState<TState>(
          (out TState? state) =>
          {
            state = getState();

            return !EqualityComparer<TState?>.Default.Equals(state, default);
          });

        return AddTransition(@event, getStateWrapper, false, null);
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(TEvent @event) => AddTransition(@event, StateId);

      private Transitions AddTransition(TEvent @event, GetState<TState> getState, bool isStatic, Action? action)
      {
        if(@event is null) throw new ArgumentNullException(nameof(@event));
        if(getState is null) throw new ArgumentNullException(nameof(getState));

        TransitionList.Add(@event, new Transition<TState, TEvent>(@event, getState, isStatic, action));

        return this;
      }
    }
  }
}
