using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate
{
  public class Transition
  {
    private readonly bool _allowNull;

    public Transition(object trigger, Type? argumentType, object state, bool allowNull)
    {
      _allowNull = allowNull;
      Trigger = trigger;
      ArgumentType = argumentType;
      State = state;
    }

    public object Trigger { get; }
    public Type? ArgumentType { get; }
    public object State { get; }

    public void ValidateParameter()
    {
      if(ArgumentType != null) throw new InvalidOperationException("Transition is configured as required a parameter");
    }
    
    public void ValidateParameter<T>(T parameter) 
    {
      if (ArgumentType == null) throw new InvalidOperationException("Transition is not configured as accepted any parameter");
      if(!_allowNull && ReferenceEquals(null, parameter)) throw new InvalidOperationException("Transition can't accept null value");
      
      var parameterType = typeof(T);
      if(!ArgumentType.IsAssignableFrom(parameterType)) 
        throw new InvalidOperationException($"Parameter type of transition '{ArgumentType}' can't accept parameter of type '{parameterType}'");
      
    }
  }
}