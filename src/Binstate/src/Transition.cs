using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    private readonly Func<TState> _getTargetStateId;
    
    public Transition(TEvent @event, Func<TState> getTargetStateId, bool isStatic)
    {
      Event = @event;
      _getTargetStateId = getTargetStateId;
      IsStatic = isStatic;
    }

    public TEvent Event { get; }
    
    [CanBeNull] 
    public readonly Type ArgumentType;
    
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