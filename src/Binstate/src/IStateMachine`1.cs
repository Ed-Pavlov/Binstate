using System;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This interface is used in 'enter' actions to control execution and to execute auto transitions 
  /// </summary>
  public interface IStateMachine<in TEvent>
  {
    /// <summary>
    /// Returns true if the state machine is in the state for which currently executing enter action is defined.  
    /// </summary>
    bool InMyState { get; }
    
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
    bool RaiseAsync<T>([NotNull] TEvent @event, [NotNull] T argument);
  }
}