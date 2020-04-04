using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This interface is used in entering actions to control execution and auto transitions 
  /// </summary>
  public interface IStateMachine
  {
    /// <summary>
    /// Returns true if the state machine is in the state for which currently executing entering action is defined.  
    /// </summary>
    bool InMyState { get; }
    
    /// <summary>
    /// Passing the event to the state machine
    /// </summary>
    Task RaiseAsync([NotNull] object @event);
    
    /// <summary>
    /// Passing the event with parameter to the state machine. Parameter is needed if the transition target state requires one.
    /// See <see cref="Config.Entering.OnEnter{T}(System.Action{IStateMachine, T})"/>,
    /// <see cref="Config.Entering.OnEnter{T}(System.Func{IStateMachine, T, Task})"/>,
    /// and <see cref="Config.Transitions.AddTransition{TParameter}"/> for details. 
    /// </summary>
    Task RaiseAsync<T>([NotNull] object @event, [NotNull] T parameter);
  }
}