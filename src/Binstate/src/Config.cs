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
      internal readonly object State;

      
      /// <summary>
      /// Protected ctor
      /// </summary>
      protected Transitions(object state) => State = state;

      internal readonly List<Transition> TransitionList = new List<Transition>();

      /// <summary>
      /// Allows transition from the currently configured state to the <param name="state"> specified state</param> when <param name="event"> event is triggered</param> 
      /// </summary>
      public Transitions AddTransition([NotNull] object @event, [NotNull] object state)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (state == null) throw new ArgumentNullException(nameof(state));
        
        TransitionList.Add(new Transition(@event, null, state, false));
        return this;
      }

      /// <summary>
      /// Allows transition from the currently configured state to the <paramref name="state"> specified state</paramref> when
      /// <paramref name="event"> event is triggered</paramref>
      /// Use this overload if target state on enter action requires an input parameter. 
      /// </summary>
      /// <typeparam name="TParameter">The type of the input parameter required by the target state of the transition</typeparam>
      public Transitions AddTransition<TParameter>([NotNull] object @event, [NotNull] object state, bool parameterCanBeNull = false)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));
        if (state == null) throw new ArgumentNullException(nameof(state));
        
        TransitionList.Add(new Transition(@event, typeof(TParameter), state, parameterCanBeNull));
        return this;
      }

      /// <summary>
      /// Allows transition from the state to itself when <param name="event"> is triggered. Entering action is called in case of such transition.</param>
      /// </summary>
      public Transitions AllowReentrancy(object @event)
      {
        TransitionList	.Add	(new Transition(@event, null, State, false));
        return this;
      }
    }
    
    /// <summary>
    /// This class is used to configure exiting action of the currently configured state.
    /// </summary>
    public class Exiting : Transitions
    {
      [CanBeNull] internal EnterInvoker Enter;
      [CanBeNull] internal Action Exit;

      /// <inheritdoc />
      protected Exiting(object state) : base(state) { }
      
      /// <summary>
      /// Specifies the action to be called on exiting the currently configured state.
      /// </summary>
      public Transitions OnExit([NotNull] Action onExit)
      {
        Exit = onExit ?? throw new ArgumentNullException(nameof(onExit));
        return this;
      }
    }

    /// <inheritdoc />
    public class Entering : Exiting
    {
      private const string asyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      internal Entering([NotNull] object state) : base(state){}

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide blocking action. To provide async action use <see cref="OnEnter(Func{IStateMachine, Task})"/>.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exiting OnEnter([NotNull] Action<IStateMachine> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        if(IsAsyncMethod(onEnter.Method)) throw new InvalidOperationException(asyncVoidMethodNotSupported);

        Enter = EnterInvoker.Create(onEnter);
        return this;
      }

      /// <summary>
      /// Specifies the action to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exiting OnEnter([NotNull] Func<IStateMachine, Task> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        
        Enter = EnterInvoker.Create(onEnter);
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
      public Exiting OnEnter<T>([NotNull] Action<IStateMachine, T> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        if(IsAsyncMethod(onEnter.Method)) throw new InvalidOperationException(asyncVoidMethodNotSupported);
      
        Enter = EnterInvoker.Create(onEnter);
        return this;
      }

      /// <summary>
      /// Specifies the action with parameter to be called on entering the currently configured state.
      /// This overload is used to provide non-blocking async action.
      /// </summary>
      /// <remarks>Do not use async void methods, async methods should return <see cref="Task"/></remarks>
      public Exiting OnEnter<T>([NotNull] Func<IStateMachine, T, Task> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        
        Enter = EnterInvoker.Create(onEnter);
        return this;
      }
    
      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }
  }
}