using System;
using System.Threading.Tasks;

namespace Binstate
{
  internal class EnterInvoker
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

  internal class NoParameterEnterInvoker : EnterInvoker
  {
    private readonly Func<IStateMachine, Task> _action;
    public NoParameterEnterInvoker(Func<IStateMachine, Task> action) => _action = action;

    public Task Invoke(IStateMachine stateMachine) => _action(stateMachine);
  }
  
  internal class EnterInvoker<T> : EnterInvoker
  {
    private readonly Func<IStateMachine, T, Task> _action;
    
    public EnterInvoker(Func<IStateMachine, T, Task> action) => _action = action;
    
    public Task Invoke(IStateMachine isInState, T arg) => _action(isInState, arg);
  }
}