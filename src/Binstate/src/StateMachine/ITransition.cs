namespace BeatyBit.Binstate;

internal interface ITransition
{
  object? OnTransitionAction { get; }
}