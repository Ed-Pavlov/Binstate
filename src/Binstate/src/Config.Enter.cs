using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Binstate;

public static partial class Config<TState, TEvent>
{
  internal class Enter : Exit, IEnter
  {
    private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

    internal Enter(StateConfig stateConfig) : base(stateConfig) { }

    public IExitEx OnEnter(Action enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
      if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

      return OnEnter(_ => enterAction());
    }

    public IExitEx OnEnter(Action<IStateController<TEvent>> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
      if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

      StateConfig.EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
      return this;
    }

    public IExitEx OnEnter(Func<Task> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

      return OnEnter(_ => enterAction());
    }

    public IExitEx OnEnter(Func<IStateController<TEvent>, Task> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

      StateConfig.EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
      return this;
    }

    public IExit<TArgument> OnEnter<TArgument>(Action<TArgument> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
      if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

      return OnEnter<TArgument>((_, argument) => enterAction(argument));
    }

    public IExit<TArgument> OnEnter<TArgument>(Action<IStateController<TEvent>, TArgument> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
      if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

      StateConfig.EnterAction = EnterActionInvokerFactory<TEvent>.Create(enterAction);
      StateConfig.Factory       = new StateFactory<TArgument>();
      return new Exit<TArgument>(StateConfig);
    }

    public IExit<TArgument> OnEnter<TArgument>(Func<TArgument, Task> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

      return OnEnter<TArgument>((_, argument) => enterAction(argument));
    }

    public IExit<TArgument> OnEnter<TArgument>(Func<IStateController<TEvent>, TArgument, Task> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

      StateConfig.EnterAction  = EnterActionInvokerFactory<TEvent>.Create(enterAction);
      StateConfig.Factory = new StateFactory<TArgument>();
      return new Exit<TArgument>(StateConfig);
    }

    public IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<TArgument, TRelay> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
      if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

      return OnEnter<ITuple<TArgument, TRelay>>(tuple => enterAction(tuple!.PassedArgument, tuple.RelayedArgument));
    }

    public IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Action<IStateController<TEvent>, TArgument, TRelay> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
      if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

      return OnEnter<ITuple<TArgument, TRelay>>((stateMachine, tuple) => enterAction(stateMachine, tuple!.PassedArgument, tuple.RelayedArgument));
    }

    public IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<TArgument, TRelay, Task> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

      return OnEnter<ITuple<TArgument, TRelay>>(tuple => enterAction(tuple!.PassedArgument, tuple.RelayedArgument));
    }

    public IExit<ITuple<TArgument, TRelay>> OnEnter<TArgument, TRelay>(Func<IStateController<TEvent>, TArgument, TRelay, Task> enterAction)
    {
      if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

      return OnEnter<ITuple<TArgument, TRelay>>((stateMachine, tuple) => enterAction(stateMachine, tuple!.PassedArgument, tuple.RelayedArgument));
    }

    private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
  }
}