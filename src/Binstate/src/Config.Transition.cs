using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        if (stateId.IsNull()) throw new ArgumentNullException(nameof(stateId));

        return AddTransition(@event, () => stateId, true, action);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state. 
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns null no transition executed</param>
      public Transitions AddTransition([NotNull] TEvent @event, [NotNull] Func<TState> getState)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        if (getState.IsNull()) throw new ArgumentNullException(nameof(getState));
        
        return AddTransition(@event, getState, false, null);
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(TEvent @event) => AddTransition(@event, () => StateId, true, null);
      
      private Transitions AddTransition(TEvent @event, Func<TState> getState, bool isStatic, [CanBeNull] Action action)
      {
        TransitionList.Add(new Transition<TState, TEvent>(@event, getState, isStatic, action));
        return this;
      }
    }
    
  }
}