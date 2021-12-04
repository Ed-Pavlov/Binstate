// using System;
// using System.Collections.Generic;
// using System.Transactions;
//
// namespace Binstate;
//
// /// <summary>
// /// This class provides syntax-sugar to configure the state machine.
// /// </summary>
// public static partial class Config<TState, TEvent>
// {
//   public class Transitions<T> : Transitions
//   {
//     public virtual Transitions AddTransition(TEvent @event, TState stateId, Action<T>? action = null)
//     {
//       
//     }
//   }
// }