using System;
using System.Threading.Tasks;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    /// <inheritdoc cref="IEnterAction"/>
    public interface IEnterAction<TArgument> : IExitAction<TArgument>
    {
      /// <summary>
      /// Specifies a synchronous (blocking) action to be executed upon entering the state.
      /// </summary>
      /// <param name="enterAction">The synchronous action to execute.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <exception cref="ArgumentException">Thrown when <paramref name="enterAction"/> is an 'async void' method.</exception>
      IExitAction<TArgument> OnEnter(Action enterAction);

      /// <summary>
      /// Specifies a synchronous (blocking) action with access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The synchronous action with access to the state controller.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <exception cref="ArgumentException">Thrown when <paramref name="enterAction"/> is an 'async void' method.</exception>
      IExitAction<TArgument> OnEnter(Action<IStateController<TEvent>> enterAction);

      /// <summary>
      /// Specifies an asynchronous (non-blocking) action to be executed upon entering the state.
      /// </summary>
      /// <param name="enterAction">The asynchronous action returning a <see cref="Task"/>.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <remarks>Do not use 'async void' methods; they must return a <see cref="Task"/>.</remarks>
      IExitAction<TArgument> OnEnter(Func<Task> enterAction);

      /// <summary>
      /// Specifies an asynchronous (non-blocking) action with access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The asynchronous action with access to the state controller, returning a <see cref="Task"/>.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <remarks>Do not use 'async void' methods; they must return a <see cref="Task"/>.</remarks>
      IExitAction<TArgument> OnEnter(Func<IStateController<TEvent>, Task> enterAction);

      /// <summary>
      /// Specifies a synchronous (blocking) action with an argument and access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The synchronous action with an argument and access to the state controller.</param>
      /// <returns>An <see cref="IExitAction{TArgument}"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <exception cref="ArgumentException">Thrown when <paramref name="enterAction"/> is an 'async void' method.</exception>
      /// <remarks>Refer to <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> for argument usage details.</remarks>
      IExitAction<TArgument> OnEnter(Action<TArgument> enterAction);

      /// <summary>
      /// Specifies a synchronous (blocking) action with an argument and access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The synchronous action with an argument and access to the state controller.</param>
      /// <returns>An <see cref="IExitAction{TArgument}"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <exception cref="ArgumentException">Thrown when <paramref name="enterAction"/> is an 'async void' method.</exception>
      /// <remarks>Refer to <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> for argument usage details.</remarks>
      IExitAction<TArgument> OnEnter(Action<IStateController<TEvent>, TArgument> enterAction);

      /// <summary>
      /// Specifies an asynchronous (non-blocking) action with an argument, to be executed upon entering the state.
      /// </summary>
      /// <param name="enterAction">The asynchronous action with an argument, returning a <see cref="Task"/>.</param>
      /// <returns>An <see cref="IExitAction{TArgument}"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <remarks>Do not use 'async void' methods; they must return a <see cref="Task"/>. Refer to <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> for argument usage details.</remarks>
      IExitAction<TArgument> OnEnter(Func<TArgument, Task> enterAction);

      /// <summary>
      /// Specifies an asynchronous (non-blocking) action with an argument and access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The asynchronous action with an argument and access to the state controller, returning a <see cref="Task"/>.</param>
      /// <returns>An <see cref="IExitAction{TArgument}"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <remarks>Do not use 'async void' methods; they must return a <see cref="Task"/>. Refer to <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> for argument usage details.</remarks>
      IExitAction<TArgument> OnEnter(Func<IStateController<TEvent>, TArgument, Task> enterAction);
    }
  }
}