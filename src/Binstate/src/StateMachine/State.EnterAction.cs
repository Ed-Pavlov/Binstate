using System;
using System.Threading.Tasks;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal sealed partial class State<TState, TEvent, TArgument>
{
  public class EnterAction
  {
    private readonly Func<IStateController<TEvent>, TArgument, Task?> _userAction;
    private readonly bool                                             _requireArgument;

    private EnterAction(Func<IStateController<TEvent>, TArgument, Task?> userAction, bool requireArgument)
    {
      _userAction      = userAction ?? throw new ArgumentNullException(nameof(userAction));
      _requireArgument = requireArgument;
    }

    public Task? Call(IStateController<TEvent> controller, Maybe<TArgument> argument) => _userAction(controller, _requireArgument ? argument.Value : default!);

    public static EnterAction Create(Func<IStateController<TEvent>, TArgument, Task> action) => new EnterAction(action, true);
    public static EnterAction Create(Func<TArgument, Task>                           action) => new EnterAction((_, argument) => action(argument), true);
    public static EnterAction Create(Func<IStateController<TEvent>, Task>            action) => new EnterAction((controller, _) => action(controller), false);
    public static EnterAction Create(Func<Task>                                      action) => new EnterAction((_, _) => action(), false);

    public static EnterAction Create(Action action)
      => new EnterAction((_, _) =>
        {
          action();
          return null;
        }, false
      );

    public static EnterAction Create(Action<IStateController<TEvent>> action)
      => new EnterAction((controller, _) =>
        {
          action(controller);
          return null;
        }, false
      );

    public static EnterAction Create(Action<IStateController<TEvent>, TArgument> action)
      => new EnterAction((controller, argument) =>
        {
          action(controller, argument);
          return null;
        }, true
      );

    public static EnterAction Create(Action<TArgument> action)
      => new EnterAction((_, argument) =>
        {
          action(argument);
          return null;
        }, true
      );
  }
}