using System;

namespace Binstate;

/// <summary>
///   Extracted from <see cref="IStateController{TEvent}" /> in order to separate interface allowing <see cref="IStateController{TEvent}.Relaying{TRelay}" /> and
///   providing common functionality by raising an event.
/// </summary>
public interface IAutoTransition<in TEvent>
{
  /// <summary>
  ///   Passing the event to the state machine asynchronously.
  /// </summary>
  /// <returns> Synchronously returns false if a transition was not found and true if the transition will be performed. </returns>
  /// <exception cref="TransitionException">
  ///   Throws if the 'enter' action of the target state requires argument.
  ///   All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
  ///   using the delegate passed into <see cref="Builder{TState,TEvent}(System.Action{Exception})" />
  /// </exception>
  bool RaiseAsync(TEvent @event);

  /// <summary>
  ///   Passing the event with an argument to the state machine asynchronously. The arguments is needed if the 'enter' action of the
  ///   target state requires one.
  ///   See <see cref="Config{TState, TEvent}.Enter.OnEnter{T}(System.Action{IStateController{TEvent}, T})" />,
  /// </summary>
  /// <returns> Synchronously returns false if a transition was not found and true if the transition will be performed. </returns>
  /// <exception cref="TransitionException">
  ///   Throws if the 'enter' action of the target state requires argument.
  ///   All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
  ///   using the delegate passed into <see cref="Builder{TState,TEvent}(System.Action{Exception})" />
  /// </exception>
  bool RaiseAsync<T>(TEvent @event, T argument);
}

/// <summary>
///   This interface is used in 'enter' actions to control execution and to execute 'auto transitions'
/// </summary>
public interface IStateController<in TEvent> : IAutoTransition<TEvent>
{
  /// <summary>
  ///   Returns true if the state machine is in the state for which currently executing enter action is defined.
  /// </summary>
  bool InMyState { get; }

  /// <summary />
  [Obsolete(
      "Since version 1.2 relaying arguments from the currently active states to states require them performs automatically."
    + "This method is not needed and adds nothing to the behaviour of the state machine."
    )]
  IAutoTransition<TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true);
}