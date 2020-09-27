using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    private readonly Func<TState> _getTargetStateId;
    [CanBeNull]
    private readonly Action _action;

    public Transition(TEvent @event, Func<TState> getTargetStateId, bool isStatic, [CanBeNull] Action action)
    {
      Event = @event;
      _getTargetStateId = getTargetStateId;
      IsStatic = isStatic;
      _action = action;
    }
    
    /// <summary>
    /// Means a transition targets the predefined state in opposite to the calculated dynamically runtime
    /// </summary>
    public readonly bool IsStatic;
    
    public TEvent Event { get; }

    public bool GetTargetStateId(out TState state)
    {
      state = _getTargetStateId();
      return state.IsNotNull();
    }

    public void InvokeActionSafe(Action<Exception> onException)
    {
      try {
        _action?.Invoke();
      }
      catch (Exception exc) {  // transition action can throw "user" exception
        onException(exc);
      }
    }

    public override string ToString() => $"[{Event} -> {(IsStatic ? _getTargetStateId().ToString() : "dynamic")}]";
  }
}