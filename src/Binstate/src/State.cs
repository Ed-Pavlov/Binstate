using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Binstate;

internal sealed class State<TState, TEvent, TArgument> : IState<TState, TEvent>, IGetArgument<TArgument>, ISetArgument<TArgument>
  where TState : notnull
  where TEvent : notnull
{
  private readonly object? _enterAction;

  /// <summary>
  ///   This event is used to wait while state's 'enter' action is finished before call 'exit' action and change the active state of the state machine.
  ///   See usages for details.
  /// </summary>
  private readonly ManualResetEvent _enterActionFinished = new ManualResetEvent(true);

  /// <summary>
  ///   This event is used to avoid race condition when <see cref="ExitSafe" /> method is called before <see cref="EnterSafe{T}" /> method.
  ///   See usages for details.
  /// </summary>
  private readonly ManualResetEvent _entered = new ManualResetEvent(true);

  private readonly object? _exitAction;

  private TArgument? _argument;

  private volatile bool _isActive;

  /// <summary>
  ///   This task is used to wait while state's 'enter' action is finished before call 'exit' action and change the active state of the state machine in
  ///   case of async OnEnter action.
  ///   See usages for details.
  /// </summary>
  private Task? _task;

  public State(
    TState                                         id,
    object?                                        enterAction,
    object?                                        exitAction,
    Dictionary<TEvent, Transition<TState, TEvent>> transitions,
    IState<TState, TEvent>?                        parentState)
  {
    Id           = id ?? throw new ArgumentNullException(nameof(id));
    _enterAction = enterAction;
    _exitAction  = exitAction;
    Transitions  = transitions ?? throw new ArgumentNullException(nameof(transitions));
    ParentState  = parentState;
    DepthInTree  = parentState?.DepthInTree + 1 ?? 0;
  }

  public TArgument Argument
  {
    get => _argument ?? throw new InvalidOperationException("Argument is not set");
    set => _argument = value;
  }

  public Dictionary<TEvent, Transition<TState, TEvent>> Transitions { get; }

  public TState Id { get; }

  public int                     DepthInTree { get; }
  public IState<TState, TEvent>? ParentState { get; }

  /// <summary>
  ///   This property is set from protected by lock part of the code so it's no need synchronization
  ///   see <see cref="StateMachine{TState,TEvent}.ActivateStateNotGuarded" /> implementation for details.
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

  public void EnterSafe<TE>(IStateController<TE> stateController, Action<Exception> onException)
  {
    try
    {
      _enterActionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
      _entered.Set();               // it is safe to set the state as entered

      _task = _enterAction switch
      {
        null                                               => null,
        Func<IStateController<TE>, TArgument, Task?> enter => enter(stateController, Argument),
        Func<IStateController<TE>, Task?> enter            => enter(stateController),
        _                                                  => throw new ArgumentOutOfRangeException(),
      };
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
  ///   <see cref="ExitSafe" /> can be called earlier then <see cref="Config{TState,TEvent}.Enter" /> of the activated state,
  ///   see <see cref="StateMachine{TState,TEvent}.PerformTransition" /> implementation for details.
  ///   In this case it should wait till <see cref="Config{TState,TEvent}.Enter" /> will be called and exited, before call exit action
  /// </summary>
  public void ExitSafe(Action<Exception> onException)
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

      switch(_exitAction)
      {
        case null: break;

        case Action action:
          action();
          break;

        case Action<TArgument> actionT:
          actionT(Argument);
          break;

        default: throw new ArgumentOutOfRangeException();
      }
    }
    catch(Exception exception)
    {
      onException(exception);
    }
  }

  public void CallTransitionActionSafe(ITransition transition, Action<Exception> onException) => transition.InvokeActionSafe(Argument, onException);

  // use [NotNullWhen(returnValue: true)] when upgrading to .netstandard 2.1 and update usages
  public bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent>? transition)
  {
    IState<TState, TEvent>? state = this;

    while(state != null)
    {
      if(state.Transitions.TryGetValue(@event, out transition))
        return true;

      state = state.ParentState;
    }

    // no transition found through all parents
    // ReSharper disable once RedundantAssignment // it's a R# fault in fact
    transition = default;

    return false;
  }

  IState? IState.ParentState => ParentState;

  public override string ToString() => Id.ToString();
}