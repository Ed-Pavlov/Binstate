using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Binstate
{
  public class StateMachine
  {
    private readonly Dictionary<object, State> _states;
    
    private volatile object? _currentStateInternal;
    private State _currentState;

    internal StateMachine(State initialState, Dictionary<object, State> states)
    {
      _currentState = initialState;
      _states = states;
    }

#nullable disable
    public void Fire(object trigger) => FireInternal<Unit>(trigger, _ => _.ValidateParameter(), null);
    public void Fire<T>(object trigger, [MaybeNull]T parameter) => FireInternal(trigger, _ => _.ValidateParameter(parameter), parameter);
    public Task FireAsync(object trigger) => FireInternal<Unit>(trigger, _ => _.ValidateParameter(), null);
    public Task FireAsync<T>(object trigger, [MaybeNull]T parameter) => FireInternal(trigger, _ => _.ValidateParameter(parameter), parameter);

    [return:MaybeNull]
    private Task FireInternal<T>(object trigger, Action<Transition> transitionValidator, [MaybeNull]T parameter)
    {
      var transition = _currentState.FindTransition(trigger);
      transitionValidator(transition);
      
      _currentStateInternal = null; // signal current state routine to stop
      _currentState.Exit(); // wait current onEntry finished, then call OnExit

      var newState = GetState(transition.State);
      _currentState = newState;
      _currentStateInternal = _currentState.Id;
      var task = newState.Enter(new Controller(newState.Id, this), parameter);
      
      return task;
    }
#nullable restore
    
    private State GetState(object state) => _states[state];
    
    private bool IsInStateInternal(object state) => Equals(state, _currentStateInternal);
    
    private class Controller : IStateMachine
    {
      private readonly object _state;
      private readonly StateMachine _stateMachine;

      public Controller(object state, StateMachine stateMachine)
      {
        _state = state;
        _stateMachine = stateMachine;
      }

      public void Fire(object trigger) => _stateMachine.Fire(trigger);
      public void Fire<T>(object trigger, T parameter) => _stateMachine.Fire(trigger, parameter);
      public Task FireAsync(object trigger) => _stateMachine.FireAsync(trigger);
      public Task FireAsync<T>(object trigger, T parameter) => _stateMachine.FireAsync(trigger, parameter);

      public bool InMyState => _stateMachine.IsInStateInternal(_state);
    }
  }
}