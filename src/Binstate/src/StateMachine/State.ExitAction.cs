using System;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal sealed partial class State<TState, TEvent, TArgument>
{
  public class ExitAction
  {
    private readonly Action<TArgument> _userAction;
    private readonly bool              _requireArgument;

    public ExitAction(Action<TArgument> action, bool requireArgument)
    {
      _userAction      = action ?? throw new ArgumentNullException(nameof(action));
      _requireArgument = requireArgument;
    }

    public void Call(Maybe<TArgument> argument) => _userAction(_requireArgument ? argument.Value : default!);

    public static ExitAction Create(Action            action) => new ExitAction(_ => action(), false);
    public static ExitAction Create(Action<TArgument> action) => new ExitAction(action,        true);
  }
}