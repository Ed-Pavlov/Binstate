using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Binstate
{
  internal sealed class State
  {
    private readonly EnterInvoker? _enter;
    private readonly Action? _exit;
    private readonly ManualResetEvent _event = new ManualResetEvent(true);
    private Task? _task;
    
    public readonly object Id;
    private readonly Dictionary<object, Transition> _transitions;

    public State(object id, EnterInvoker? enter, Action? exit, Dictionary<object, Transition> transitions)
    {
      Id = id;
      _enter = enter;
      _exit = exit;
      _transitions = transitions;
    }

    public Transition FindTransition(object trigger)
    {
      if (!_transitions.TryGetValue(trigger, out var transition))
        throw new InvalidOperationException($"Transition '{trigger}' is not allowed from the state '{Id}'");
      return transition;
    }
    
    public Task? Enter(IStateMachine stateMachine, object? arg)
    {
      _event.Reset();
      _task = _enter!.Invoke(stateMachine, arg);
      _event.Set();
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
      return _task?.Wait(timeout) ?? _event.WaitOne(timeout);
    }
  }
}