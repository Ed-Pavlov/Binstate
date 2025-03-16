using System;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Binstate;

namespace Binstate.Tests;

public partial class Example
{
  [SuppressMessage("ReSharper", "UnusedParameter.Local")]
  [SuppressMessage("ReSharper", "UnusedMember.Global")]
  public class Elevator
  {
    private readonly IStateMachine<Event> _elevator;

    public Elevator()
    {
      var builder = new Builder<State, Event>(Console.WriteLine);

      builder
       .DefineState(State.Healthy)
       .AddTransition(Event.Error, State.Error);

      builder
       .DefineState(State.Error)
       .AddTransition(Event.Reset, State.Healthy)
       .AllowReentrancy(Event.Error);

      builder
       .DefineState(State.OnFloor)
       .AsSubstateOf(State.Healthy)
       .OnEnter(AnnounceFloor)
       .OnExit(() => Beep(2))
       .AddTransition(Event.CloseDoor, State.DoorClosed)
       .AddTransition(Event.OpenDoor,  State.DoorOpen)
       .AddTransition(Event.GoUp,      State.MovingUp)
       .AddTransition(Event.GoDown,    State.MovingDown);

      builder
       .DefineState(State.Moving)
       .AsSubstateOf(State.Healthy)
       .OnEnter(CheckOverload)
       .AddTransition(Event.Stop, State.OnFloor);

      builder.DefineState(State.MovingUp).AsSubstateOf(State.Moving);
      builder.DefineState(State.MovingDown).AsSubstateOf(State.Moving);

      builder.DefineState(State.DoorClosed).AsSubstateOf(State.OnFloor);
      builder.DefineState(State.DoorOpen).AsSubstateOf(State.OnFloor);

      _elevator = builder.Build(State.OnFloor);

      // ready to work
    }

    public void GoToUpperLevel()
    {
      _elevator.Raise(Event.CloseDoor);
      _elevator.Raise(Event.GoUp);
      _elevator.Raise(Event.OpenDoor);
    }

    public void GoToLowerLevel()
    {
      _elevator.Raise(Event.CloseDoor);
      _elevator.Raise(Event.GoDown);
      _elevator.Raise(Event.OpenDoor);
    }

    public void Error() => _elevator.Raise(Event.Error);

    public void Stop() => _elevator.Raise(Event.Stop);

    public void Reset() => _elevator.Raise(Event.Reset);

    private void AnnounceFloor()
    {
      /* announce floor number */
    }

    private void AnnounceOverload()
    {
      /* announce overload */
    }

    private void Beep(int times)
    {
      /* beep */
    }

    private void CheckOverload(IStateController<Event> stateController)
    {
      if(IsOverloaded())
      {
        AnnounceOverload();
        stateController.RaiseAsync(Event.Stop);
      }
    }

    private bool IsOverloaded() => false;

    private enum State { None, Healthy, OnFloor, Moving, MovingUp, MovingDown, DoorOpen, DoorClosed, Error, }

    private enum Event { GoUp, GoDown, OpenDoor, CloseDoor, Stop, Error, Reset, }
  }
}