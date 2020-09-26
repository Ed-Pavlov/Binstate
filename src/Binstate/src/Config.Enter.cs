using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

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

      [CanBeNull]
      internal IEnterInvoker<TEvent> EnterAction;

      internal Enter(TState stateId) : base(stateId)
      {
      }

      /// <summary>
      /// Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
      /// or transition to another one is not needed.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine{TEvent}, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exit OnEnter([NotNull] Action enterAction)
      {
        if (enterAction.IsNull()) throw new ArgumentNullException(nameof(enterAction));
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

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
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

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
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

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
        if (IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
        _stateFactory = new StateFactory<TArgument>();
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
        _stateFactory = new StateFactory<TArgument>();
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
        _stateFactory = new StateFactory<TArgument>();
        return this;
      }

      internal State<TState, TEvent> CreateState(State<TState, TEvent> parentState) => _stateFactory.CreateState(this, parentState);

      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }
  }
}