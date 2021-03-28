using System;

namespace Binstate
{
  public static partial class Config<TState, TEvent>
  {
    /// <summary>
    /// This class is used to configure composite states. 
    /// </summary>
    public class State : Enter
    {
      internal Maybe<TState> ParentStateId = Maybe<TState>.Nothing; 
      
      internal State(TState stateId) : base(stateId)
      { }

      /// <summary>
      /// Defines the currently configured state as a substate of a composite state 
      /// </summary>
      public Enter AsSubstateOf(TState parentStateId)
      {
        if (parentStateId is null) throw new ArgumentNullException(nameof(parentStateId));
        
        ParentStateId = parentStateId.ToMaybe();
        return this;
      }
    }
  }
}