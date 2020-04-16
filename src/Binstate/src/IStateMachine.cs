using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This interface is used in enter actions to control execution and auto transitions 
  /// </summary>
  public interface IStateMachine<TEvent>
  {
    /// <summary>
    /// Returns true if the state machine is in the state for which currently executing enter action is defined.  
    /// </summary>
    bool InMyState { get; }
    
    /// <summary>
    /// Passing the event to the state machine asynchronously.
    /// </summary>
    void RaiseAsync([NotNull] TEvent @event);
    
    /// <summary>
    /// Passing the event with parameter to the state machine asynchronously. Parameter is needed if the Enter action of the target state requires one.
    /// See <see cref="Config.Enter.OnEnter{T}(System.Action{IStateMachine, T})"/>,
    /// <see cref="Config.Enter.OnEnter{T}(System.Func{IStateMachine, T, Task})"/>,
    /// and <see cref="Config.Transitions.AddTransition{TParameter}"/> for details. 
    /// </summary>
    void RaiseAsync<T>([NotNull] TEvent @event, [NotNull] T parameter);
  }
}