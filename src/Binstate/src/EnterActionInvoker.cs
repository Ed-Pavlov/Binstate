using System;
using System.Threading.Tasks;

namespace Binstate
{
  /// <summary>
  /// Interface of the invoker of the enter action is used to be able to assign both generic and plain invoker instances to the one variable.
  /// See <see cref="State{TEvent, TState}.Enter{T}"/> implementation for details.
  /// </summary>
  internal interface IEnterInvoker<TEvent>
  {
  }

  /// <summary>
  /// This interface is used to make <typeparam name="TArg"> contravariant to be able to pass to the <see cref="StateMachine{TState,TEvent}.Raise"/>
  /// argument assignable to the <see cref="State{TState,TEvent}.Enter{TArgument}"/> but not exactly the same. See casting of these types
  /// in implementation of the <see cref="State{TState,TEvent}" class/> </typeparam>
  /// </summary>
  internal interface IEnterActionInvoker<TEvent, in TArg> : IEnterInvoker<TEvent>
  {
    Task Invoke(IStateMachine<TEvent> isInState, TArg arg);
  }

  internal class NoParameterEnterActionInvoker<TEvent> : IEnterInvoker<TEvent>
  {
    private readonly Func<IStateMachine<TEvent>, Task> _action;
    public NoParameterEnterActionInvoker(Func<IStateMachine<TEvent>, Task> action) => _action = action;

    public Task Invoke(IStateMachine<TEvent> stateMachine) => _action(stateMachine);
  }

  /// <summary>
  /// Generic version of the invoker of enter action introduced to avoid boxing in case of Value Type parameter
  /// </summary>
  internal class EnterActionInvoker<TEvent, TArg> : IEnterActionInvoker<TEvent, TArg>
  {
    private readonly Func<IStateMachine<TEvent>, TArg, Task> _action;

    public EnterActionInvoker(Func<IStateMachine<TEvent>, TArg, Task> action) => _action = action;

    public Task Invoke(IStateMachine<TEvent> isInState, TArg arg) => _action(isInState, arg);
  }
}