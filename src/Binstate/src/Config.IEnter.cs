using System;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  /// This class is used to configure enter action of the currently configured state.
  /// </summary>
  public interface IEnter : IExitEx
  {
    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload is used to provide blocking action, to provide async action use corresponding overloads of this method.
    /// </summary>
    IExitEx OnEnter(Action enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload allows to perform auto transition or to not exit <paramref name="enterAction"/> till the state will be deactivated
    /// using <see cref="IStateController{TEvent}"/> passed to the provided action.
    /// This overload is used to provide blocking action, to provide async action use corresponding overloads of this method.
    /// </summary>
    IExitEx OnEnter(Action<IStateController<TEvent>> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExitEx OnEnter(Func<Task> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload allows to perform auto transition or to not exit <paramref name="enterAction"/> till the state will be deactivated
    /// using <see cref="IStateController{TEvent}"/> passed to the provided action.
    /// This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExitEx OnEnter(Func<IStateController<TEvent>, Task> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload defines the action accepting the argument. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide blocking action, to provide async action use corresponding overloads of this method.
    /// </summary>
    IExit<TArgument> OnEnter<TArgument>(Action<TArgument> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload allows to perform auto transition or to not exit <paramref name="enterAction"/> till the state will be deactivated
    /// using <see cref="IStateController{TEvent}"/> passed to the provided action.
    /// This overload defines the action accepting the argument. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide blocking action, to provide async action use corresponding overloads of this method.
    /// </summary>
    IExit<TArgument> OnEnter<TArgument>(Action<IStateController<TEvent>, TArgument> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload defines the action accepting the argument. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<TArgument> OnEnter<TArgument>(Func<TArgument, Task> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload allows to perform auto transition or to not exit <paramref name="enterAction"/> till the state will be deactivated
    /// using <see cref="IStateController{TEvent}"/> passed to the provided action.
    /// This overload defines the action accepting the argument. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<TArgument> OnEnter<TArgument>(Func<IStateController<TEvent>, TArgument, Task> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload defines the action accepting two arguments. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide blocking action, to provide async action use corresponding overloads of this method.
    /// </summary>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<TArgument, TRelay> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload allows to perform auto transition or to not exit <paramref name="enterAction"/> till the state will be deactivated
    /// using <see cref="IStateController{TEvent}"/> passed to the provided action.
    /// This overload defines the action accepting two arguments. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide blocking action, to provide async action use corresponding overloads of this method.
    /// </summary>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<IStateController<TEvent>, TArgument, TRelay> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload defines the action accepting two arguments. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<TArgument, TRelay, Task> enterAction);

    /// <summary>
    /// Specifies the action to be called on entering the currently configured state.
    /// This overload allows to perform auto transition or to not exit <paramref name="enterAction"/> till the state will be deactivated
    /// using <see cref="IStateController{TEvent}"/> passed to the provided action.
    /// This overload defines the action accepting two arguments. Read about arguments in <see cref="IStateMachine{TEvent}.Raise{TArgument}"/> method
    /// documentation.
    /// This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<IStateController<TEvent>, TArgument, TRelay, Task> enterAction);
  }
}