using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Binstate
{
  public class StateConfigContinuation : TransitionConfig
  {
    internal readonly object State;
    internal EnterInvoker? Enter;
    internal Action? Exit;

    internal StateConfigContinuation(object state) => State = state;

    public TransitionConfig OnExit(Action onExit)
    {
      Exit = onExit;
      return this;
    }
  }
  
  public class StateConfig : StateConfigContinuation
  {
    internal StateConfig(object state) : base(state){}

    public StateConfigContinuation OnEntry(Action<IStateMachine> onEntry)
    {
      if(IsAsyncMethod(onEntry.Method))
        throw new InvalidOperationException("'async void' methods are not supported, use Task return type for async method");
      
      Enter = EnterInvoker.Create(onEntry);
      return this;
    }

    public StateConfigContinuation OnEntry(Func<IStateMachine, Task> onEntry)
    {
      Enter = EnterInvoker.Create(onEntry);
      return this;
    }

    public StateConfigContinuation OnEntry<T>(Action<IStateMachine, T> onEntry)
    {
      if(IsAsyncMethod(onEntry.Method))
        throw new InvalidOperationException("'async void' methods are not supported, use Task return type for async method");
      
      Enter = EnterInvoker.Create(onEntry);
      return this;
    }

    public StateConfigContinuation OnEntry<T>(Func<IStateMachine, T, Task> onEntry)
    {
      Enter = EnterInvoker.Create(onEntry);
      return this;
    }
    
    private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
  }
}