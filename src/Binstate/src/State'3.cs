using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This class describes state machine's state which requires an argument to the enter action.
  ///
  /// All these complex generic stuff is introduced to avoid casting to 'object' and thus avoid boxing when value type instance is used as the argument.
  /// </summary>
  internal class State<TState, TEvent, TArgument> : State<TState, TEvent>, IState<TState, TEvent, TArgument>
  {
    public TArgument Argument;

    public State(
      [NotNull] TState id,
      [CanBeNull] IEnterActionInvoker<TEvent> enterAction,
      [CanBeNull] Action exit,
      [NotNull] Dictionary<TEvent, Transition<TState, TEvent>> transitions,
      [CanBeNull] State<TState, TEvent> parentState) : base(id, enterAction, typeof(TArgument), exit, transitions, parentState)
    {
    }

    public void EnterSafe(IStateMachine<TEvent> stateMachine, TArgument argument, Action<Exception> onException)
    {
      Enter(onException, enter =>
        {
          if(Binstate.Argument.IsSpecified<TArgument>())
            Argument = argument; // remember an argument passed into enter action if any
          
          var typedEnter = (IEnterActionInvoker<TEvent, TArgument>) enter;
          return typedEnter.Invoke(stateMachine, argument);
        });
    }
    
    public MixOf<TA, TArgument> CreateTuple<TA>(TA argument) => new MixOf<TA, TArgument>(argument.ToMaybe(), Argument.ToMaybe());  
  }
}