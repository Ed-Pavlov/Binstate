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
  public static class Config
  {
    /// <summary>
    /// This class is used to configure which transitions allowed from the currently configured state. 
    /// </summary>
    public class Transitions
    {
      internal readonly object StateId;
      
      /// <summary>
      /// Protected ctor
      /// </summary>
      protected Transitions(object stateId) => StateId = stateId;

      internal readonly List<Transition> TransitionList = new List<Transition>();

      /// <summary>
      /// Defines transition from the currently configured state to the <param name="stateId"> specified state</param> when <param name="event"> event is raised</param> 
      /// </summary>
      public Transitions AddTransition([NotNull] object @event, [NotNull] object stateId)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (stateId == null) throw new ArgumentNullException(nameof(stateId));
        
        TransitionList.Add(new Transition(@event, null, stateId, false));
        return this;
      }

      /// <summary>
      /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref> when
      /// <paramref name="event"> event is raised</paramref>
      /// Use this overload if target state enter action requires an input parameter.
      /// </summary>
      public Transitions AddTransition<TParameter>([NotNull] object @event, [NotNull] object stateId, bool parameterCanBeNull = false)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (stateId == null) throw new ArgumentNullException(nameof(stateId));
        
        TransitionList.Add(new Transition(@event, typeof(TParameter), stateId, parameterCanBeNull));
        return this;
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(object @event) => TransitionList.Add	(new Transition(@event, null, StateId, false));
    }
    
    /// <summary>
    /// This class is used to configure exit action of the currently configured state.
    /// </summary>
    public class Exit : Transitions
    {
      [CanBeNull] internal EnterActionInvoker EnterAction;
      [CanBeNull] internal Action ExitAction;

      /// <inheritdoc />
      protected Exit(object stateId) : base(stateId) { }
      
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
      private const string asyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      internal Enter([NotNull] object stateId) : base(stateId){}

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Action<IStateMachine> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(asyncVoidMethodNotSupported);

        EnterAction = EnterActionInvoker.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Func<IStateMachine, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvoker.Create(enterAction);
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
      public Exit OnEnter<T>([NotNull] Action<IStateMachine, T> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(asyncVoidMethodNotSupported);
      
        EnterAction = EnterActionInvoker.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<T>([NotNull] Func<IStateMachine, T, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvoker.Create(enterAction);
        return this;
      }
    
      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }
  }
}