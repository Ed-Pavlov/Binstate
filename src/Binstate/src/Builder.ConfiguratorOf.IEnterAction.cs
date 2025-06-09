using System;
using System.Threading.Tasks;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    /// <summary>
    /// This interface allows you to specify an action that will be called upon entering the state currently being configured.
    /// </summary>
    public interface IEnterAction : IExitAction
    {
      /// <summary>
      /// Specifies a synchronous (blocking) action to be executed upon entering the state.
      /// </summary>
      /// <param name="enterAction">The synchronous action to execute.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <exception cref="ArgumentException">Thrown when <paramref name="enterAction"/> is an 'async void' method.</exception>
      IExitAction OnEnter(Action? enterAction = null);

      /// <summary>
      /// Specifies a synchronous (blocking) action with access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The synchronous action with access to the state controller.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <exception cref="ArgumentException">Thrown when <paramref name="enterAction"/> is an 'async void' method.</exception>
      IExitAction OnEnter(Action<IStateController<TEvent>> enterAction);

      /// <summary>
      /// Specifies an asynchronous (non-blocking) action to be executed upon entering the state.
      /// </summary>
      /// <param name="enterAction">The asynchronous action returning a <see cref="Task"/>.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <remarks>Do not use 'async void' methods; they must return a <see cref="Task"/>.</remarks>
      IExitAction OnEnter(Func<Task> enterAction);

      /// <summary>
      /// Specifies an asynchronous (non-blocking) action with access to the state controller, to be executed upon entering the state.
      /// This overload provides the ability to perform auto transitions or waiting till the state be exited through <see cref="IStateController{TEvent}"/>.
      /// </summary>
      /// <param name="enterAction">The asynchronous action with access to the state controller, returning a <see cref="Task"/>.</param>
      /// <returns>An <see cref="IExitAction"/> instance for configuring the exit action.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="enterAction"/> is null.</exception>
      /// <remarks>Do not use 'async void' methods; they must return a <see cref="Task"/>.</remarks>
      IExitAction OnEnter(Func<IStateController<TEvent>, Task> enterAction);
    }
  }
}