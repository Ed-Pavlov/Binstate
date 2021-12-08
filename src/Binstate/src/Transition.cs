using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate;

internal interface ITransition
{
  void InvokeActionSafe<T>(T argument, Action<Exception> onException);
}

internal class Transition<TState, TEvent> : ITransition
{
  private readonly object? _action;

  public readonly GetState<TState> GetTargetStateId;

  /// <summary>
  ///   Means a transition targets the predefined state in opposite to the calculated dynamically runtime
  /// </summary>
  public readonly bool IsStatic;

  public Transition(TEvent @event, GetState<TState> getTargetStateId, bool isStatic, object? action)
  {
    Event            = @event;
    GetTargetStateId = getTargetStateId;
    IsStatic         = isStatic;
    _action          = action;
  }

  public TEvent Event { get; }

  public void InvokeActionSafe<T>(T argument, Action<Exception> onException)
  {
    if(_action is null) return;
    try
    {
      if(_action is Action<T> actionT) // TODO: what if T is Unit?
        actionT(argument);
      else if(_action is Action action)
        action();
      else
        throw new ArgumentOutOfRangeException();
    }
    catch(Exception exc)
    { // transition action can throw "user" exception
      onException(exc);
    }
  }

  [ExcludeFromCodeCoverage]
  public override string ToString()
  {
    var stateName = "dynamic";

    if(IsStatic)
    {
      GetTargetStateId(out var state);
      stateName = state!.ToString();
    }

    return $"[{Event} -> {stateName}]";
  }
}