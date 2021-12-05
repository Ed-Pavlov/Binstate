using System;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  ///   This class is used to configure enter action of the currently configured state.
  /// </summary>
  public interface IEnter : IExitEx
  {
    /// <summary>
    ///   Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
    ///   or transition to another one is not needed.
    ///   This overload is used to provide blocking action. To provide async action use
    ///   <see cref="Enter.OnEnter(System.Func{Binstate.IStateMachine{TEvent},System.Threading.Tasks.Task})" />.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExitEx OnEnter(Action enterAction);

    /// <summary>
    ///   Specifies the action to be called on entering the currently configured state.
    ///   This overload is used to provide blocking action. To provide async action use
    ///   <see
    ///     cref="Enter.OnEnter(System.Func{Binstate.IStateMachine{TEvent},System.Threading.Tasks.Task})" />
    ///   .
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExitEx OnEnter(Action<IStateMachine<TEvent>> enterAction);

    /// <summary>
    ///   Specifies the simple action to be called on entering the currently configured state in case of controlling the current state
    ///   or transition to another one is not needed.
    ///   This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExitEx OnEnter(Func<Task> enterAction);

    /// <summary>
    ///   Specifies the action to be called on entering the currently configured state.
    ///   This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExitEx OnEnter(Func<IStateMachine<TEvent>, Task> enterAction);

    /// <summary>
    ///   Specifies the simple action with parameter to be called on entering the currently configured state in case of controlling the current state
    ///   or transition to another one is not needed.
    ///   This overload is used to provide blocking action. To provide async action use
    ///   <see>
    ///     <cref> OnEnter(Func{IStateMachine, T, Task}) </cref>
    ///   </see>
    ///   .
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<TArgument> OnEnter<TArgument>(Action<TArgument> enterAction);

#pragma warning disable 1574
    /// <summary>
    ///   Specifies the action with parameter to be called on entering the currently configured state.
    ///   This overload is used to provide blocking action. To provide async action use
    ///   <see cref="OnEnter{TArgument}(System.Func{IStateMachine{TEvent}, TArgument, Task})" />
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
#pragma warning restore 1574
    IExit<TArgument> OnEnter<TArgument>(Action<IStateMachine<TEvent>, TArgument> enterAction);

    /// <summary>
    ///   Specifies the simple action with parameter to be called on entering the currently configured state in case of controlling the current state
    ///   or transition to another one is not needed.
    ///   This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<TArgument> OnEnter<TArgument>(Func<TArgument, Task> enterAction);

    /// <summary>
    ///   Specifies the action with parameter to be called on entering the currently configured state.
    ///   This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<TArgument> OnEnter<TArgument>(Func<IStateMachine<TEvent>, TArgument, Task> enterAction);

    /// <summary>
    ///   Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}" />
    ///   and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}()" />
    ///   to be called on entering the currently configured state for case when controlling the current state
    ///   or transition to another one is not needed.
    ///   This overload is used to provide blocking action. To provide async action use
    ///   <see cref="Enter.OnEnter{TArgument,TRelay}(System.Func{TArgument,TRelay,System.Threading.Tasks.Task})" />
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<TArgument, TRelay> enterAction);

#pragma warning disable 1574
    /// <summary>
    ///   Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}" />
    ///   and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}()" />
    ///   to be called on entering the currently configured state.
    ///   This overload is used to provide blocking action. To provide async action use
    ///   <see cref="OnEnter{TArgument, TRelay}(Func{IStateMachine{TEvent}, TArgument, TRelay, Task})" />
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
#pragma warning restore 1574
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<IStateMachine<TEvent>, TArgument, TRelay> enterAction);

    /// <summary>
    ///   Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}" />
    ///   and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}()" />
    ///   to be called on entering the currently configured state for case when controlling the current state
    ///   or transition to another one is not needed.
    ///   This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<TArgument, TRelay, Task> enterAction);

    /// <summary>
    ///   Specifies the action with two parameters, one passed using <see cref="StateMachine{TState,TEvent}.Raise{TArgument}" />
    ///   and another relayed from the currently active state using <see cref="StateMachine{TState,TEvent}.Relaying{TRelay}()" />
    ///   to be called on entering the currently configured state.
    ///   This overload is used to provide non-blocking async action.
    /// </summary>
    /// <remarks> Do not use async void methods, async methods should return <see cref="Task" /> </remarks>
    IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<IStateMachine<TEvent>, TArgument, TRelay, Task> enterAction);
  }
}