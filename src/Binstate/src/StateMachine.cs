using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// The state machine. Use <see cref="Builder{TState, TEvent}"/> to configure and build a state machine.
  /// </summary>
  public partial class StateMachine<TState, TEvent>
  {
    /// <summary>
    /// The map of all defined states
    /// </summary>
    private readonly Dictionary<TState, State<TState, TEvent>> _states;

    private readonly Action<Exception> _onException;

    /// <summary>
    /// Active composite (hierarchy of) state
    /// </summary>
    private readonly Stack<State<TState, TEvent>> _activeStates = new Stack<State<TState, TEvent>>();
    private readonly object _currentStateAccess = new object();

    internal StateMachine(State<TState, TEvent> initialState, Dictionary<TState, State<TState, TEvent>> states, Action<Exception> onException)
    {
      _states = states;
      _onException = onException;
      _activeStates.Push(initialState);
    }

    /// <summary>
    /// Raises the event in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    /// <returns>Returns true if state was changed, false if not</returns>
    public bool Raise([NotNull] TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return ExecuteTransition<Unit>(@event, _ => _.ValidateParameter(), null);
    }

    /// <summary>
    /// Raises the event with parameter in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method of the new state will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    /// <returns>Returns true if state was changed, false if not</returns>
    public bool Raise<T>([NotNull] TEvent @event, [CanBeNull] T parameter)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return ExecuteTransition(@event, _ => _.ValidateParameter(parameter), parameter);
    }

    public bool Raise<T>([NotNull] TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return ExecuteTransition<T>(@event, _ => _.ValidateParameter<T>(), default, true);
    }

    /// <summary>
    /// Raises the event asynchronously. Finishing can be controller by returned <see cref="Task"/>, entering and exiting actions (if defined) of the current
    /// state is finished, then:
    /// if the entering action of the target state is blocking, Task finishes when entering action of the new state is finished;
    /// if the entering action of the target state is async, Task finishes right after the state is changed.
    /// </summary>
    public Task<bool> RaiseAsync([NotNull] TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return Task.Run(() => ExecuteTransition<Unit>(@event, _ => _.ValidateParameter(), null));
    }

    /// Raises the event with parameter asynchronously. Finishing can be controller by returned <see cref="Task"/>, entering and exiting actions (if defined)
    /// of the current state is finished, then:
    /// if the entering action of the target state is blocking, Task finishes when entering action of the new state is finished;
    /// if the entering action of the target state is async, Task finishes right after the state is changed.
    public Task<bool> RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T parameter)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return Task.Run(() => ExecuteTransition(@event, _ => _.ValidateParameter(parameter), parameter));
    }

    private bool ExecuteTransition<T>(TEvent @event, Action<Transition<TState, TEvent>> transitionValidator, T argument, bool propagate = false)
    {
      var enterActions = new List<Action>();

      lock(_currentStateAccess)
      {
        var activeState = _activeStates.Peek(); // there should be at least one active state, don't need to check count
        
        var transition = activeState.FindTransitionTransitive(@event); // looks for a transition through all parent states
        transitionValidator(transition);

        var stateId = transition.GetTargetStateId(_onException);
        if(stateId.IsNull()) // dynamic transition can return null, means no transition needed
          return false;
        
        var newState = GetState(stateId);

        var parameter = argument;
        if (propagate)
        {
          var stateWithArgument = (State<TState, TEvent, T>)activeState;
          parameter = stateWithArgument.Argument;
        }
        
        // exit all active states which are not parent for the new state
        while (activeState != null && !newState.IsSubstateOf(activeState))
        {
          activeState.Exit(_onException);
          _activeStates.Pop(); // remove from active states
          activeState = _activeStates.Count == 0 ? null : _activeStates.Peek();
        }

        // if new state has parent states enter all parent states from the top
        var states = newState.GetAllStatesForActivationFrom(activeState); // get all states to activate starting from activeState till newState itself
        foreach (var state in states)
        {
          var controller = new Controller(state, this);
          state.IsActive = true; // set is as active inside the lock, see implementation of State class for details

          if(state is IState<TState, TEvent, T> stateWithArgument)
            enterActions.Add(() => stateWithArgument.Enter(controller, parameter, _onException));
          else
            enterActions.Add(() => state.Enter(controller, argument, _onException));
          
          _activeStates.Push(state);
        }
      }

      // call Enter actions out of a lock due to it can block execution
      foreach (var enterAction in enterActions)
        enterAction();
      
      return true;
    }
    
    private State<TState, TEvent> GetState([NotNull] TState state)
    {
      if (!_states.TryGetValue(state, out var result))
        throw new InvalidOperationException($"State '{state}' is not defined");
      return result;
    }
  }
}