using System;
using System.Threading.Tasks;

namespace Binstate;

internal class NoParameterEnterActionActionInvoker<TEvent> : IEnterActionInvoker
{
  private readonly Func<IStateMachine<TEvent>, Task?> _action;

  public NoParameterEnterActionActionInvoker(Func<IStateMachine<TEvent>, Task?> action) => _action = action;

  public Task? Invoke(IStateMachine<TEvent> stateMachine) => _action(stateMachine);
}

/// <summary>
/// Generic version of the invoker of enter action introduced to avoid boxing in case of Value Type parameter
/// </summary>
internal class EnterActionInvoker<TEvent, TArgument> : IEnterActionInvoker<TEvent, TArgument>
{
  private readonly Func<IStateMachine<TEvent>, TArgument?, Task?> _action;

  public EnterActionInvoker(Func<IStateMachine<TEvent>, TArgument?, Task?> action) => _action = action;

  public Task? Invoke(IStateMachine<TEvent> isInState, TArgument? argument) => _action(isInState, argument);
}