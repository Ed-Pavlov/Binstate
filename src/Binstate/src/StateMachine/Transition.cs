using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

internal class Transition<TState, TEvent> : ITransition
{
  public Transition(TEvent @event, GetState<TState> getTargetStateId, bool isStatic, object? transitionAction)
  {
    Event            = @event;
    GetTargetStateId = getTargetStateId;
    IsStatic         = isStatic;
    TransitionAction = transitionAction;
  }

  public TEvent Event { get; }

  /// <summary>
  /// Means a transition targets the predefined state in opposite to the calculated dynamically runtime
  /// </summary>
  public readonly bool IsStatic;

  public readonly GetState<TState> GetTargetStateId;

  public object? TransitionAction { get; }

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