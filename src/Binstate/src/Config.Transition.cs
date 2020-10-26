using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
      
      /// <summary>
      /// Protected ctor
      /// </summary>
      protected Transitions(TState stateId) => StateId = stateId;

      internal readonly List<Transition<TState, TEvent>> TransitionList = new List<Transition<TState, TEvent>>();

      /// <summary>
      /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref> when <paramref name="event"> event is raised</paramref> 
      /// </summary>
      public Transitions AddTransition([NotNull] TEvent @event, [NotNull] TState stateId, Action action = null)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (stateId == null) throw new ArgumentNullException(nameof(stateId));

        bool GetStateWrapper(out TState state)
        {
          state = stateId;
          return true;
        }
        
        return AddTransition(@event, GetStateWrapper, true, action);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state.
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns false no transition performed.</param>
      /// <remarks>
      /// Use this overload if you use a value type (e.g. enum) as a <typeparamref name="TState"/> and the default value of the value type as a valid State id.
      /// Otherwise consider using <see cref="AddTransition(TEvent,Func{TState})"/> method as more simple.
      /// </remarks>
      public Transitions AddTransition([NotNull] TEvent @event, [NotNull] GetState<TState> getState)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (getState == null) throw new ArgumentNullException(nameof(getState));

        return AddTransition(@event, getState, false, null);
      }

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
      public Transitions AddTransition([NotNull] TEvent @event, [NotNull] Func<TState> getState)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (getState == null) throw new ArgumentNullException(nameof(getState));

        bool GetStateWrapper(out TState state)
        {
          state = getState();
          return !EqualityComparer<TState>.Default.Equals(state, default); // generics in C# sucks
        }

        return AddTransition(@event, GetStateWrapper, false, null);
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(TEvent @event) => AddTransition(@event, StateId);
      
      private Transitions AddTransition(TEvent @event, GetState<TState> getState, bool isStatic, [CanBeNull] Action action)
      {
        TransitionList.Add(new Transition<TState, TEvent>(@event, getState, isStatic, action));
        return this;
      }
      
      internal Dictionary<TEvent, Transition<TState, TEvent>> CreateTransitions()
      {
        var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
        foreach (var transition in TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{StateId}'");

          transitions.Add(transition.Event, transition);
        }

        return transitions;
      }
    }
  }
}