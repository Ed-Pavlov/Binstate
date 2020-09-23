using System;
using JetBrains.Annotations;

namespace Binstate
{
  internal class Transition<TState, TEvent>
  {
    private readonly Func<TState> _getTargetStateId;
    private readonly bool _allowNullArgument;
    
    public Transition(TEvent @event, Func<TState> getTargetStateId, bool isStatic, Type argumentType, bool allowNullArgument)
    {
      Event = @event;
      _getTargetStateId = getTargetStateId;
      IsStatic = isStatic;
      ArgumentType = argumentType;
      _allowNullArgument = allowNullArgument;
    }

    public TEvent Event { get; }
    
    [CanBeNull] 
    public readonly Type ArgumentType;
    
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
      if(ArgumentType != null) throw new TransitionException("Transition is configured as required an argument");
    }
    
    public void ValidateParameter<T>([CanBeNull] T parameter) 
    {
      if (ArgumentType == null) throw new TransitionException("Transition is not configured as accepted an argument");
      if(!_allowNullArgument && ReferenceEquals(null, parameter)) throw new TransitionException("Transition can't accept null value");
      
      var argumentType = typeof(T);
      if(!ArgumentType.IsAssignableFrom(argumentType)) 
        throw new InvalidOperationException($"Parameter type '{ArgumentType}' of transition can't accept argument of type '{argumentType}'");
    }
  }
}