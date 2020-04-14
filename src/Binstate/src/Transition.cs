using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition
  {
    private readonly bool _allowNull;
    [CanBeNull] 
    private readonly Type _argumentType;
    
    public Transition(object @event, Type argumentType, object state, bool allowNull)
    {
      _allowNull = allowNull;
      Event = @event;
      _argumentType = argumentType;
      State = state;
    }

    public object Event { get; }
    
    public object State { get; }

    public void ValidateParameter()
    {
      if(_argumentType != null) throw new TransitionException("Transition is configured as required a parameter");
    }
    
    public void ValidateParameter<T>([CanBeNull] T parameter) 
    {
      if (_argumentType == null) throw new TransitionException("Transition is not configured as accepted any parameter");
      if(!_allowNull && ReferenceEquals(null, parameter)) throw new TransitionException("Transition can't accept null value");
      
      var parameterType = typeof(T);
      if(!_argumentType.IsAssignableFrom(parameterType)) 
        throw new InvalidOperationException($"Parameter type of transition '{_argumentType}' can't accept parameter of type '{parameterType}'");
    }
  }
}