using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BeatyBit.Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    internal class EnterAction<TStateArgument> : ExitAction<TStateArgument>, IEnterAction<TStateArgument>
    {
      private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";

      internal EnterAction(StateConfig<TStateArgument> stateConfig) : base(stateConfig) { }

      public IExitAction<TStateArgument> OnEnter(Action enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Action<IStateController<TEvent>> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Func<Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Func<IStateController<TEvent>, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Action<TStateArgument> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Action<IStateController<TEvent>, TStateArgument> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Func<TStateArgument, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      public IExitAction<TStateArgument>  OnEnter(Func<IStateController<TEvent>, TStateArgument, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = State<TState, TEvent, TStateArgument>.EnterAction.Create(enterAction);
        return this;
      }

      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;
    }
  }
}