using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  // internal class State<TState, TEvent, TArgument> : State<TState, TEvent>
  // {
  //   public State(
  //     TState id, 
  //     EnterActionInvoker<TEvent> enter, 
  //     Action exit, 
  //     Dictionary<TEvent, Transition<TState, TEvent>> transitions) : base(id, enter, exit, transitions)
  //   {
  //   }
  //   
  //   public void Enter(IStateMachine<TEvent> stateMachine, TArgument arg, Action<Exception> onException)
  //   {
  //     try
  //     {
  //       _enterFunctionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
  //       _entered.Set(); // it is safe to set the state as entered
  //
  //       if (_enter == null) return;
  //
  //       if (typeof(TArgument) == typeof(Unit))
  //       {
  //         var noParameterEnter = (NoParameterEnterInvoker<TEvent>) _enter;
  //         _task = noParameterEnter.Invoke(stateMachine);
  //       }
  //       else
  //       {
  //         var typedEnter = (EnterInvoker<TEvent, TArgument>) _enter;
  //         _task = typedEnter.Invoke(stateMachine, arg);
  //       }
  //     }
  //     catch (Exception exception)
  //     {
  //       onException(exception);
  //     }
  //     finally
  //     {
  //       _enterFunctionFinished.Set();
  //     }
  //   }
  // }

  internal class State<TState, TEvent>
  {
    /// <summary>
    /// This event is used to avoid race condition when <see cref="Exit"/> method is called before <see cref="Config{TState,TEvent}.Enter"/> method.
    /// See usages for details.
    /// </summary>
    protected readonly ManualResetEvent _entered = new ManualResetEvent(true);

    /// <summary>
    /// This event is used to wait while state's OnEnter action is finished before call OnExit action and change the active state of the state machine.
    /// See usages for details. 
    /// </summary>
    protected readonly ManualResetEvent _enterFunctionFinished = new ManualResetEvent(true);

    /// <summary>
    /// This task is used to wait while state's OnEnter action is finished before call OnExit action and change the active state of the state machine in
    /// case of async OnEnter action.
    /// See usages for details. 
    /// </summary>
    [CanBeNull] 
    protected Task _task;

    [CanBeNull] 
    protected readonly IEnterInvoker<TEvent> _enter;
    
    [CanBeNull]
    public Type EnterArgumentType;
    
    [CanBeNull] 
    private readonly Action _exit;

    public readonly TState Id;
    private /*readonly*/ State<TState, TEvent> _parentState; // building can be optimized (using topology sorting) and parent state could be passed into ctor
    public readonly Dictionary<TEvent, Transition<TState, TEvent>> Transitions;

    private volatile bool _isActive;

    public State(TState id, IEnterInvoker<TEvent> enter, Type enterArgumentType, Action exit, Dictionary<TEvent, Transition<TState, TEvent>> transitions)
    {
      Id = id;
      _enter = enter;
      EnterArgumentType = enterArgumentType;
      _exit = exit;
      Transitions = transitions;
    }

    public void Enter<TArgument>(IStateMachine<TEvent> stateMachine, TArgument arg, Action<Exception> onException)
    {
      try
      {
        _enterFunctionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
        _entered.Set(); // it is safe to set the state as entered

        if (_enter == null) return;

        if (typeof(TArgument) == typeof(Unit))
        {
          var noParameterEnter = (NoParameterEnterInvoker<TEvent>) _enter;
          _task = noParameterEnter.Invoke(stateMachine);
        }
        else
        {
          var parameterizedEnter = (IEnterInvoker<TEvent, TArgument>) _enter; //use interface but not class for contravariant TArgument parameter
          _task = parameterizedEnter.Invoke(stateMachine, arg);
        }
      }
      catch (Exception exception)
      {
        onException(exception);
      }
      finally
      {
        _enterFunctionFinished.Set();
      }
    }
    
    public Transition<TState, TEvent> FindTransitionTransitive(TEvent @event)
    {
      var state = this;
      while (state != null)
      {
        if (state.Transitions.TryGetValue(@event, out var transition))
          return transition;
        state = state._parentState;
      }

      // no transition found through all parents
      throw new TransitionException($"No transition defined by raising the event '{@event}' in the state '{Id}'");
    }

    /// <summary>
    /// This property is set from protected by lock part of the code so it's no need synchronization
    /// see <see cref="StateMachine{TState,TEvent}.ExecuteTransition{T}"/> implementation for details.
    /// </summary>
    public bool IsActive
    {
      get => _isActive;
      set
      {
        if (value) _entered.Reset();
        _isActive = value;
      }
    }

    

    /// <summary>
    /// <see cref="Exit"/> can be called earlier then <see cref="Config{TState,TEvent}.Enter"/> of the activated state,
    /// see <see cref="StateMachine{TState,TEvent}.ExecuteTransition{T}"/> implementation for details.
    /// In this case it should wait till <see cref="Config{TState,TEvent}.Enter"/> will be called and exited, before call exit action
    /// </summary>
    /// <param name="onException"></param>
    public void Exit(Action<Exception> onException)
    {
      try
      {
        IsActive = false; // signal that current state is no more active

        // if action is set as active but enter action still is not called, wait for it 
        _entered.WaitOne();

        // wait till State.Enter function finishes
        // if enter action is blocking or no action: _enterFunctionFinished is set means it finishes
        _enterFunctionFinished.WaitOne(Timeout.Infinite);
        // if async: _enterFunctionFinished is set means there is a value assigned to _task, which allows waiting till action finishes
        _task?.Wait(Timeout.Infinite);

        _exit?.Invoke();
      }
      catch (Exception exception)
      {
        onException(exception);
      }
    }

    public void AddParent([NotNull] State<TState, TEvent> parentState) => _parentState = parentState ?? throw new ArgumentNullException(nameof(parentState));

    public bool IsSubstateOf(State<TState, TEvent> state) => _parentState != null && (ReferenceEquals(state, _parentState) || _parentState.IsSubstateOf(state));

    public IReadOnlyCollection<State<TState, TEvent>> GetAllStatesForActivationFrom(State<TState, TEvent> tillState)
    {
      var states = new List<State<TState, TEvent>>();
      var parent = _parentState;
      while (parent != null && !ReferenceEquals(parent, tillState))
      {
        states.Add(parent);
        parent = parent._parentState;
      }

      states.Reverse();
      states.Add(this);
      return states;
    }
  }
}