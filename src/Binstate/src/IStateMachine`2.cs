﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public interface IStateMachine<TState, TEvent>
  {
    /// <summary>
    /// Raises the event in the blocking way. It waits while on entering and exiting actions (if defined) of the current state is finished, then:
    /// if the entering action of the target state is blocking, it will block till on entering method will finish.
    /// if the entering action of the target state is async, it will return after the state is changed.
    /// </summary>
    /// <returns>Returns true if state was changed, false if not</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state requires argument.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(System.Action{System.Exception}(System.Exception))"/>
    /// </exception>
    bool Raise([NotNull] TEvent @event);

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
    bool Raise<T>([NotNull] TEvent @event, [CanBeNull] T argument);

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
    Task<bool> RaiseAsync([NotNull] TEvent @event);

    /// Raises the event with an argument asynchronously. Finishing can be controller by returned <see cref="Task"/>, entering and exiting actions (if defined)
    /// of the current state is finished, then:
    /// if the entering action of the target state is blocking, Task finishes when entering action of the new state is finished;
    /// if the entering action of the target state is async, Task finishes right after the state is changed.
    /// <returns>Synchronously returns false if the transition was not found.</returns>
    /// <exception cref="TransitionException">Throws if the 'enter' action of the target state doesn't requires argument or requires argument of not compatible type.
    /// All users exception from the 'enter', 'exit' and 'dynamic transition' actions are caught and reported
    /// using the delegate passed into <see cref="Builder{TState,TEvent}(Action{Exception})"/>
    /// </exception>
    Task<bool> RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument);
  }
}