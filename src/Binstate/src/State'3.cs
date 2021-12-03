using System;
using System.Collections.Generic;

namespace Binstate;

/// <summary>
/// This class describes state machine's state which requires an argument to the enter action.
///
/// All these complex generic stuff is introduced to avoid casting to 'object' and thus avoid boxing when value type instance is used as the argument.
/// </summary>
internal class State<TState, TEvent, TArgument> : State<TState, TEvent>, IState<TState, TEvent, TArgument>
  where TState : notnull where TEvent : notnull
{
  private TArgument? _argument;

  public TArgument Argument
  {
    get => _argument ?? throw new InvalidOperationException("Argument is not set");
    private set => _argument = value;
  }

  public State(
    TState                                         id,
    IEnterActionInvoker?                           enterAction,
    IExitActionInvoker?                            exitAction,
    Dictionary<TEvent, Transition<TState, TEvent>> transitions,
    State<TState, TEvent>?                         parentState) : base(id, enterAction, typeof(TArgument), exitAction, transitions, parentState) { }

  public void EnterSafe(IStateMachine<TEvent> stateMachine, TArgument argument, Action<Exception> onException)
    => Enter(
      onException,
      enter =>
      {
        if(Binstate.Argument.IsSpecified<TArgument>())
          Argument = argument; // remember an argument passed into enter action if any

        var typedEnter = (IEnterActionInvoker<TEvent, TArgument>) enter;
        return typedEnter.Invoke(stateMachine, argument);
      });

  public override void ExitSafe(Action<Exception> onException)
    => Exit(
      onException,
      exit =>
      {
        if(exit is ExitActionInvoker<TArgument> typedExitAction)
          typedExitAction.Invoke(Argument);
        else
          base.ExitSafe(onException);
      });

  public MixOf<TA, TArgument> CreateTuple<TA>(TA argument) => new(argument.ToMaybe(), Argument.ToMaybe());
}