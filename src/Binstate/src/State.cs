using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  /// <summary>
  /// This interface is used to make <typeparamref name="TArgument"/> contravariant.
  /// </summary>
  internal interface IState<TState, out TEvent, in TArgument>
  {
    void EnterSafe(IStateMachine<TEvent> stateMachine, TArgument argument, Action<Exception> onException);
  }

  /// <summary>
  /// This class describes state machine's state which requires an argument to the enter action.
  ///
  /// All these complex generic stuff is introduced to avoid casting to 'object' and thus avoid boxing when value type instance is used as the argument.
  /// </summary>
  internal class State<TState, TEvent, TArgument> : State<TState, TEvent>, IState<TState, TEvent, TArgument>
  {
    public TArgument Argument;

    public State(
      [NotNull] TState id,
      [CanBeNull] IEnterInvoker<TEvent> enter,
      [CanBeNull] Action exit,
      [NotNull] Dictionary<TEvent, Transition<TState, TEvent>> transitions,
      [CanBeNull] State<TState, TEvent> parentState) : base(id, enter, typeof(TArgument), exit, transitions, parentState)
    {
    }

    public void EnterSafe(IStateMachine<TEvent> stateMachine, TArgument argument, Action<Exception> onException)
    {
      Enter(onException, enter =>
        {
          Argument = argument;
          var typedEnter = (IEnterActionInvoker<TEvent, TArgument>) enter;
          return typedEnter.Invoke(stateMachine, argument);
        });
    }
  }
  
  internal class State<TState, TEvent>
  {
    [CanBeNull]
    private readonly IEnterInvoker<TEvent> _enter;
    [CanBeNull]
    private readonly Action _exit;

    private readonly Dictionary<TEvent, Transition<TState, TEvent>> _transitions;

    /// <summary>
    /// This event is used to avoid race condition when <see cref="ExitSafe"/> method is called before <see cref="Config{TState,TEvent}.Enter"/> method.
    /// See usages for details.
    /// </summary>
    private readonly ManualResetEvent _entered = new ManualResetEvent(true);

    /// <summary>
    /// This event is used to wait while state's OnEnter action is finished before call OnExit action and change the active state of the state machine.
    /// See usages for details. 
    /// </summary>
    private readonly ManualResetEvent _enterFunctionFinished = new ManualResetEvent(true);

    /// <summary>
    /// This task is used to wait while state's OnEnter action is finished before call OnExit action and change the active state of the state machine in
    /// case of async OnEnter action.
    /// See usages for details. 
    /// </summary>
    [CanBeNull]
    private Task _task;

    private volatile bool _isActive;

    public State(
      TState id,
      IEnterInvoker<TEvent> enter,
      Type enterArgumentType,
      Action exit,
      Dictionary<TEvent, Transition<TState, TEvent>> transitions,
      State<TState, TEvent> parentState)
    {
      Id = id;
      _enter = enter;
      EnterArgumentType = enterArgumentType;
      _exit = exit;
      _transitions = transitions;
      ParentState = parentState;
      DepthInTree = parentState?.DepthInTree + 1 ?? 0;
    }

    public readonly TState Id;
    [CanBeNull]
    public readonly Type EnterArgumentType;

    public readonly int DepthInTree;
    public readonly State<TState, TEvent> ParentState;

    public void EnterSafe(IStateMachine<TEvent> stateMachine, Action<Exception> onException)
    {
      Enter(onException, enter =>
        {
          var noParameterEnter = (NoParameterEnterActionInvoker<TEvent>) enter;
          return noParameterEnter.Invoke(stateMachine);
        });
    }

    protected void Enter(Action<Exception> onException, Func<IEnterInvoker<TEvent>, Task> invokeEnterAction)
    {
      try
      {
        _enterFunctionFinished.Reset(); // Exit will wait this event before call OnExit so after resetting it
        _entered.Set(); // it is safe to set the state as entered

        if (_enter == null) return;
        _task = invokeEnterAction(_enter);
      }
      catch (Exception exception)
      {
        onException(exception);
      }
      finally
      {
        _enterFunctionFinished.Set();
      }
    }

    public bool FindTransitionTransitive(TEvent @event, out Transition<TState, TEvent> transition)
    {
      var state = this;
      while (state != null)
      {
        if (state._transitions.TryGetValue(@event, out transition))
          return true;

        state = state.ParentState;
      }

      // no transition found through all parents
      transition = default;
      return false;
    }

    /// <summary>
    /// This property is set from protected by lock part of the code so it's no need synchronization
    /// see <see cref="StateMachine{TState,TEvent}.PerformTransition{T}"/> implementation for details.
    /// </summary>
    public bool IsActive
    {
      get => _isActive;
      set
      {
        if (value) _entered.Reset();
        _isActive = value;
      }
    }

    /// <summary>
    /// <see cref="ExitSafe"/> can be called earlier then <see cref="Config{TState,TEvent}.Enter"/> of the activated state,
    /// see <see cref="StateMachine{TState,TEvent}.PerformTransition{T}"/> implementation for details.
    /// In this case it should wait till <see cref="Config{TState,TEvent}.Enter"/> will be called and exited, before call exit action
    /// </summary>
    public void ExitSafe(Action<Exception> onException)
    {
      try
      {
        IsActive = false; // signal that current state is no more active

        // if action is set as active but enter action still is not called, wait for it 
        _entered.WaitOne();

        // wait till State.Enter function finishes
        // if enter action is blocking or no action: _enterFunctionFinished is set means it finishes
        _enterFunctionFinished.WaitOne(Timeout.Infinite);
        // if async: _enterFunctionFinished is set means there is a value assigned to _task, which allows waiting till action finishes
        _task?.Wait(Timeout.Infinite);

        _exit?.Invoke();
      }
      catch (Exception exception)
      {
        onException(exception);
      }
    }

    public IReadOnlyCollection<State<TState, TEvent>> GetAllStatesForActivationTillParent([CanBeNull] State<TState, TEvent> tillState)
    {
      var states = new List<State<TState, TEvent>>();
      var parent = ParentState;
      while (parent != null && !ReferenceEquals(parent, tillState))
      {
        states.Add(parent);
        parent = parent.ParentState;
      }

      states.Reverse();
      states.Add(this);
      return states;
    }

    public override string ToString() => Id.ToString();
  }
}