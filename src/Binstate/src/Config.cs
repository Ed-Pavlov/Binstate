namespace Binstate;

/// <summary>
///   This class provides syntax-sugar to configure the state machine.
/// </summary>
public static partial class Config<TState, TEvent> where TState : notnull where TEvent : notnull
{

  /// <summary>
  ///   There are two types of the state in the system with and w/o Argument. In order to make a code type safe and avoid
  ///   boxing of value type arguments, complex generic types machinery is introduced. This is the first part of this machinery,
  ///   Factories what creates <see cref="State{TState, TEvent, Unit}" /> or <see cref="State{TState, TEvent, TArguments}" />. Depending on was 'enter' action
  ///   with parameter is defined for the state one of these types are instantiated.
  ///   See <see cref="IEnterActionInvoker{TEvent,TArgument}" />, <see cref="IState{TState,TEvent,TArgument}" />, <see cref="State{TState,TEvent,TArgument}" />
  ///   and their usage for implementation details.
  /// </summary>
  internal interface IStateFactory
  {
    /// <summary>
    /// </summary>
    /// <param name="stateConfig"> </param>
    /// <param name="parentState"> </param>
    /// <returns> </returns>
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