using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Binstate
{
  public static partial class Config<TState, TEvent>
  {
    /// <summary>
    /// This class is used to configure enter action of the currently configured state.
    /// </summary>
    public class Enter : Exit
    {
      private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      private IStateFactory _stateFactory = new NoArgumentStateFactory();

      internal IEnterActionInvoker? EnterActionInvoker;

      internal Enter(TState stateId) : base(stateId)
      {
      }

      /// <summary>
      /// Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine{TEvent}, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter(Action enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        return OnEnter(_ => enterAction());
      }

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine{TEvent}, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter(Action<IStateMachine<TEvent>> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        EnterActionInvoker = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        return this;
      }

      /// <summary>
      /// Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter(Func<Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        return OnEnter(_ => enterAction());
      }

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter(Func<IStateMachine<TEvent>, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));

        EnterActionInvoker = EnterActionInvokerFactory<TEvent>.Create(enterAction);
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
      public Exit OnEnter<TArgument>(Action<TArgument?> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        return OnEnter<TArgument>((_, argument) => enterAction(argument));
      }

#pragma warning disable 1574
      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use
      /// <see cref="OnEnter{TArgument}(Func{IStateMachine{TEvent}, TArgument, Task})"/>
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
#pragma warning restore 1574
      public Exit OnEnter<TArgument>(Action<IStateMachine<TEvent>, TArgument?> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        EnterActionInvoker = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        _stateFactory = new StateFactory<TArgument>();
        return this;
      }
      
      /// <summary>
      /// Specifies the simple action with parameter to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>(Func<TArgument?, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        return OnEnter<TArgument>((_, argument) => enterAction(argument));
      }

      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument>(Func<IStateMachine<TEvent>, TArgument?, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));

        EnterActionInvoker = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        _stateFactory = new StateFactory<TArgument>();
        return this;
      }

      /// <summary>
      /// Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}"/>
      /// and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}"/>
      /// to be called on entering the currently configured state for case when controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide blocking action. To provide async action use
      /// <see cref="OnEnter{TArgument, TRelay}(Func{TArgument, TRelay, Task})"/>
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument, TRelay>(Action<TArgument?, TRelay?> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
        return OnEnter<ITuple<TArgument, TRelay>>(tuple => enterAction(tuple!.PassedArgument, tuple.RelayedArgument));
      }

#pragma warning disable 1574
      /// <summary>
      /// Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}"/>
      /// and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}"/>
      /// to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use
      /// <see cref="OnEnter{TArgument, TRelay}(Func{IStateMachine{TEvent}, TArgument, TRelay, Task})"/>
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
#pragma warning restore 1574
      public Exit OnEnter<TArgument, TRelay>(Action<IStateMachine<TEvent>, TArgument?, TRelay?> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
        return OnEnter<ITuple<TArgument, TRelay>>((stateMachine, tuple) => enterAction(stateMachine, tuple!.PassedArgument, tuple.RelayedArgument));
      }
      
      /// <summary>
      /// Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}"/>
      /// and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}"/>
      /// to be called on entering the currently configured state for case when controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument, TRelay>(Func<TArgument?, TRelay?, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        return OnEnter<ITuple<TArgument, TRelay>>(tuple => enterAction(tuple!.PassedArgument, tuple.RelayedArgument));
      }

      /// <summary>
      /// Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}"/>
      /// and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}"/>
      /// to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter<TArgument, TRelay>(Func<IStateMachine<TEvent>, TArgument?, TRelay?, Task> enterAction)
      {
        if (enterAction == null) throw new ArgumentNullException(nameof(enterAction));
        return OnEnter<ITuple<TArgument, TRelay>>((stateMachine, tuple) => enterAction(stateMachine, tuple!.PassedArgument, tuple.RelayedArgument));
      }
      
      internal State<TState, TEvent> CreateState(State<TState, TEvent>? parentState) => _stateFactory.CreateState(this, parentState);

      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }
  }
}