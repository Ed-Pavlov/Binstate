using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    [CanBeNull]
    private readonly Action _action;

    public Transition(TEvent @event, GetState<TState> getTargetStateId, bool isStatic, [CanBeNull] Action action)
    {
      Event = @event;
      GetTargetStateId = getTargetStateId;
      IsStatic = isStatic;
      _action = action;
    }
    
    /// <summary>
    /// Means a transition targets the predefined state in opposite to the calculated dynamically runtime
    /// </summary>
    public readonly bool IsStatic;
    
    public TEvent Event { get; }

    public readonly GetState<TState> GetTargetStateId;
    
    public void InvokeActionSafe(Action<Exception> onException)
    {
      try {
        _action?.Invoke();
      }
      catch (Exception exc) {  // transition action can throw "user" exception
        onException(exc);
      }
    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
      var stateName = "dynamic";
      if (IsStatic)
      {
        GetTargetStateId(out var state);
        stateName = state.ToString();
      }
      return $"[{Event} -> {stateName}]";
    }
  }
}