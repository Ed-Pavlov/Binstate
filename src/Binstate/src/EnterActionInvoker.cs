using System;
using System.Threading.Tasks;

namespace Binstate
{
  
  /// <summary>
  /// Base class of the invoker of enter action is used to be able to assign both generic and plain invoker instances to the one variable.
  /// See <see cref="State.Enter{T}"/> implementation for details  
  /// </summary>
  internal abstract class EnterActionInvoker
  {
    public static NoParameterEnterInvoker Create(Action<IStateMachine> action) => new NoParameterEnterInvoker((stateMachine) =>
    {
      action(stateMachine);
      return null;
    });
    
    public static NoParameterEnterInvoker Create(Func<IStateMachine, Task> action) => new NoParameterEnterInvoker(action);
    
    public static EnterInvoker<T> Create<T>(Action<IStateMachine, T> action) => new EnterInvoker<T>((stateMachine, arg) =>
    {
       action(stateMachine, arg);
       return null;
    });
    
    public static EnterInvoker<T> Create<T>(Func<IStateMachine, T, Task> action) => new EnterInvoker<T>(action);
  }

  internal class NoParameterEnterInvoker : EnterActionInvoker
  {
    private readonly Func<IStateMachine, Task> _action;
    public NoParameterEnterInvoker(Func<IStateMachine, Task> action) => _action = action;

    public Task Invoke(IStateMachine stateMachine) => _action(stateMachine);
  }
  
  
  /// <summary>
  /// Generic version of the invoker of enter action introduced to avoid boxing in case of Value Type parameter
  /// </summary>
  /// <typeparam name="T"></typeparam>
  internal class EnterInvoker<T> : EnterActionInvoker
  {
    private readonly Func<IStateMachine, T, Task> _action;
    
    public EnterInvoker(Func<IStateMachine, T, Task> action) => _action = action;
    
    public Task Invoke(IStateMachine isInState, T arg) => _action(isInState, arg);
  }
}