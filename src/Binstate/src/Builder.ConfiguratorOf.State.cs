using System;

namespace Binstate;

public partial class Builder<TState, TEvent>
{
  public static partial class ConfiguratorOf
  {
    /// <summary>
    /// This interface allows you to define composite states, allowing a state to be designated as a substate of another.
    /// </summary>
    public interface IState : IEnterAction
    {
      /// <summary>
      /// Designates the currently configured state as a substate of the specified parent state.
      /// </summary>
      /// <param name="parentStateId">The ID of the parent state.</param>
      /// <returns>An <see cref="IEnterAction"/> instance for further configuration.</returns>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentStateId"/> is null.</exception>
      IEnterAction AsSubstateOf(TState parentStateId);
    }

    internal class State : EnterAction, IState
    {
      internal State(StateData stateData) : base(stateData) { }

      public IEnterAction AsSubstateOf(TState parentStateId)
      {
        if(parentStateId is null) throw new ArgumentNullException(nameof(parentStateId));
        StateData.ParentStateId = parentStateId.ToMaybe();
        return this;
      }
    }
  }
}