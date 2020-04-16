using System.Diagnostics.CodeAnalysis;
using Binstate;

namespace Instate.Tests.example
{
  public partial class Example
  {
    // uncompleted
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Elevator
    {
      private readonly StateMachine<States, Events> _elevator;

      private enum States
      {
        Healthy,
        OnFloor,
        Moving,
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

      public Elevator()
      {
        var builder = new Builder<States, Events>();

        builder
          .AddState(States.Healthy)
          .AddTransition(Events.Error, States.Error);

        builder
          .AddState(States.Error)
          .AddTransition(Events.Reset, States.Healthy);

        builder
          .AddState(States.OnFloor)
          .OnEnter(AnnounceFloor)
          .OnExit(() => Beep(2))
          .AddTransition(Events.CloseDoor, States.DoorClosed)
          .AddTransition(Events.OpenDoor, States.DoorOpen)
          .AddTransition(Events.GoUp, States.Moving);

        builder
          .AddState(States.Moving)
          .OnEnter(CheckOverload);

        builder
          .AddState(States.Moving)
          .AddTransition(Events.Stop, States.OnFloor);

        _elevator = builder.Build(States.OnFloor);
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

      private void AnnounceFloor(IStateMachine<Events> stateMachine)
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
    }
  }
}