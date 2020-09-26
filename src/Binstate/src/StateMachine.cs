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
    private readonly Action<Exception> _onException;
    
    /// <summary>
    /// The map of all defined states
    /// </summary>
    private readonly Dictionary<TState, State<TState, TEvent>> _states;

    private State<TState, TEvent> _activeState;

    internal StateMachine(State<TState, TEvent> initialState, Dictionary<TState, State<TState, TEvent>> states, Action<Exception> onException)
    {
      _states = states;
      _onException = onException;

      _activeState = initialState;
      ActivateInitialState(initialState, onException);
    }

    /// <summary>
    /// Raises the event in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    /// <returns>Returns true if state was changed, false if not</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state requires argument.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(Action{Exception})"/>
    /// </exception>
    public bool Raise([NotNull] TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionSync<Unit>(@event, null);
    }

    /// <summary>
    /// Raises the event with an argument in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method of the new state will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    /// <returns>Returns true if state was changed, false if not</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state doesn't requires argument or requires argument of not compatible type.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(Action{Exception})"/>
    /// </exception>
    public bool Raise<T>([NotNull] TEvent @event, [CanBeNull] T argument)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionSync(@event, argument);
    }

    /// <summary>
    /// Raises the event asynchronously. Finishing can be controller by returned <see cref="Task"/>, entering and exiting actions (if defined) of the current
    /// state is finished, then:
    /// if the entering action of the target state is blocking, Task finishes when entering action of the new state is finished;
    /// if the entering action of the target state is async, Task finishes right after the state is changed.
    /// </summary>
    /// <returns>Synchronously returns false if the transition was not found.</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state requires argument.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(Action{Exception})"/>
    /// </exception>
    public Task<bool> RaiseAsync([NotNull] TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionAsync<Unit>(@event, default);
    }

    /// Raises the event with an argument asynchronously. Finishing can be controller by returned <see cref="Task"/>, entering and exiting actions (if defined)
    /// of the current state is finished, then:
    /// if the entering action of the target state is blocking, Task finishes when entering action of the new state is finished;
    /// if the entering action of the target state is async, Task finishes right after the state is changed.
    /// <returns>Synchronously returns false if the transition was not found.</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state doesn't requires argument or requires argument of not compatible type.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(Action{Exception})"/>
    /// </exception>
    public Task<bool> RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionAsync(@event, argument);
    }

    public bool RaisePropagate<T>(TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionSync<T>(@event, default, true);
    }
    
    public Task<bool> RaisePropagateAsync<T>(TEvent @event)
    {
      if (@event.IsNull()) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionAsync<T>(@event, default, true);
    }
    
    private bool PerformTransitionSync<T>(TEvent @event, T argument, bool propagateStateArgument = false)
    {
      var data = PrepareTransition(@event, argument, propagateStateArgument);
      return data != null && PerformTransition(data.Value);
    }

    private Task<bool> PerformTransitionAsync<T>(TEvent @event, T argument, bool propagateStateArgument = false)
    {
      var data = PrepareTransition(@event, argument, propagateStateArgument);

      return data == null
        ? Task.FromResult(false)
        : Task.Run(() => PerformTransition(data.Value));
    }

    private void ActivateInitialState(State<TState, TEvent> initialState, Action<Exception> onException)
    {
      if(initialState.EnterArgumentType != null)
        throw new TransitionException("The enter action of the initial state must not require argument.");
      
      var enterAction = ActivateStateNotGuarded<Unit>(initialState, default);
      try {
        enterAction();
      }
      catch (Exception exception) {
        onException(exception);
      }
    }
    
    private State<TState, TEvent> GetStateById([NotNull] TState state) => 
      _states.TryGetValue(state, out var result) ? result : throw new TransitionException($"State '{state}' is not defined");

    [CanBeNull]
    private static State<TState, TEvent> FindLeastCommonAncestor(State<TState, TEvent> l, State<TState, TEvent> r)
    {
      if (ReferenceEquals(l, r)) return null; // no common ancestor with yourself

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

    /// <summary>
    /// Validates that all 'enter' actions match (not)passed argument. Throws the exception if not, because it is not runtime problem, but the problem
    /// of configuration.
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private static void ValidateStates<T>(
      IEnumerable<State<TState, TEvent>> states,
      State<TState, TEvent> activeState,
      TEvent @event,
      T argument,
      bool propagateStateArgument)
    {
      var argumentType = typeof(T);
      var argumentSpecified = argumentType != typeof(Unit);

      var enterWithArgumentCount = 0;

      foreach (var state in states)
      {
        if (state.EnterArgumentType != null)
        {
          if (!argumentSpecified)
            throw new TransitionException($"The enter action of the state '{state.Id}' is configured as required an argument but no argument was specified.");

          if (!state.EnterArgumentType.IsAssignableFrom(argumentType))
            throw new TransitionException($"Cannot convert from '{argumentType}' to '{state.EnterArgumentType}' invoking the enter action of the state '{state.Id}'");

          enterWithArgumentCount++;
        }
      }

      if (argumentSpecified && enterWithArgumentCount == 0)
      {
        var statesToActivate = string.Join("->", states.Select(_ => _.Id.ToString()));

        var argumentMessage = propagateStateArgument ? "was propagated" : $"'{argument}' was passed to the Raise call";
        
        throw new TransitionException(
          $"Transition from the state '{activeState.Id}' by the event '{@event}' will activate following states [{statesToActivate}]. No one of them are defined with "
          + $"the enter action accepting an argument, but argument {argumentMessage}");
      }
    }
  }
}