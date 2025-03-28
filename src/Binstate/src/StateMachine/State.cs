using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal sealed class State<TState, TEvent, TArgument> : IState<TState, TEvent>, IGetArgument<TArgument>, ISetArgument<TArgument>
  where TState : notnull
  where TEvent : notnull
{
  private readonly Type? _argumentType;

  private readonly object? _enterAction;

  /// <summary>
  /// This event is used to wait while state's 'enter' action is finished before call 'exit' action and change the active state of the state machine.
  /// See usages for details.
  /// </summary>
  private readonly ManualResetEvent _enterActionFinished = new ManualResetEvent(true);

  /// <summary>
  /// This event is used to avoid race condition when <see cref="ExitSafe" /> method is called before <see cref="EnterSafe{T}" /> method.
  /// See usages for details.
  /// </summary>
  private readonly ManualResetEvent _enterActionStarted = new ManualResetEvent(true);

  private readonly object? _exitAction;

  private Maybe<TArgument> _argument;

  private volatile bool _isActive;

  /// <summary>
  /// This task is used to wait while state's 'enter' action is finished before call 'exit' action and change the active state of the state machine in
  /// case of async OnEnter action.
  /// See usages for details.
  /// </summary>
  private Task? _task;

  public State(
    TState                                                  id,
    object?                                                 enterAction,
    object?                                                 exitAction,
    IReadOnlyDictionary<TEvent, Transition<TState, TEvent>> transitions,
    IState<TState, TEvent>?                                 parentState)
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

  public IReadOnlyDictionary<TEvent, Transition<TState, TEvent>> Transitions { get; }

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

  public void EnterSafe<TE>(IStateController<TE> stateController, Action<Exception> onException)
  {
    try
    {
      _enterActionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
      _enterActionStarted.Set();    // it is safe to set the state as entered

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

  public void CallTransitionActionSafe(ITransition transition, Action<Exception> onException)
  {
    try
    {
      switch(transition.OnTransitionAction)
      {
        case null: break; // no action

        case Action action:
          action();
          break;

        case Action<TArgument> actionT: // action requires argument
          actionT(Argument);
          break;

        default: throw new ArgumentOutOfRangeException();
      }
    }
    catch(Exception exc)
    { // transition action can throw "user" exception
      onException(exc);
    }
  }

  public bool FindTransitionTransitive(TEvent @event, [NotNullWhen(returnValue: true)] out Transition<TState, TEvent>? transition)
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