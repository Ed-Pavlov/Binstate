using System;
using System.Threading.Tasks;

namespace Binstate;

internal class EnterActionInvoker<TEvent> : IEnterActionInvoker
{
  private readonly Func<IStateController<TEvent>, Task?> _action;

  public EnterActionInvoker(Func<IStateController<TEvent>, Task?> action) => _action = action;

  public Task? Invoke(IStateController<TEvent> stateController) => _action(stateController);
}

/// <summary>
///   Generic version of the invoker of enter action introduced to avoid boxing in case of Value Type parameter
/// </summary>
internal class EnterActionInvoker<TEvent, TArgument> : IEnterActionInvoker<TEvent, TArgument>
{
  private readonly Func<IStateController<TEvent>, TArgument, Task?> _action;

  public EnterActionInvoker(Func<IStateController<TEvent>, TArgument, Task?> action) => _action = action;

  public Task? Invoke(IStateController<TEvent> isInState, TArgument argument) => _action(isInState, argument);
}