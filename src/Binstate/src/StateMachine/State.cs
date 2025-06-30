using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal sealed partial class State<TState, TEvent, TArgument> : IState<TState, TEvent>, IGetArgument<TArgument>, ISetArgument<TArgument>
  where TState : notnull
  where TEvent : notnull
{
  private readonly Type? _argumentType;

  private readonly EnterAction? _enterAction;

  /// <summary>
  /// This event is used to wait while the state's 'enter' action is finished before call 'exit' action and change the active state of the state machine.
  /// See usages for details.
  /// </summary>
  private readonly ManualResetEvent _enterActionFinished = new ManualResetEvent(true);

  /// <summary>
  /// This event is used to avoid race condition when <see cref="ExitSafe" /> method is called before <see cref="EnterSafe" /> method.
  /// See usages for details.
  /// </summary>
  private readonly ManualResetEvent _enterActionStarted = new ManualResetEvent(true);

  private readonly ExitAction? _exitAction;

  private Maybe<TArgument> _argument;

  private volatile bool _isActive;

  /// <summary>
  /// This task is used to wait while the state's 'enter' action is finished before call 'exit' action and change the active state of the state machine in
  /// case of async OnEnter action.
  /// See usages for details.
  /// </summary>
  private Task? _task;

  public State(
    TState                                                   id,
    EnterAction?                                             enterAction,
    ExitAction?                                                  exitAction,
    IReadOnlyDictionary<TEvent, ITransition<TState, TEvent>> transitions,
    IState<TState, TEvent>?                                  parentState)
  {
    Id           = id ?? throw new ArgumentNullException(nameof(id));
    _enterAction = enterAction;
    _exitAction  = exitAction;
    Transitions  = transitions ?? throw new ArgumentNullException(nameof(transitions));
    ParentState  = parentState;
    DepthInTree  = parentState?.DepthInTree + 1 ?? 0;

    var argumentType = typeof(TArgument);
    _argumentType = argumentType == typeof(Unit) ? null : argumentType;
  }

  public TState Id { get; }

  public int                     DepthInTree { get; }
  public IState<TState, TEvent>? ParentState { get; }

  public TArgument Argument
  {
    get => _argument.HasValue ? _argument.Value : throw new InvalidOperationException("Argument is not set");
    set => _argument = value.ToMaybe();
  }

  public Maybe<object?> GetArgumentAsObject() => _argument.HasValue ? _argument.Value.ToMaybe<object?>() : Maybe<object?>.Nothing;
  public Type?          GetArgumentTypeSafe() => _argumentType;

  public IReadOnlyDictionary<TEvent, ITransition<TState, TEvent>> Transitions { get; }

  public bool IsActive
  {
    get => _isActive;

    set
    {
      if(value)
        _enterActionStarted.Reset();

      _isActive = value;
    }
  }

  public void EnterSafe(IStateController<TEvent> stateController, Action<Exception> onException)
  {
    try
    {
      _enterActionFinished.Reset(); // Exit will wait for this event before call OnExit so after resetting it
      _enterActionStarted.Set();    // it is safe to set the state as entered

      _task = _enterAction?.Call(stateController, _argument);
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

  /// <summary>
  /// <see cref="ExitSafe" /> can be called earlier then <see cref="Builder{TState,TEvent}.ConfiguratorOf.EnterAction" /> of the activated state,
  /// see <see cref="StateMachine{TState,TEvent}.PerformTransition" /> implementation for details.
  /// In this case it should wait till <see cref="Builder{TState,TEvent}.ConfiguratorOf.EnterAction" /> will be called and exited, before call exit action
  /// </summary>
  public void ExitSafe(Action<Exception> onException)
  {
    try
    {
      IsActive = false; // signal that the current state is no more active and blocking enter action can finish

      // if action is set as active but enter action still is not called, wait for it
      _enterActionStarted.WaitOne();

      // wait till the state's 'enter' function finishes
      // if enter action is blocking or no action: _enterFunctionFinished is set means it finishes
      _enterActionFinished.WaitOne();

      // if async: _enterFunctionFinished is set means there is a value assigned to _task, which allows waiting till the action finishes
      _task?.Wait();

      _exitAction?.Call(_argument);
    }
    catch(Exception exception)
    {
      onException(exception);
    }
  }

  public bool FindTransitionTransitive(TEvent @event, [NotNullWhen(returnValue: true)] out ITransition<TState, TEvent>? transition)
  {
    IState<TState, TEvent>? state = this;

    while(state != null)
    {
      if(state.Transitions.TryGetValue(@event, out transition))
        return true;

      state = state.ParentState;
    }

    // no transition found through all parents
    transition = null;
    return false;
  }

  IState? IState.ParentState => ParentState;

  public override string ToString() => Id.ToString();
}