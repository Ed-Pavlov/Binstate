namespace Binstate
{
  public static partial class Config<TState, TEvent>
  {
    /// <summary>
    /// This class is used to configure composite states. 
    /// </summary>
    public class State : Enter
    {
      internal TState ParentStateId; 
      
      internal State(TState stateId) : base(stateId)
      { }

      /// <summary>
      /// Defines the currently configured state as a subset of a composite state 
      /// </summary>
      public Enter AsSubstateOf(TState parentStateId)
      {
        ParentStateId = parentStateId;
        return this;
      }
    }
  }
}