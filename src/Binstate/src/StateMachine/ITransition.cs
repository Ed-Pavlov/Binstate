namespace BeatyBit.Binstate;

internal interface ITransition
{
  object? TransitionAction { get; }
  bool    IsReentrant      { get; }
}