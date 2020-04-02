using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition
  {
    private readonly bool _allowNull;
    [CanBeNull] 
    private readonly Type _argumentType;
    
    public Transition(object trigger, Type argumentType, object state, bool allowNull)
    {
      _allowNull = allowNull;
      Trigger = trigger;
      _argumentType = argumentType;
      State = state;
    }

    public object Trigger { get; }
    
    public object State { get; }

    public void ValidateParameter()
    {
      if(_argumentType != null) throw new InvalidOperationException("Transition is configured as required a parameter");
    }
    
    public void ValidateParameter<T>([CanBeNull] T parameter) 
    {
      if (_argumentType == null) throw new InvalidOperationException("Transition is not configured as accepted any parameter");
      if(!_allowNull && ReferenceEquals(null, parameter)) throw new InvalidOperationException("Transition can't accept null value");
      
      var parameterType = typeof(T);
      if(!_argumentType.IsAssignableFrom(parameterType)) 
        throw new InvalidOperationException($"Parameter type of transition '{_argumentType}' can't accept parameter of type '{parameterType}'");
    }
  }
}