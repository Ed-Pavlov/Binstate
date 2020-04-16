using System;
using System.Threading.Tasks;

namespace Binstate
{
  
  /// <summary>
  /// Base class of the invoker of enter action is used to be able to assign both generic and plain invoker instances to the one variable.
  /// See <see cref="State{TEvent, TState}.Enter{T}"/> implementation for details  
  /// </summary>
  internal abstract class EnterActionInvoker<TEvent>
  {
    public static NoParameterEnterInvoker<TEvent> Create(Action<IStateMachine<TEvent>> action) => new NoParameterEnterInvoker<TEvent>((stateMachine) =>
    {
      action(stateMachine);
      return null;
    });
    
    public static NoParameterEnterInvoker<TEvent> Create(Func<IStateMachine<TEvent>, Task> action) => new NoParameterEnterInvoker<TEvent>(action);
    
    public static EnterInvoker<TEvent, TArg> Create<TArg>(Action<IStateMachine<TEvent>, TArg> action) => new EnterInvoker<TEvent, TArg>((stateMachine, arg) =>
    {
       action(stateMachine, arg);
       return null;
    });
    
    public static EnterInvoker<TEvent, TArg> Create<TArg>(Func<IStateMachine<TEvent>, TArg, Task> action) => new EnterInvoker<TEvent, TArg>(action);
  }

  internal class NoParameterEnterInvoker<TEvent> : EnterActionInvoker<TEvent>
  {
    private readonly Func<IStateMachine<TEvent>, Task> _action;
    public NoParameterEnterInvoker(Func<IStateMachine<TEvent>, Task> action) => _action = action;

    public Task Invoke(IStateMachine<TEvent> stateMachine) => _action(stateMachine);
  }
  
  
  /// <summary>
  /// Generic version of the invoker of enter action introduced to avoid boxing in case of Value Type parameter
  /// </summary>
  internal class EnterInvoker<TEvent, TArg> : EnterActionInvoker<TEvent>
  {
    private readonly Func<IStateMachine<TEvent>, TArg, Task> _action;
    
    public EnterInvoker(Func<IStateMachine<TEvent>, TArg, Task> action) => _action = action;
    
    public Task Invoke(IStateMachine<TEvent> isInState, TArg arg) => _action(isInState, arg);
  }
}