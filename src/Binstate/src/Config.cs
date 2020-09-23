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

        return AddTransition(@event, () => stateId, true, null, false);
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
        
        return AddTransition(@event, getState, false, null, false);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the <paramref name="stateId"> specified state</paramref> when
      /// <paramref name="event"> event is raised</paramref>
      /// Use this overload if target state enter action requires an input argument.
      /// </summary>
      public Transitions AddTransition<TArgument>([NotNull] TEvent @event, [NotNull] TState stateId, bool argumentCanBeNull = false)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        if (stateId.IsNull()) throw new ArgumentNullException(nameof(stateId));
        
        return AddTransition(@event, () => stateId, true, typeof(TArgument), argumentCanBeNull);
      }

      /// <summary>
      /// Defines transition from the currently configured state to the state calculated dynamically depending on other application state. 
      /// </summary>
      /// <param name="event"></param>
      /// <param name="getState">If getState returns default(TState) no transition executed</param>
      /// <param name="argumentCanBeNull">True if argument passed to enter action can be null</param>
      public Transitions AddTransition<TArgument>([NotNull] TEvent @event, [NotNull] Func<TState> getState, bool argumentCanBeNull = false)
      {
        if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
        if (getState.IsNull()) throw new ArgumentNullException(nameof(getState));
        
        return AddTransition(@event, getState, false, typeof(TArgument), argumentCanBeNull);
      }

      /// <summary>
      /// Defines transition from the state to itself when <param name="event"> is raised. Exit and enter actions are called in case of such transition.</param>
      /// </summary>
      public void AllowReentrancy(TEvent @event) => AddTransition(@event, () => StateId, true, null, false);
      
      /// <summary>
      /// Defines transition from the state to itself when <paramref name="event"> is raised. Exit and enter actions are called in case of such transition.</paramref>
      /// Use this overload if the state enter action requires an input argument.
      /// </summary>
      /// <param name="event"></param>
      /// <param name="argumentCanBeNull">True if argument passed to enter action can be null</param>
      public void AllowReentrancy<TArgument>(TEvent @event, bool argumentCanBeNull = false) => AddTransition(@event, () => StateId, true, typeof(TArgument), argumentCanBeNull);
      
      private Transitions AddTransition(TEvent @event, Func<TState> getState, bool isStatic, Type parameterType, bool argumentCanBeNull)
      {
        TransitionList.Add(new Transition<TState, TEvent>(@event, getState, isStatic, parameterType, argumentCanBeNull));
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
      private IStateFactory _stateFactory = new NoArgumentStateFactory();
      
      internal const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      [CanBeNull] 
      internal IEnterInvoker<TEvent> EnterAction;
      
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

    /// <summary>
    /// This class is used to configure composite states. 
    /// </summary>
    public class State : Enter
    {
      internal State(TState stateId) : base(stateId)
      { }
      
      internal TState ParentStateId;

      /// <summary>
      /// Defines the currently configured state as a subset of a composite state 
      /// </summary>
      public Enter AsSubstateOf(TState parentStateId)
      {
        ParentStateId = parentStateId;
        return this;
      }
    }

    private interface IStateFactory
    {
      State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState);
    }

    private class NoArgumentStateFactory : IStateFactory
    {
      public State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState)
      {
        var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");

          transitions.Add(transition.Event, transition);
        }

        return new State<TState, TEvent>(stateConfig.StateId, stateConfig.EnterAction, stateConfig.ExitAction, transitions, parentState);
      }
    }

    private class StateFactory<TArgument> : IStateFactory
    {
      public State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState)
      {
        var transitions = new Dictionary<TEvent, Transition<TState, TEvent>>();
        foreach (var transition in stateConfig.TransitionList)
        {
          if (transitions.ContainsKey(transition.Event))
            throw new InvalidOperationException($"Duplicated event '{transition.Event}' in state '{stateConfig.StateId}'");

          transitions.Add(transition.Event, transition);
        }

        return new State<TState, TEvent, TArgument>(stateConfig.StateId, stateConfig.EnterAction, stateConfig.ExitAction, transitions, parentState);
      }
    }
  }
}