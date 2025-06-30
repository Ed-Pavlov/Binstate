using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class EnterAction : ExitAction, IEnterAction
    {
      private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      internal EnterAction(StateConfig<Unit> stateConfig) : base(stateConfig) { }

      public IExitAction OnEnter(Action? enterAction = null)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        return OnEnter(_ => enterAction());
      }

      public IExitAction OnEnter(Action<IStateController<TEvent>> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = State<TState, TEvent, Unit>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction OnEnter(Func<Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = State<TState, TEvent, Unit>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction OnEnter(Func<IStateController<TEvent>, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = State<TState, TEvent, Unit>.EnterAction.Create(enterAction);
        return this;
      }

//      public IExitAction<TArgument> OnEnter<TArgument>(Action<TArgument> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
//
//        return OnEnter<TArgument>((_, argument) => enterAction(argument));
//      }
//
//      public IExitAction<TArgument> OnEnter<TArgument>(Action<IStateController<TEvent>, TArgument> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
//
//        StateData.EnterAction = WrapAction(enterAction);
//        StateData._factory     = new StateFactory<TArgument>();
//        return new ExitAction<TArgument>(StateData);
//      }
//
//      public IExitAction<TArgument> OnEnter<TArgument>(Func<TArgument, Task> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//
//        return OnEnter<TArgument>((_, argument) => enterAction(argument));
//      }
//
//      public IExitAction<TArgument> OnEnter<TArgument>(Func<IStateController<TEvent>, TArgument, Task> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//
//        StateData.EnterAction = enterAction;
//        StateData._factory     = new StateFactory<TArgument>();
//        return new ExitAction<TArgument>(StateData);
//      }
//
//      public IExitAction<ITuple<TArgument, TPropagate>> OnEnter<TArgument, TPropagate>(Action<TArgument, TPropagate> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
//
//        return OnEnter<ITuple<TArgument, TPropagate>>(tuple => enterAction(tuple!.ItemX, tuple.ItemY));
//      }
//
//      public IExitAction<ITuple<TArgument, TPropagate>> OnEnter<TArgument, TPropagate>(Action<IStateController<TEvent>, TArgument, TPropagate> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);
//
//        return OnEnter<ITuple<TArgument, TPropagate>>((stateMachine, tuple) => enterAction(stateMachine, tuple!.ItemX, tuple.ItemY));
//      }
//
//      public IExitAction<ITuple<TArgument, TPropagate>> OnEnter<TArgument, TPropagate>(Func<TArgument, TPropagate, Task> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//
//        return OnEnter<ITuple<TArgument, TPropagate>>(tuple => enterAction(tuple!.ItemX, tuple.ItemY));
//      }
//
//      public IExitAction<ITuple<TArgument, TPropagate>> OnEnter<TArgument, TPropagate>(Func<IStateController<TEvent>, TArgument, TPropagate, Task> enterAction)
//      {
//        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
//
//        return OnEnter<ITuple<TArgument, TPropagate>>((stateMachine, tuple) => enterAction(stateMachine, tuple!.ItemX, tuple.ItemY));
//      }

      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
    }
  }
}