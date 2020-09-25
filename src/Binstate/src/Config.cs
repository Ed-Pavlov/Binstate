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
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        if (stateId.IsNull()) throw new ArgumentNullException(nameof(stateId));

        return AddTransition(@event, () => stateId, true);
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
        
        return AddTransition(@event, getState, false);
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(TEvent @event) => AddTransition(@event, () => StateId, true);
      
      private Transitions AddTransition(TEvent @event, Func<TState> getState, bool isStatic)
      {
        TransitionList.Add(new Transition<TState, TEvent>(@event, getState, isStatic));
        return this;
      }
    }
    
    /// <summary>
    /// This class is used to configure exit action of the currently configured state.
    /// </summary>
    public class Exit : Transitions
    {
      [CanBeNull] 
      internal Action ExitAction;

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
      internal const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      [CanBeNull] 
      internal IEnterInvoker<TEvent> EnterAction;
      
      [CanBeNull]
      internal Type EnterArgumentType;
      
      internal Enter(TState stateId) : base(stateId){}

      /// <summary>
      /// Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine{TEvent}, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Action enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        return OnEnter(_ => enterAction());
      }

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine{TEvent}, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Action<IStateMachine<TEvent>> enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Func<Task> enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvokerFactory<TEvent>.Create(_ => enterAction());
        return this;
      }
      
      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Func<IStateMachine<TEvent>, Task> enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the simple action with parameter to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide blocking action. To provide async action use
      /// <see>
      ///   <cref>OnEnter(Func{IStateMachine, T, Task})</cref>
      /// </see>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>([NotNull] Action<TArgument> enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        return OnEnter<TArgument>((_, argument) => enterAction(argument));
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
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
      
        EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        EnterArgumentType = typeof(TArgument);
        return this;
      }

      /// <summary>
      /// Specifies the simple action with parameter to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>([NotNull] Func<TArgument, Task> enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvokerFactory<TEvent>.Create<TArgument>((_, arg) => enterAction(arg));
        EnterArgumentType = typeof(TArgument);
        return this;
      }
      
      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>([NotNull] Func<IStateMachine<TEvent>, TArgument, Task> enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        
        EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        EnterArgumentType = typeof(TArgument);
        return this;
      }
    
      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }

    /// <summary>
    /// This class is used to configure composite states. 
    /// </summary>
    public class Substate : Enter
    {
      internal TState ParentStateId; 
      
      internal Substate(TState stateId) : base(stateId)
      { }

      /// <summary>
      /// Defines the currently configured state as a subset of a composite state 
      /// </summary>
      public Enter AsSubstateOf(TState parentStateId)
      {
        ParentStateId = parentStateId;
        return this;
      }
    }
  }
}