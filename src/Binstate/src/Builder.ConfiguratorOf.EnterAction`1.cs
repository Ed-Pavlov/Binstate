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

      internal EnterAction(StateConfig stateConfig) : base(stateConfig) { }

      public IExitAction<TStateArgument> OnEnter(Action enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = enterAction;
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Action<IStateController<TEvent>> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = ConvertToGeneralForm(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Func<Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = enterAction;
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Func<IStateController<TEvent>, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = enterAction;
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Action<TStateArgument> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        return OnEnter((_, argument) => enterAction(argument));
      }

      public IExitAction<TStateArgument> OnEnter(Action<IStateController<TEvent>, TStateArgument> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));
        if(IsAsyncMethod(enterAction.Method)) throw new ArgumentException(AsyncVoidMethodNotSupported);

        StateConfig.EnterAction = ConvertToGeneralForm(enterAction);
        return this;
      }

      public IExitAction<TStateArgument> OnEnter(Func<TStateArgument, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        return OnEnter((_, argument) => enterAction(argument));
      }

      public IExitAction<TStateArgument>  OnEnter(Func<IStateController<TEvent>, TStateArgument, Task> enterAction)
      {
        if(enterAction is null) throw new ArgumentNullException(nameof(enterAction));

        StateConfig.EnterAction = enterAction;
        return this;
      }

      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) is not null;

      private static Func<IStateController<TEvent>, Task?> ConvertToGeneralForm(Action<IStateController<TEvent>> enterAction)
        => controller =>
        {
          enterAction(controller);
          return null;
        };

      private static Func<IStateController<TEvent>, TStateArgument, Task?> ConvertToGeneralForm(
        Action<IStateController<TEvent>, TStateArgument> enterAction)
        => (controller, argument) =>
        {
          enterAction(controller, argument);
          return null;
        };
    }
  }
}