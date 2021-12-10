namespace Binstate;

public static partial class Config<TState, TEvent>
{
  /// <summary>
  ///   This class is used to configure composite states.
  /// </summary>
  public interface IState : IEnter
  {
    /// <summary>
    ///   Defines the currently configured state as a substate of a composite state
    /// </summary>
    IEnter AsSubstateOf(TState parentStateId);
  }
}