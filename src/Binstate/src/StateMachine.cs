using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// The state machine
  /// </summary>
  public partial class StateMachine
  {
    private readonly Dictionary<object, State> _states;
    
    private object _currentControllerState;
    private State _currentState;

    private readonly object _access = new object();
    
    internal StateMachine(State initialState, Dictionary<object, State> states)
    {
      _states = states;
      _currentControllerState = initialState.Id;
      _currentState = initialState;
    }

    /// <summary>
    /// Raises the event in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    public void Raise([NotNull] object @event)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));
      RaiseInternal<Unit>(@event, _ => _.ValidateParameter(), null);
    }

    /// <summary>
    /// Raises the event with parameter in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    public void Raise<T>([NotNull] object @event, [CanBeNull] T parameter)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));
      RaiseInternal(@event, _ => _.ValidateParameter(parameter), parameter);
    }

    /// <summary>
    /// Raises the event asynchronously. Finishing can be controller by returned <see cref="Task"/>.
    /// if the entering action of the target state is blocking, Task is finished when the entering action is finished.
    /// if the entering action of the target state is async, Task is finishes after the state is changed. 
    /// </summary>
    public Task RaiseAsync([NotNull] object @event)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));
      return Task.Run(() => RaiseInternal<Unit>(@event, _ => _.ValidateParameter(), null));
    }

    /// <summary>
    /// Raises the event with parameter asynchronously. Finishing can be controller by returned <see cref="Task"/>.
    /// if the entering action of the target state is blocking, Task is finished when the entering action is finished.
    /// if the entering action of the target state is async, Task is finishes after the state is changed. 
    /// </summary>
    public Task RaiseAsync<T>([NotNull] object @event, [CanBeNull] T parameter)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));
      return Task.Run(() => RaiseInternal(@event, _ => _.ValidateParameter(parameter), parameter));
    }

    private void RaiseInternal<T>(object @event, Action<Transition> transitionValidator, T parameter)
    {
      lock(_access)
      {
        var transition = _currentState.FindTransition(@event);
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