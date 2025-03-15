using System;

namespace BeatyBit.Binstate;

/// <summary>
/// This interface is used in 'enter' actions to control execution and to perform 'auto transitions'
/// </summary>
public interface IStateController<in TEvent>
{
  /// <summary>
  /// Gets a value indicating whether the state machine is currently in the state for which the executing 'enter' action is defined.
  /// </summary>
  bool InMyState { get; }

  /// <summary>
  /// Asynchronously raises an event to trigger a state transition.
  /// </summary>
  /// <param name="event">The event to raise.</param>
  /// <returns>Synchronously returns <c>true</c> if a transition will be performed; otherwise, <c>false</c> if no transition was found.</returns>
  /// <exception cref="TransitionException">
  /// Thrown if the 'enter' action of the target state requires an argument.
  /// User-defined exceptions from 'enter', 'exit', and 'dynamic transition' actions are caught and reported using the delegate
  /// passed to <see cref="Builder{TState,TEvent}(System.Action{Exception}, Builder{TState,TEvent}.Options)"/>.
  /// </exception>
  bool RaiseAsync(TEvent @event);

  /// <summary>
  /// Asynchronously raises an event with an argument to trigger a state transition.
  /// </summary>
  /// <typeparam name="T">The type of the argument.</typeparam>
  /// <param name="event">The event to raise.</param>
  /// <param name="argument">The argument to pass with the event.</param>
  /// <param name="argumentIsFallback">Indicates whether this argument is used only as a fallback; if a state-specific argument is available, it will be used instead.</param>
  /// <returns>Synchronously returns <c>true</c> if a transition will be performed; otherwise, <c>false</c> if no transition was found.</returns>
  /// <exception cref="TransitionException">
  /// Thrown if the 'enter' action of the target state requires an argument.
  /// User-defined exceptions from 'enter', 'exit', and 'dynamic transition' actions are caught and reported using the delegate passed to <see cref="Builder{TState,TEvent}(System.Action{Exception}, Builder{TState,TEvent}.Options)"/>.
  /// </exception>
  /// <remarks>
  /// The argument is required if the 'enter' action of the target state or 'exit' action of the current state is configured to accept it.
  /// See <see cref="Builder{TState,TEvent}.ConfiguratorOf.IEnterAction.OnEnter{T}(System.Action{IStateController{TEvent}, T})"/>.
  /// </remarks>
  bool RaiseAsync<T>(TEvent @event, T argument, bool argumentIsFallback = false);
}