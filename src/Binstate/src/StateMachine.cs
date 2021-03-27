using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Binstate
{
  /// <summary>
  /// The state machine. Use <see cref="Builder{TState, TEvent}"/> to configure and build a state machine.
  /// </summary>
  [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
  public partial class StateMachine<TState, TEvent> : IStateMachine<TState, TEvent>
  {
    private readonly Action<Exception> _onException;

    /// <summary>
    /// The map of all defined states
    /// </summary>
    private readonly Dictionary<TState, State<TState, TEvent>> _states;

    private readonly AutoResetEvent _lock = new AutoResetEvent(true);
    private volatile State<TState, TEvent>? _activeState;

    internal StateMachine(Dictionary<TState, State<TState, TEvent>> states, Action<Exception> onException)
    {
      _states = states;
      _onException = onException;
    }
    
    internal void SetInitialState<T>(TState initialStateId, T? initialStateArgument)
    {
      _activeState = GetStateById(initialStateId);
      var enterAction = ActivateStateNotGuarded(_activeState, new MixOf<T, Unit>(initialStateArgument.ToMaybe(), Maybe<Unit>.Nothing));
      try {
        enterAction();
      } catch (Exception exception) {
        _onException(exception);
      }
    }

    /// <inheritdoc />
    public bool Raise(TEvent @event)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionSync<Unit, Unit>(@event, null, Maybe<Unit>.Nothing);
    }

    /// <inheritdoc />
    public bool Raise<T>(TEvent @event, T? argument)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionSync(@event, argument, Maybe<Unit>.Nothing);
    }

    /// <inheritdoc />
    public Task<bool> RaiseAsync(TEvent @event)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionAsync<Unit, Unit>(@event, default, Maybe<Unit>.Nothing);
    }

    /// <inheritdoc />
    public Task<bool> RaiseAsync<T>(TEvent @event, T? argument)
    {
      if (@event == null) throw new ArgumentNullException(nameof(@event));

      return PerformTransitionAsync(@event, argument, Maybe<Unit>.Nothing);
    }

    /// <summary>
    /// Tell the state machine that it should get an argument attached to the currently active state (or any of parents) and pass it to the newly activated state
    /// </summary>
    /// <typeparam name="TRelay">The type of the argument. Should be exactly the same as the generic type passed into 
    /// <see cref="Config{TState,TEvent}.Enter.OnEnter{T}(Action{T})"/> or one of it's overload when configured currently active state (of one of it's parent).
    /// </typeparam>
    /// <param name="relayArgumentIsRequired">If there is no active state with argument for relaying:
    /// true: Raise method throws an exception
    /// false: state machine will pass default(TRelay) as an argument 
    /// </param>
    public IStateMachine<TState, TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true) => 
      new Relayer<TRelay>(this, relayArgumentIsRequired ? Maybe<TRelay>.Nothing : default(TRelay).ToMaybe());
    
    private bool PerformTransitionSync<TA, TRelay>(TEvent @event, TA? argument, Maybe<TRelay> backupRelayArgument)
    {
      var data = PrepareTransition(@event, argument, backupRelayArgument);
      return data != null && PerformTransition(data.Value);
    }

    private Task<bool> PerformTransitionAsync<TA, TRelay>(TEvent @event, TA? argument, Maybe<TRelay> backupRelayArgument)
    {
      var data = PrepareTransition(@event, argument, backupRelayArgument);

      return data == null
        ? Task.FromResult(false)
        : Task.Run(() => PerformTransition(data.Value));
    }

    private State<TState, TEvent> GetStateById(TState state) =>
      _states.TryGetValue(state, out var result) ? result : throw new TransitionException($"State '{state}' is not defined");

    private static State<TState, TEvent>? FindLeastCommonAncestor(State<TState, TEvent> left, State<TState, TEvent> right)
    {
      if (ReferenceEquals(left, right)) return null; // no common ancestor with itself

      var l = left;
      var r = right;
      
      var lDepth = l.DepthInTree;
      var rDepth = r.DepthInTree;

      // State<TState, TEvent>? left = l;
      
      while (lDepth != rDepth)
      {
        if (lDepth > rDepth)
        {
          lDepth--;
          l = l!.ParentState;
        }
        else
        {
          rDepth--;
          r = r!.ParentState;
        }
      }

      while (!ReferenceEquals(l, r))
      {
        l = l!.ParentState;
        r = r!.ParentState;
      }

      return l;
    }

    /// <summary>
    /// Validates that all 'enter' actions match (not)passed argument. Throws the exception if not, because it is not runtime problem, but the problem
    /// of configuration.
    /// </summary>
    private static void ValidateStates<TA, TRelay>(
      State<TState, TEvent> activeState,
      TEvent @event,
      State<TState, TEvent> targetState,
      MixOf<TA, TRelay> argument,
      State<TState, TEvent>? commonAncestor)
    {
      var enterWithArgumentCount = 0;

      var state = targetState; 
      while(state != commonAncestor)
      {
        if (state!.EnterArgumentType != null)
        {
          if (!argument.HasAnyArgument)
            throw new TransitionException($"The enter action of the state '{state.Id}' is configured as required an argument but no argument was specified.");

          if (!argument.IsMatch(state.EnterArgumentType))
            throw new TransitionException(
              $"The state '{state.Id}' requires argument of type '{state.EnterArgumentType}' but no argument of compatible type has passed nor relayed");

          enterWithArgumentCount++;
        }

        state = state.ParentState;
      }

      if (argument.HasAnyArgument && enterWithArgumentCount == 0)
      {
        // we can allocate here, because it's an exceptional case
        var states = new List<State<TState, TEvent>>();
        state = targetState.ParentState;
        while (state != commonAncestor)
        {
          states.Add(state!);
          state = state!.ParentState;
        }
        states.Reverse();
        states.Add(targetState);

        var statesToActivate = string.Join("->", states.Select(_ => _.Id!.ToString()));

        throw new TransitionException(
          $"Transition from the state '{activeState.Id}' by the event '{@event}' will activate following states [{statesToActivate}]. No one of them are defined with "
          + "the enter action accepting an argument, but argument was passed or relayed");
      }
    }
  }
}