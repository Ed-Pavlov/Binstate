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
  public static class Config<TState, TEvent>
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
      /// Defines transition from the currently configured state to the <param name="stateId"> specified state</param> when <param name="event"> event is raised</param> 
      /// </summary>
      public Transitions AddTransition([NotNull] TEvent @event, [NotNull] TState stateId)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (stateId == null) throw new ArgumentNullException(nameof(stateId));

        return AddTransition(@event, () => stateId, true, null, false);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state. 
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns null no transition executed</param>
      public Transitions AddTransition([NotNull] TEvent @event, [NotNull] Func<TState> getState)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (getState == null) throw new ArgumentNullException(nameof(getState));
        
        return AddTransition(@event, getState, false, null, false);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref> when
      /// <paramref name="event"> event is raised</paramref>
      /// Use this overload if target state enter action requires an input parameter.
      /// </summary>
      public Transitions AddTransition<TArgument>([NotNull] TEvent @event, [NotNull] TState stateId, bool argumentCanBeNull = false)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (stateId == null) throw new ArgumentNullException(nameof(stateId));
        
        return AddTransition(@event, () => stateId, true, typeof(TArgument), argumentCanBeNull);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state. 
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns null no transition executed</param>
      /// <param name="argumentCanBeNull">True if argument passed to enter action can be null</param>
      public Transitions AddTransition<TArgument>([NotNull] TEvent @event, [NotNull] Func<TState> getState, bool argumentCanBeNull = false)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (getState == null) throw new ArgumentNullException(nameof(getState));
        
        return AddTransition(@event, getState, false, typeof(TArgument), argumentCanBeNull);
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(TEvent @event) => AddTransition(@event, () => StateId, true, null, false);
      
      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// Use this overload if the state enter action requires an input parameter.
      /// </summary>
      public void AllowReentrancy<TArgument>(TEvent @event, bool argumentCanBeNull = false) => AddTransition(@event, () => StateId, true, typeof(TArgument), argumentCanBeNull);
      
      private Transitions AddTransition([NotNull] TEvent @event, [NotNull] Func<TState> getState, bool isStatic, Type argumentType, bool argumentCanBeNull)
      {
        TransitionList.Add(new Transition<TState, TEvent>(@event, argumentType, getState, isStatic, argumentCanBeNull));
        return this;
      }
    }
    
    /// <summary>
    /// This class is used to configure exit action of the currently configured state.
    /// </summary>
    public class Exit : Transitions
    {
      [CanBeNull] internal EnterActionInvoker<TEvent> EnterAction;
      [CanBeNull] internal Action ExitAction;

      /// <inheritdoc />
      protected Exit(TState stateId) : base(stateId) { }
      
      /// <summary>
      /// Specifies the action to be called on exiting the currently configured state.
      /// </summary>
      public Transitions OnExit([NotNull] Action exitAction)
      {
        ExitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        return this;
      }
    }

    /// <summary>
    /// This class is used to configure enter action of the currently configured state.
    /// </summary>
    public class Enter : Exit
    {
      private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      internal Enter([NotNull] TState stateId) : base(stateId){}

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine{TEvent}, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Action<IStateMachine<TEvent>> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        EnterAction = EnterActionInvoker<TEvent>.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Func<IStateMachine<TEvent>, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvoker<TEvent>.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use
      /// <see>
      ///   <cref>OnEnter(Func{IStateMachine, T, Task})</cref>
      /// </see>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>([NotNull] Action<IStateMachine<TEvent>, TArgument> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
      
        EnterAction = EnterActionInvoker<TEvent>.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>([NotNull] Func<IStateMachine<TEvent>, TArgument, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvoker<TEvent>.Create(enterAction);
        return this;
      }
    
      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }
  }
}