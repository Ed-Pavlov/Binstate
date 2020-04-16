using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  internal sealed class State<TState, TEvent>
  {
    /// <summary>
    /// This event is used to avoid race condition when <see cref="Exit"/> method is called before <see cref="Enter{T}"/> method.
    /// See usages for details.
    /// </summary>
    private readonly ManualResetEvent _entered = new ManualResetEvent(true);
    
    /// <summary>
    /// This event is used to wait while state's OnEnter action is finished before call OnExit action and change the active state of the state machine.
    /// See usages for details. 
    /// </summary>
    private readonly ManualResetEvent _enterFunctionFinished = new ManualResetEvent(true);
    
    /// <summary>
    /// This task is used to wait while state's OnEnter action is finished before call OnExit action and change the active state of the state machine in
    /// case of async OnEnter action.
    /// See usages for details. 
    /// </summary>
    [CanBeNull] 
    private Task _task;
    
    [CanBeNull] 
    private readonly EnterActionInvoker<TEvent> _enter;
    [CanBeNull] 
    private readonly Action _exit;

    public readonly object Id;
    public readonly Dictionary<object, Transition<TState, TEvent>> Transitions;

    public State([NotNull] object id, [CanBeNull] EnterActionInvoker<TEvent> enter, [CanBeNull] Action exit, [NotNull] Dictionary<object, Transition<TState, TEvent>> transitions)
    {
      Id = id ?? throw new ArgumentNullException(nameof(id));
      _enter = enter;
      _exit = exit;
      Transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
    }

    public Transition<TState, TEvent> FindTransition(TEvent @event)
    {
      if (!Transitions.TryGetValue(@event, out var transition))
        throw new TransitionException($"No transition defined by raising the event '{@event}' in the state '{Id}'");
      return transition;
    }

    /// <summary>
    /// This method is called from protected by lock part of the code so it's no need synchronization
    /// see <see cref="StateMachine{TEvent, TState}.RaiseInternal{T}"/> implementation for details.
    /// </summary>
    public void SetAsActive() => _entered.Reset();

    public void Enter<TArg>(IStateMachine<TEvent> stateMachine, TArg arg)
    {
      try
      {
        _enterFunctionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
        _entered.Set();                 // it is safe to set the state as entered

        if (_enter == null) return;
        
        if (typeof(TArg) == typeof(Unit))
        {
          var noParameterEnter = (NoParameterEnterInvoker<TEvent>) _enter;
          _task = noParameterEnter.Invoke(stateMachine);
        }
        else
        {
          var typedEnter = (EnterInvoker<TEvent, TArg>) _enter;
          _task = typedEnter.Invoke(stateMachine, arg);
        }
      }
      finally
      {
        _enterFunctionFinished.Set();  
      }
    }

    /// <summary>
    /// <see cref="Exit"/> can be called earlier then <see cref="Enter{T}"/> of the activated state,
    /// see <see cref="StateMachine{TEvent, TState}.RaiseInternal{T}"/> implementation for details.
    /// In this case it should wait till <see cref="Enter{T}"/> will be called and exited, before call exit action
    /// </summary>
    public void Exit()
    {
      // if action is set as active but enter action still is not called, wait for it 
      _entered.WaitOne();
      
      // wait till State.Enter function finishes
      // if enter action is blocking or no action: _enterFunctionFinished is set means it finishes
      _enterFunctionFinished.WaitOne(Timeout.Infinite);
      // if async: _enterFunctionFinished is set means there is a value assigned to _task, which allows waiting till action finishes
      _task?.Wait(Timeout.Infinite);
      
      _exit?.Invoke();
    }
  }
}