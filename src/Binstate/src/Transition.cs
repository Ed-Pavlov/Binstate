using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate;

/// <summary>
/// </summary>
/// <typeparam name="TState"> </typeparam>
/// <typeparam name="TEvent"> </typeparam>
public class Transition<TState, TEvent>
{
  private readonly object? _action;

  /// <summary>
  /// </summary>
  public readonly GetState<TState> GetTargetStateId;

  /// <summary>
  ///   Means a transition targets the predefined state in opposite to the calculated dynamically runtime
  /// </summary>
  public readonly bool IsStatic;

  /// <summary>
  /// </summary>
  /// <param name="event"> </param>
  /// <param name="getTargetStateId"> </param>
  /// <param name="isStatic"> </param>
  /// <param name="action"> </param>
  public Transition(TEvent @event, GetState<TState> getTargetStateId, bool isStatic, object? action)
  {
    Event            = @event;
    GetTargetStateId = getTargetStateId;
    IsStatic         = isStatic;
    _action          = action;
  }

  /// <summary>
  /// </summary>
  public TEvent Event { get; }

  /// <summary>
  /// </summary>
  /// <param name="argument"> </param>
  /// <param name="onException"> </param>
  /// <typeparam name="T"> </typeparam>
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

  /// <inheritdoc />
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