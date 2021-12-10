namespace Binstate;

/// <summary>
///   This class provides syntax-sugar to configure the state machine.
/// </summary>
public static partial class Config<TState, TEvent> where TState : notnull where TEvent : notnull
{
  /// <summary>
  ///   There are two types of the state in the system with and w/o Argument. In order to make a code type safe and avoid
  ///   boxing of value type arguments, the <see cref="State{TState, TEvent, TArguments}" /> class has TArgument generic argument.
  ///   What type will be used depends on the state Enter, Exit, and Transition actions configuration and became known during runtime.
  ///   If no arguments required the type <see cref="Unit"/> is used as a TArgument and it is treated by the implementation as "no argument required".
  /// </summary>
  internal interface IStateFactory
  {
    IState<TState, TEvent> CreateState(StateConfig stateConfig, IState<TState, TEvent>? parentState);
  }

  private class StateFactory : IStateFactory
  {
    public IState<TState, TEvent> CreateState(StateConfig stateConfig, IState<TState, TEvent>? parentState)
      => new State<TState, TEvent, Unit>(
        stateConfig.StateId,
        stateConfig.EnterAction,
        stateConfig.ExitAction,
        stateConfig.TransitionList,
        parentState
      );
  }

  private class StateFactory<TArgument> : IStateFactory
  {
    public IState<TState, TEvent> CreateState(StateConfig stateConfig, IState<TState, TEvent>? parentState)
      => new State<TState, TEvent, TArgument>(
        stateConfig.StateId,
        stateConfig.EnterAction,
        stateConfig.ExitAction,
        stateConfig.TransitionList,
        parentState
      );
  }
}