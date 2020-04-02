using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  internal sealed class State
  {
    private readonly ManualResetEvent _enterFunctionFinished = new ManualResetEvent(true);
    [CanBeNull] private readonly EnterInvoker _enter;
    [CanBeNull] private readonly Action _exit;

    public readonly object Id;
    public readonly Dictionary<object, Transition> Transitions;
    
    [CanBeNull] private Task _task;

    public State(object id, EnterInvoker enter, Action exit, Dictionary<object, Transition> transitions)
    {
      Id = id;
      _enter = enter;
      _exit = exit;
      Transitions = transitions;
    }

    public Transition FindTransition(object trigger)
    {
      if (!Transitions.TryGetValue(trigger, out var transition))
        throw new InvalidOperationException($"Transition '{trigger}' is not allowed from the state '{Id}'");
      return transition;
    }
    
    public Task Enter<T>(IStateMachine stateMachine, T arg)
    {
      if (_enter == null) return null;
      
      _enterFunctionFinished.Reset();
      if (typeof(T) == typeof(Unit))
      {
        var noParameterEnter = (NoParameterEnterInvoker) _enter;
        _task = noParameterEnter.Invoke(stateMachine);
      }
      else
      {
        var typedEnter = (EnterInvoker<T>) _enter;
        _task = typedEnter.Invoke(stateMachine, arg);
      }
      _enterFunctionFinished.Set();
      return _task;
    }

    public void Exit()
    {
      WaitForStopRoutine();
      _exit?.Invoke();
    }

    private bool WaitForStopRoutine(int timeout = Timeout.Infinite)
    {
      // if entry method async it returns task and we wait it, if entry method is blocking we wait for event 
      return _task?.Wait(timeout) ?? _enterFunctionFinished.WaitOne(timeout);
    }
  }
}