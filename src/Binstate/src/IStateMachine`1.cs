using System;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// Extracted from <see cref="IStateMachine{TEvent}"/> in order to separate interface allowing <see cref="IStateMachine{TEvent}.Relaying{TRelay}"/> and
  /// providing common functionality by raising an event. 
  /// </summary>
  public interface IAutoTransition<in TEvent>
  {
    /// <summary>
    /// Passing the event to the state machine asynchronously.
    /// </summary>
    /// <returns>Synchronously returns false if a transition was not found and true if the transition will be performed.</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state requires argument.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(System.Action{Exception}, bool)"/>
    /// </exception>
    bool RaiseAsync([NotNull] TEvent @event);
    
    /// <summary>
    /// Passing the event with an argument to the state machine asynchronously. The arguments is needed if the 'enter' action of the
    /// target state requires one.
    /// See <see cref="Config{TState, TEvent}.Enter.OnEnter{T}(System.Action{IStateMachine{TEvent}, T})"/>,
    /// </summary>
    /// <returns>Synchronously returns false if a transition was not found and true if the transition will be performed.</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state requires argument.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(System.Action{Exception}, bool)"/>
    /// </exception>
    bool RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument);
  }
  
  /// <summary>
  /// This interface is used in 'enter' actions to control execution and to execute 'auto transitions' 
  /// </summary>
  public interface IStateMachine<in TEvent> : IAutoTransition<TEvent>
  {
    /// <summary>
    /// Returns true if the state machine is in the state for which currently executing enter action is defined.  
    /// </summary>
    bool InMyState { get; }

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
    IAutoTransition<TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true);
  }
}