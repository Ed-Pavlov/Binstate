using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    private readonly Func<TState> _getTargetStateId;
    private readonly bool _allowNullArgument;
    [CanBeNull] 
    private readonly Type _argumentType;

    public Transition(TEvent @event, Func<TState> getTargetStateId, bool isStatic, Type argumentType, bool allowNullArgument)
    {
      Event = @event;
      _getTargetStateId = getTargetStateId;
      IsStatic = isStatic;
      _argumentType = argumentType;
      _allowNullArgument = allowNullArgument;
    }

    public TEvent Event { get; }
    
    /// <summary>
    /// Means transition targets the predefined state in opposite to calculated dynamically runtime
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
        return default;
      }
    }

    public void ValidateParameter()
    {
      if(_argumentType != null) throw new TransitionException("Transition is configured as required an argument");
    }
    
    public void ValidateParameter<T>([CanBeNull] T parameter) 
    {
      if (_argumentType == null) throw new TransitionException("Transition is not configured as accepted an argument");
      if(!_allowNullArgument && ReferenceEquals(null, parameter)) throw new TransitionException("Transition can't accept null value");
      
      var argumentType = typeof(T);
      if(!_argumentType.IsAssignableFrom(argumentType)) 
        throw new InvalidOperationException($"Parameter type '{_argumentType}' of transition can't accept argument of type '{argumentType}'");
    }
  }
}