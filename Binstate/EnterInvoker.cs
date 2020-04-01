using System;
using System.Threading.Tasks;

namespace Binstate
{
  internal class EnterInvoker
  {
    private readonly Func<IStateMachine, object?, Task?> _action;

    private EnterInvoker(Func<IStateMachine, object?, Task?> action) => _action = action;

    public Task? Invoke(IStateMachine isInState, object? arg) => _action(isInState, arg);

    public static EnterInvoker Create(Action<IStateMachine> action) => new EnterInvoker((isInState, arg) =>
    {
      action(isInState);
      return null;
    });

    public static EnterInvoker Create<T>(Action<IStateMachine, T> action) => new EnterInvoker((isInState, arg) =>
    {
 #pragma warning disable 8601
      action(isInState, (T) arg);
#pragma warning restore 8601
      return null;
    });
    
    public static EnterInvoker Create(Func<IStateMachine, Task> action) => new EnterInvoker((isInState, arg) => action(isInState));
    
#pragma warning disable 8601
    public static EnterInvoker Create<T>(Func<IStateMachine, T, Task> action) => new EnterInvoker((isInState, arg) => action(isInState, (T)arg));
#pragma warning restore 8601
  }
}