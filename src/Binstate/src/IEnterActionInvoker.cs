﻿using System.Threading.Tasks;

namespace Binstate
{
  /// <summary>
  /// Interface of the invoker of the enter action is used to be able to assign both generic and plain invoker instances to the one variable.
  /// See <see cref="State{TState,TEvent}.EnterSafe{TArgument}"/> implementation for details.
  /// </summary>
  internal interface IEnterActionInvoker<TEvent>
  {
  }
  
  /// <summary>
  /// This interface is used to make <typeparamref name="TArgument"/> contravariant to be able to pass to the <see cref="StateMachine{TState,TEvent}.Raise"/>
  /// argument assignable to the <see cref="State{TState,TEvent}.EnterSafe{TArgument}"/> but not exactly the same. See casting of these types
  /// in implementation of the <see cref="State{TState,TEvent}"/> class
  /// </summary>
  internal interface IEnterActionInvoker<TEvent, in TArgument> : IEnterActionInvoker<TEvent>
  {
    Task Invoke(IStateMachine<TEvent> isInState, TArgument argument);
  }

}