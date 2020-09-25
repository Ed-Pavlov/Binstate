using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    private readonly Func<TState> _getTargetStateId;
    [CanBeNull]
    public readonly Action _action;

    public Transition(TEvent @event, Func<TState> getTargetStateId, bool isStatic, [CanBeNull] Action action)
    {
      Event = @event;
      _getTargetStateId = getTargetStateId;
      IsStatic = isStatic;
      _action = action;
    }

    public TEvent Event { get; }

    public void InvokeAction(Action<Exception> onException)
    {
      try
      {
        _action?.Invoke();
      }
      catch (Exception exc)
      {
        onException(exc);
        throw;
      }
    }

    /// <summary>
    /// Means a transition targets the predefined state in opposite to the calculated dynamically runtime
    /// </summary>
    public readonly bool IsStatic;

    public TState GetTargetStateId(Action<Exception> onException)
    {
      try
      {
        return _getTargetStateId();
      }
      catch (Exception exception)
      {
        onException(exception);
        throw;
      }
    }
  }
}