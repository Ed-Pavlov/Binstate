namespace Binstate
{
  /// <summary>
  /// This class provides syntax-sugar to configure the state machine.
  /// </summary>
  public static partial class Config<TState, TEvent>
  {
    /// <summary>
    /// There are two types of the state in the system with and w/o parameter in the 'enter' action. In order to make a code type safe and avoid
    /// boxing of value type arguments, complex generic types machinery is introduced. This is the first part of this machinery,
    /// Factories what creates <see cref="State{TState, TEvetn}"/> or <see cref="State{TState, TEvent, TArguments}"/>. Depending on was 'enter' action
    /// with parameter is defined for the state one of these types are instantiated.
    /// See <see cref="IEnterActionInvoker{TEvent,TArgument}"/>, <see cref="IState{TState,TEvent,TArgument}"/>, <see cref="State{TState,TEvent,TArgument}"/>
    /// and their usage for implementation details.
    /// </summary>
    private interface IStateFactory
    {
      State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState);
    }

    private class NoArgumentStateFactory : IStateFactory
    {
      public State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState) => 
        new State<TState, TEvent>(stateConfig.StateId, stateConfig.EnterActionInvoker, null, stateConfig.ExitAction, stateConfig.CreateTransitions(), parentState);
    }

    private class StateFactory<TArgument> : IStateFactory
    {
      public State<TState, TEvent> CreateState(Enter stateConfig, State<TState, TEvent> parentState) => 
        new State<TState, TEvent, TArgument>(stateConfig.StateId, stateConfig.EnterActionInvoker, stateConfig.ExitAction, stateConfig.CreateTransitions(), parentState);
    }
  }
}