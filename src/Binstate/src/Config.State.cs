using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public static partial class Config<TState, TEvent>
  {
    /// <summary>
    /// This class is used to configure composite states. 
    /// </summary>
    public class Substate : Enter
    {
      internal TState ParentStateId; 
      
      internal Substate(TState stateId) : base(stateId)
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