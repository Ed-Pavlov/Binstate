using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// The state machine. Use <see cref="Builder{TState, TEvent}"/> to configure and build a state machine.
  /// </summary>
  [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
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
      return PerformTransition<Unit>(@event, null);
    }

    /// <summary>
    /// Raises the event with an argument in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method of the new state will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    /// <returns>Returns true if state was changed, false if not</returns>
    public bool Raise<T>([NotNull] TEvent @event, [CanBeNull] T argument)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return PerformTransition(@event, argument);
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
      return Task.Run(() => PerformTransition<Unit>(@event, null));
    }

    /// Raises the event with an argument asynchronously. Finishing can be controller by returned <see cref="Task"/>, entering and exiting actions (if defined)
    /// of the current state is finished, then:
    /// if the entering action of the target state is blocking, Task finishes when entering action of the new state is finished;
    /// if the entering action of the target state is async, Task finishes right after the state is changed.
    public Task<bool> RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));
      return Task.Run(() => PerformTransition(@event, argument));
    }

    private bool PerformTransition<T>(TEvent @event, T argument)
    {
      var enterActions = new List<Action>();

      lock(_currentStateAccess)
      {
        var activeState = _activeStates.Peek(); // there should be at least one active state, don't need to check count
        
        var transition = activeState.FindTransitionTransitive(@event); // looks for a transition through all parent states
        var stateId = transition.GetTargetStateId(_onException);
        if(stateId.IsNull()) // dynamic transition can return null, means no transition needed
          return false;
        var newState = GetState(stateId);

        var commonAncestor = FindLeastCommonAncestor(newState, activeState);
        var statesToEnter = newState.GetAllStatesForActivationTillParent(commonAncestor); // get states from activeState with all parents till newState itself to activate 
        ValidateStates(statesToEnter, argument, activeState, @event); // validate before changing any state of the state machine
        
        // ---------------------------------------------------------------------------------------------------
        // till this point we can throw an exception (TransitionException) because no real changes were performed
        // ---------------------------------------------------------------------------------------------------
        
        // exit all active states which are not parent for the new state
        while (activeState != null && !newState.IsSubstateOf(activeState))
        {
          activeState.Exit(_onException);
          _activeStates.Pop(); // remove from active states
          activeState = _activeStates.Count == 0 ? null : _activeStates.Peek();
        }
       
        // activate new active states
        foreach (var state in statesToEnter)
        {
          var controller = new Controller(state, this);
          state.IsActive = true; // set is as active inside the lock, see implementation of State class for details

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

    [CanBeNull]
    private static State<TState, TEvent> FindLeastCommonAncestor(State<TState, TEvent> l, State<TState, TEvent> r)
    {
      var lDepth = l.DepthInTree;
      var rDepth = r.DepthInTree;
      while (lDepth != rDepth)
      {
        if (lDepth > rDepth)
        {
          lDepth--;
          l = l.ParentState;
        }
        else
        {
          rDepth--;
          r = r.ParentState;
        }
      }

      while (!ReferenceEquals(l, r))
      {
        l = l.ParentState;
        r = r.ParentState;
      }

      return l;
    }
    
    private static void ValidateStates<T>(IReadOnlyCollection<State<TState,TEvent>> states, T argument, State<TState,TEvent> activeState, TEvent @event)
    {
      var argumentType = typeof(T);
      var argumentSpecified = argumentType != typeof(Unit);
      
      var enterWithArgumentCount = 0;
      foreach (var state in states)
      {
        if (state.EnterArgumentType != null)
        {
          if(!argumentSpecified)
            throw new TransitionException($"The enter action of the state '{state.Id}' is configured as required an argument but no argument was specified.");
          
          if(!state.EnterArgumentType.IsAssignableFrom(argumentType))
            throw new TransitionException($"Cannot convert from '{argumentType}' to '{state.EnterArgumentType}' invoking the enter action of the state '{state.Id}'");

          enterWithArgumentCount++;
        }
      }

      if (argumentSpecified && enterWithArgumentCount == 0)
      {
        var statesToActivate = string.Join("->", states.Select(_ => _.Id.ToString()));
        
        throw new TransitionException(
          $"Transition from the state '{activeState.Id}' by the event '{@event}' will activate following states [{statesToActivate}]. No one of them are defined with "
          + $"the enter action accepting an argument, but argument '{argument}' was passed to the Raise call");
      }
    }
  }
}