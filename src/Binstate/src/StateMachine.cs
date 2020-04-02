using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine
  {
    private volatile bool _started;
    
    private readonly Dictionary<object, State> _states;
    
    private object _currentControllerState;
    private State _currentState;

    private readonly object _access = new object();
    
    internal StateMachine(Dictionary<object, State> states) => _states = states;

    public void Start([NotNull] object initialState)
    {
      if (initialState == null) throw new ArgumentNullException(nameof(initialState));
      Start<Unit>(initialState, null);
    }

    public void Start<T>([NotNull] object initialState, [CanBeNull] T parameter)
    {
      if (initialState == null) throw new ArgumentNullException(nameof(initialState));
      if(_started) throw  new InvalidOperationException("Is already started");
      
      _started = true;
      lock (_access)
      {
        _currentState = GetState(initialState);
        EnterCurrentState(parameter);
      }
    }
    
    public void Stop()
    {
      if(!_started) throw new InvalidOperationException("StateMachine is not started");
      _started = false;
      
      lock (_access) 
        ExitCurrentState();
    }

    /// <summary>
    /// Fires the trigger in the blocking way.
    /// If OnEnter method of the state is blocking, <see cref="Fire"/> will block till on entering method will finish.
    /// If OnEnter method of the state is async, <see cref="Fire"/> will return just after OnExit of previous state is finished and state is changed.
    /// </summary>
    public void Fire([NotNull] object trigger)
    {
      if (trigger == null) throw new ArgumentNullException(nameof(trigger));
      FireInternal<Unit>(trigger, _ => _.ValidateParameter(), null);
    }

    public void Fire<T>([NotNull] object trigger, [CanBeNull] T parameter)
    {
      if (trigger == null) throw new ArgumentNullException(nameof(trigger));
      FireInternal(trigger, _ => _.ValidateParameter(parameter), parameter);
    }

    /// <summary>
    /// Fires the trigger in the asynchronous way. Execution can be controller by returned <see cref="Task"/>.
    /// If OnEnter method of the state is blocking, Task will be completed when on entering method will finish.
    /// If OnEnter method of the state is async, Task will be completed just after OnExit of previous state is finished and state is changed. 
    /// </summary>
    public Task FireAsync([NotNull] object trigger)
    {
      if (trigger == null) throw new ArgumentNullException(nameof(trigger));
      return Task.Run(() => FireInternal<Unit>(trigger, _ => _.ValidateParameter(), null));
    }

    public Task FireAsync<T>([NotNull] object trigger, [CanBeNull] T parameter)
    {
      if (trigger == null) throw new ArgumentNullException(nameof(trigger));
      return Task.Run(() => FireInternal(trigger, _ => _.ValidateParameter(parameter), parameter));
    }

    private void FireInternal<T>(object trigger, Action<Transition> transitionValidator, T parameter)
    {
      lock(_access)
      {
        var transition = _currentState.FindTransition(trigger);
        transitionValidator(transition);

        ExitCurrentState();
        
        var previousState = _currentState;
        _currentState = GetState(transition.State);
        
        try {
          EnterCurrentState(parameter);
        }
        catch {
          _currentState = previousState;
          EnterCurrentState(parameter);
          throw;
        }
      }
    }

    private void EnterCurrentState<T>(T parameter)
    {
      _currentControllerState = _currentState.Id;
      _currentState.Enter(new Controller(_currentState.Id, this), parameter);
    }
    
    private void ExitCurrentState()
    {
      _currentControllerState = null; // signal current state routine to stop
      _currentState.Exit(); // wait current onEntry finished, then call OnExit
    }

    private State GetState(object state)
    {
      if (!_states.TryGetValue(state, out var result))
        throw new InvalidOperationException($"State '{state}' is not registered in the state machine");
      return result;
    }
  }
}