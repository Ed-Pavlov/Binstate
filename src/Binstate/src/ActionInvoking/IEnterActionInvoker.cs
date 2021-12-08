﻿using System.Threading.Tasks;

namespace Binstate;

/// <summary>
///   Interface of the invoker of the enter action is used to be able to assign both generic and plain invoker instances to the one variable.
///   See <see cref="State{TState,TEvent,TArgument}.EnterSafe" /> implementation for details.
/// </summary>
internal interface IEnterActionInvoker { }

/// <summary>
///   This interface is used to make <typeparamref name="TArgument" /> contravariant to be able to pass to the <see cref="StateMachine{TState,TEvent}.Raise" />
///   argument assignable to the <see cref="State{TState,TEvent,TArgument}.EnterSafe(IStateController{TEvent},System.Action{System.Exception})" /> but not exactly the same.
///   See casting of these types in implementation of the <see cref="State{TState,TEvent,TArgument}" /> class.
/// </summary>

// ReSharper disable once TypeParameterCanBeVariant
internal interface IEnterActionInvoker<TEvent, in TArgument> : IEnterActionInvoker
{
  Task? Invoke(IStateController<TEvent> isInState, TArgument argument);
}