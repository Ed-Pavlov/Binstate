using System;
using System.Threading.Tasks;

namespace BeatyBit.Binstate;

/// <summary>
/// Defines the interface for a state machine, providing methods to raise events and manage state transitions.
/// </summary>
public interface IStateMachine<in TEvent>
{
  /// <summary>
  /// Raises an event synchronously, blocking until all related actions are completed.
  /// </summary>
  /// <remarks>
  /// Waits for the completion of 'exit' and 'enter' actions (if defined) of the current state.
  /// If the 'enter' action of the target state is synchronous, it blocks until the action completes.
  /// If the 'enter' action of the target state is asynchronous, it returns immediately after the state change.
  /// </remarks>
  /// <param name="event">The event to raise.</param>
  /// <returns><c>true</c> if the state was changed; otherwise, <c>false</c>.</returns>
  /// <exception cref="TransitionException">
  /// Thrown if the 'enter' action of the target state requires an argument.
  /// User-defined exceptions from 'enter', 'exit', and 'dynamic transition' actions are caught and reported using the
  /// delegate passed to <see cref="Builder{TState,TEvent}(System.Action{Exception}, Builder{TState,TEvent}.Options)"/>.
  /// </exception>
  bool Raise(TEvent @event);

  /// <summary>
  /// Raises an event with an argument synchronously, blocking until all related actions are completed.
  /// </summary>
  /// <typeparam name="T">The type of the argument.</typeparam>
  /// <remarks>
  /// Waits for the completion of 'exit' and 'enter' actions (if defined) of the current state.
  /// If the 'enter' action of the target state is synchronous, it blocks until the action completes.
  /// If the 'enter' action of the target state is asynchronous, it returns immediately after the state change.
  /// </remarks>
  /// <param name="event">The event to raise.</param>
  /// <param name="argument">The argument to pass with the event.</param>
  /// <param name="argumentIsFallback">Indicates whether this argument is used only as a fallback; if a state-specific argument is available, it will be used instead.</param>
  /// <returns><c>true</c> if the state was changed; otherwise, <c>false</c>.</returns>
  /// <exception cref="TransitionException">
  /// Thrown if the 'enter' action of the target state does not require an argument or requires an argument of an incompatible type.
  /// User-defined exceptions from 'enter', 'exit', and 'dynamic transition' actions are caught and reported using the delegate passed to <see cref="Builder{TState,TEvent}(System.Action{Exception}, Builder{TState,TEvent}.Options)"/>.
  /// </exception>
  bool Raise<T>(TEvent @event, T argument, bool argumentIsFallback = false);

  /// <summary>
  /// Raises an event asynchronously, allowing control of completion through the returned <see cref="Task"/>.
  /// </summary>
  /// <remarks>
  /// Waits for the completion of 'exit' and 'enter' actions (if defined) of the current state.
  /// If the 'enter' action of the target state is synchronous, the <see cref="Task"/> completes when the action completes.
  /// If the 'enter' action of the target state is asynchronous, the <see cref="Task"/> completes immediately after the state change.
  /// </remarks>
  /// <param name="event">The event to raise.</param>
  /// <returns>A <see cref="Task{TResult}"/> that completes with <c>true</c> if a transition was found; otherwise, <c>false</c>.</returns>
  /// <exception cref="TransitionException">
  /// Thrown if the 'enter' action of the target state requires an argument.
  /// User-defined exceptions from 'enter', 'exit', and 'dynamic transition' actions are caught and reported using the delegate passed to <see cref="Builder{TState,TEvent}(System.Action{Exception}, Builder{TState,TEvent}.Options)"/>.
  /// </exception>
  Task<bool> RaiseAsync(TEvent @event);

  /// <summary>
  /// Raises an event with an argument asynchronously, allowing control of completion through the returned <see cref="Task"/>.
  /// </summary>
  /// <typeparam name="T">The type of the argument.</typeparam>
  /// <remarks>
  /// Waits for the completion of 'exit' and 'enter' actions (if defined) of the current state.
  /// If the 'enter' action of the target state is synchronous, the <see cref="Task"/> completes when the action completes.
  /// If the 'enter' action of the target state is asynchronous, the <see cref="Task"/> completes immediately after the state change.
  /// </remarks>
  /// <param name="event">The event to raise.</param>
  /// <param name="argument">The argument to pass with the event.</param>
  /// <param name="argumentIsFallback">Indicates whether this argument is used only as a fallback; if a state-specific argument is available, it will be used instead.</param>
  /// <returns>A <see cref="Task{TResult}"/> that completes with <c>true</c> if a transition was found; otherwise, <c>false</c>.</returns>
  /// <exception cref="TransitionException">
  /// Thrown if the 'enter' action of the target state does not require an argument or requires an argument of an incompatible type.
  /// User-defined exceptions from 'enter', 'exit', and 'dynamic transition' actions are caught and reported using the delegate passed to <see cref="Builder{TState,TEvent}(System.Action{Exception}, Builder{TState,TEvent}.Options)"/>.
  /// </exception>
  Task<bool> RaiseAsync<T>(TEvent @event, T argument, bool argumentIsFallback = false);

  /// <summary>
  /// Serializes the StateMachine in its current state to string representation.
  /// It can be restored using <see cref="Builder{TState,TEvent}.Restore"/> method.
  /// </summary>
  /// <param name="customSerializer">If not primitive or string type is used as TState in <see cref="Builder{TState,TEvent}"/> or
  /// an argument passed to <see cref="IStateMachine{TEvent}.Raise"/> custom serializer should be provided.</param>
  string Serialize(ICustomSerializer? customSerializer = null);
}