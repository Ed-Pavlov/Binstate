namespace Binstate;

/// <summary>
/// Possible modes of transferring arguments during state transition from the currently active states to the newly activated.
/// When transition is performed the state machine looks up for a required argument in the following order:
///  * Not fallback argument passed to the <see cref="IStateMachine{TEvent}.Raise{T}"/> (or overload) method
///  * Active state and all its parents
///  * Fallback argument passed to the <see cref="IStateMachine{TEvent}.Raise{T}"/> (or overload) method
/// </summary>
public enum ArgumentTransferMode
{
  /// <summary> default value is invalid </summary>
  Invalid = 0,

  /// <summary>
  /// All actions performed on 'enter', 'exit', and/or 'transition' of a state involved in child/parent relation should have parameter of the same type
  /// in the declared method used as the action. Also, it's possible that some states require an argument but some not.
  /// </summary>
  Strict = 2,

  /// <summary>
  ///  Each state can have its own argument type.
  ///
  ///  If an argument type of the currently active state is <see cref="ITuple{TX,TY}" /> all newly activated actions
  ///  require arguments of type <see cref="ITuple{TX,TY}" />, TX, and TY will receive corresponding argument.
  ///
  ///  If a newly activated state requires an argument of type <see cref="ITuple{TX,TY}" /> it is mixed from the arguments,
  ///  see <see cref="ArgumentTransferMode"/> for details.
  /// </summary>
  Free = 4,
}