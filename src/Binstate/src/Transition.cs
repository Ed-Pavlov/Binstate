using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    private readonly bool _allowNullArgument;
    [CanBeNull] 
    private readonly Type _argumentType;

    public Transition(TEvent @event, Type argumentType, Func<TState> getTargetStateId, bool isStatic, bool allowNullArgument)
    {
      _allowNullArgument = allowNullArgument;
      Event = @event;
      _argumentType = argumentType;
      IsStatic = isStatic;
      GetTargetStateId = getTargetStateId;
    }

    public TEvent Event { get; }
    
    public readonly bool IsStatic;
    public Func<TState> GetTargetStateId { get; }

    public void ValidateParameter()
    {
      if(_argumentType != null) throw new TransitionException("Transition is configured as required a parameter");
    }
    
    public void ValidateParameter<T>([CanBeNull] T parameter) 
    {
      if (_argumentType == null) throw new TransitionException("Transition is not configured as accepted any parameter");
      if(!_allowNullArgument && ReferenceEquals(null, parameter)) throw new TransitionException("Transition can't accept null value");
      
      var parameterType = typeof(T);
      if(!_argumentType.IsAssignableFrom(parameterType)) 
        throw new InvalidOperationException($"Parameter type of transition '{_argumentType}' can't accept parameter of type '{parameterType}'");
    }
  }
}