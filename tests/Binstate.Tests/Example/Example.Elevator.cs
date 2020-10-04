using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate.Tests
{
  public partial class Example
  {
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Elevator
    {
      private readonly StateMachine<States, Events> _elevator;

      public Elevator()
      {
        var builder = new Builder<States, Events>(Console.WriteLine);

        builder
          .DefineState(States.Healthy)
          .AddTransition(Events.Error, States.Error);

        builder
          .DefineState(States.Error)
          .AddTransition(Events.Reset, States.Healthy)
          .AllowReentrancy(Events.Error);

        builder
          .DefineState(States.OnFloor).AsSubstateOf(States.Healthy)
          .OnEnter(AnnounceFloor)
          .OnExit(() => Beep(2))
          .AddTransition(Events.CloseDoor, States.DoorClosed)
          .AddTransition(Events.OpenDoor, States.DoorOpen)
          .AddTransition(Events.GoUp, States.MovingUp)
          .AddTransition(Events.GoDown, States.MovingDown);

        builder
          .DefineState(States.Moving).AsSubstateOf(States.Healthy)
          .OnEnter(CheckOverload)
          .AddTransition(Events.Stop, States.OnFloor);

        builder.DefineState(States.MovingUp).AsSubstateOf(States.Moving);
        builder.DefineState(States.MovingDown).AsSubstateOf(States.Moving);

        builder.DefineState(States.DoorClosed).AsSubstateOf(States.OnFloor);
        builder.DefineState(States.DoorOpen).AsSubstateOf(States.OnFloor);
        
        _elevator = builder.Build(States.OnFloor);
        
        // ready to work
      }

      public void GoToUpperLevel()
      {
        _elevator.Raise(Events.CloseDoor);
        _elevator.Raise(Events.GoUp);
        _elevator.Raise(Events.OpenDoor);
      }

      public void GoToLowerLevel()
      {
        _elevator.Raise(Events.CloseDoor);
        _elevator.Raise(Events.GoDown);
        _elevator.Raise(Events.OpenDoor);
      }

      public void Error()
      {
        _elevator.Raise(Events.Error);
      }

      public void Stop()
      {
        _elevator.Raise(Events.Stop);
      }

      public void Reset()
      {
        _elevator.Raise(Events.Reset);
      }

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

      private void CheckOverload(IStateMachine<Events> stateMachine)
      {
        if (IsOverloaded())
        {
          AnnounceOverload();
          stateMachine.RaiseAsync(Events.Stop);
        }
      }

      private bool IsOverloaded() => false;
      
      private enum States
      {
        None,
        Healthy,
        OnFloor,
        Moving,
        MovingUp,
        MovingDown,
        DoorOpen,
        DoorClosed,
        Error
      }

      private enum Events
      {
        GoUp,
        GoDown,
        OpenDoor,
        CloseDoor,
        Stop,
        Error,
        Reset
      }
    }
  }
}