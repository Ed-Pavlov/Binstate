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

    public StateConfigContinuation OnEnter(Action<IStateMachine> onEnter)
    {
      if(IsAsyncMethod(onEnter.Method))
        throw new InvalidOperationException("'async void' methods are not supported, use Task return type for async method");
      
      Enter = EnterInvoker.Create(onEnter);
      return this;
    }

    public StateConfigContinuation OnEnter(Func<IStateMachine, Task> onEnter)
    {
      Enter = EnterInvoker.Create(onEnter);
      return this;
    }

    public StateConfigContinuation OnEnter<T>(Action<IStateMachine, T> onEnter)
    {
      if(IsAsyncMethod(onEnter.Method))
        throw new InvalidOperationException("'async void' methods are not supported, use Task return type for async method");
      
      Enter = EnterInvoker.Create(onEnter);
      return this;
    }

    public StateConfigContinuation OnEnter<T>(Func<IStateMachine, T, Task> onEnter)
    {
      Enter = EnterInvoker.Create(onEnter);
      return this;
    }
    
    private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
  }
}