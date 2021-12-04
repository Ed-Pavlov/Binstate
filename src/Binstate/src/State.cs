using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Binstate;

internal class State<TState, TEvent> where TState : notnull where TEvent : notnull
{
  private readonly IEnterActionInvoker? _enterAction;
  private readonly IActionInvoker?      _exitAction;

  private readonly Dictionary<TEvent, Transition<TState, TEvent>> _transitions;

  /// <summary>
  /// This event is used to avoid race condition when <see cref="ExitSafe"/> method is called before <see cref="EnterSafe"/> method.
  /// See usages for details.
  /// </summary>
  private readonly ManualResetEvent _entered = new(true);

  /// <summary>
  /// This event is used to wait while state's 'enter' action is finished before call 'exit' action and change the active state of the state machine.
  /// See usages for details. 
  /// </summary>
  private readonly ManualResetEvent _enterActionFinished = new(true);

  /// <summary>
  /// This task is used to wait while state's 'enter' action is finished before call 'exit' action and change the active state of the state machine in
  /// case of async OnEnter action.
  /// See usages for details. 
  /// </summary>
  private Task? _task;

  private volatile bool _isActive;

  public State(
    TState                                         id,
    IEnterActionInvoker?                           enterAction,
    Type?                                          enterArgumentType,
    IActionInvoker?                                exitAction,
    Dictionary<TEvent, Transition<TState, TEvent>> transitions,
    State<TState, TEvent>?                         parentState)
  {
    Id                = id ?? throw new ArgumentNullException(nameof(id));
    _enterAction      = enterAction;
    EnterArgumentType = enterArgumentType;
    _exitAction       = exitAction;
    _transitions      = transitions ?? throw new ArgumentNullException(nameof(transitions));
    ParentState       = parentState;
    DepthInTree       = parentState?.DepthInTree + 1 ?? 0;
  }

  public readonly TState Id;
  public readonly Type?  EnterArgumentType;

  public readonly int                    DepthInTree;
  public readonly State<TState, TEvent>? ParentState;

  /// <summary>
  /// This property is set from protected by lock part of the code so it's no need synchronization
  /// see <see cref="StateMachine{TState,TEvent}.ActivateStateNotGuarded{TArgument,TRelay}"/> implementation for details.
  /// </summary>
  public bool IsActive
  {
    get => _isActive;

    set
    {
      if(value) _entered.Reset();
      _isActive = value;
    }
  }

  public void EnterSafe(IStateMachine<TEvent> stateMachine, Action<Exception> onException)
    => Enter(
      onException,
      enter =>
      {
        var noParameterEnter = (NoParameterEnterActionActionInvoker<TEvent>)enter;

        return noParameterEnter.Invoke(stateMachine);
      });

  /// <summary>
  /// <see cref="ExitSafe"/> can be called earlier then <see cref="Config{TState,TEvent}.Enter"/> of the activated state,
  /// see <see cref="StateMachine{TState,TEvent}.PerformTransition{TArgument, TRelay}"/> implementation for details.
  /// In this case it should wait till <see cref="Config{TState,TEvent}.Enter"/> will be called and exited, before call exit action
  /// </summary>
  public virtual void ExitSafe(Action<Exception> onException)
    => Exit(
      onException,
      exit =>
      {
        var noParameterExit = (ActionInvoker)exit;
        noParameterExit.Invoke();
      });

  public virtual void CallTransitionActionSafe(Transition<TState, TEvent> transition, Action<Exception> onException) => transition.InvokeActionSafe(onException);

  protected void Enter(Action<Exception> onException, Func<IEnterActionInvoker, Task?> invokeEnterAction)
  {
    try
    {
      _enterActionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
      _entered.Set();               // it is safe to set the state as entered

      if(_enterAction is null) return;
      _task = invokeEnterAction(_enterAction);
    }
    catch(Exception exception)
    {
      onException(exception);
    }
    finally
    {
      _enterActionFinished.Set();
    }
  }

  protected void Exit(Action<Exception> onException, Action<IActionInvoker> invokeExitAction)
  {
    try
    {
      IsActive = false; // signal that current state is no more active and blocking enter action can finish

      // if action is set as active but enter action still is not called, wait for it 
      _entered.WaitOne();

      // wait till State.Enter function finishes
      // if enter action is blocking or no action: _enterFunctionFinished is set means it finishes
      _enterActionFinished.WaitOne();

      // if async: _enterFunctionFinished is set means there is a value assigned to _task, which allows waiting till action finishes
      _task?.Wait();

      if(_exitAction is null) return;
      invokeExitAction(_exitAction);
    }
    catch(Exception exception)
    {
      onException(exception);
    }
  }

  // use [NotNullWhen(returnValue: true)] when upgrading to .netstandard 2.1 and update usages
  public bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent>? transition)
  {
    var state = this;

    while(state != null)
    {
      if(state._transitions.TryGetValue(@event, out transition))
        return true;

      state = state.ParentState;
    }

    // no transition found through all parents
    // ReSharper disable once RedundantAssignment // it's a R# fault in fact
    transition = default;

    return false;
  }

  public override string ToString() => Id.ToString();
}