<p align='right'>If <b>Binstate</b> has done you any good, consider supporting my future initiatives</p>
<p align="right">
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=ed@pavlov.is&lc=US&item_name=Kudos+for+Binstate&no_note=0&cn=&currency_code=EUR">
    <img src="/.build/button.png" width="76" height="32">
  </a>
</p>

___

# Binstate

<p align="center">
  <img src="/.build/icon.png" width="86" height="86">
</p>

**Binstate** *(pronounced as "Be in state")* is a simple but yet powerful thread-safe, hierarchical state machine for .NET.
Features include support for async methods, argument passing, state serialization, and more.

[![Build & Test](https://github.com/Ed-Pavlov/Binstate/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/Ed-Pavlov/Binstate/actions/workflows/build-and-test.yml)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/Ed-Pavlov/294bcfc592339fa417166638864b77ce/raw/binstate-test-coverage.json)
[![Nuget](https://img.shields.io/nuget/dt/BeatyBit.Binstate)](https://www.nuget.org/packages/BeatyBit.Binstate/)

___

## Powered by
<p align="right">
  <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider.png" width="185" height="64">
</p>

___

## Features

### Thread safety

The state machine is fully thread safe and allows calling any method from any thread.

### Control on what thread enter and exit actions are executed

**Binstate** doesn't use its own thread to execute actions.
It gives an application full control over the threading model of the state machine, and only you decide will it be a passive or an active state machine.

`Raise(event)`
executes an 'exit' action of the current state and an 'enter' action of the new state on the current thread.
If an 'enter' action is blocking `Raise` will block until enter action finishes.
`RaiseAsync(event)` uses `Task.Run` to execute a transition.

### Support async methods

Supports async methods as an 'enter' action of the state. Binstate guarantees that an async 'enter' action will finish before calling an 'exit'
 action of the current state and an 'enter' action of the new state. An async method should return `Task`, `async void` methods aren’t supported.

    .OnEnter(
      new Func<Task<string>>( async () =>
      {
        var result = await HttGetRequest();
        return GetOpponentName(result);
      }
     )

### Hierarchically nested states
Supports hierarchically nested states.

    ...
    builder
      .DefineState(States.OnFloor)
      .AsSubstateOf(States.Healthy) // set the parent state
      .OnEnter(AnnounceFloor)
    ...
See the "Elevator" example for more details.

### Conditional transitions using C# not DSL

Instead of introducing conditional transition into state machine's DSL like


    ❌
    .If(CallDialled, Ringing, () => IsValidNumber)
    .If(CallDialled, Beeping, () => !IsValidNumber);

or

    ❌
    .If(CheckOverload).Goto(MovingUp)
    .Otherwise().Execute(AnnounceOverload)

**Binstate** allows using C#

✔️

    .AddTransition(CallDialed, () => IsValidNumber ? RingingEvent : BeepingEvent)

    .AddTransition(GoUp, () =>
      {
        if(CheckOverload) return MovingUp;
        AnnounceOverload();
        return null; // no transition will be performed
      });

### Safe checking if state machine still in the state

The current state of the state machine is not exposed publicly. No knowledge which state to check - less errors.

❌ not

    while(stateController.State == ❌CopyPastedWrongState❌)

✔️ but

    private static Task PlayMusic(IStateController<Event> stateController)
    {
      return Task.Run(() =>
      {
        while (✔️stateController.InMyState✔️)
        {
          // play music
        }
      });
    }

### Changing a state from an 'enter' action

      private async Task TrackGame(IStateController<Event> stateController, string opponentName)
      {
        while (stateController.InMyState)
        {
          // track game
          if(IsGameFinished())
            stateController.RaiseAsync(GameFinishedEvent);
        }
      }

 ### Enter/Exit/Transition actions with arguments

         builder
           .DefineState(WaitingForGame)
           .OnExit<string>(WaitForGame)
           .AddTransition<string>(GameStarted, TrackingGame, OnTransitionToTrackingGame)
           ...

         builder
           .DefineState(TrackingGame)
           .OnEnter<string>(TrackGame)
           ...

### Persistence
Serialize the state machine's current state using `var serializedData = stateMachine.Serialize()`.<br>
To restore it later, use `var stateMachine = builder.Restore(serializedData)`.<br>
This recreates the state machine in its saved state, ready to resume operation.

### Passing and propagating arguments
#### Propagating arguments attached to a state upon activation

         builder
           .DefineState(SomeState)
           .OnEnter<string>(...) // argument passed to 'Raise' method is passed to the 'enter' action and is 'attached' to the state
           .AddTransition(SomeEvent, AnotherState)

         builder
           .DefineState(AnotherState)
           .OnEnter<string>(...) // this state also requires an argument
           ...

         stateMachine.Raise(SomeEvent); // argument will be passed from SomeState to AnotherState

#### Mixing propagating and passing arguments
          builder
           .DefineState(SomeState)
           .OnEnter<string>(...) // argument passed to 'Raise' mtehod is passed to the 'enter' action and is 'attached' to the state
           .AddTransition(SomeEvent, AnotherState)

         builder
           .DefineState(AnotherState)
           .OnEnter<ITuple<object, string>>(...) // this state requires two arguments; OnEnter<object, string>(...) overload can be used to simplify code
           ...

         // one argument will be propagated from the SomeState and the second one passed through Raise method
         stateMachine.Raise(SomeEvent, new object());

#### Propagate an argument to one of the activated states and pass to another
          builder
            .DefineState(SomeState)
            .OnEnter<string>(...) // argument passed to the 'enter' action is 'attached' to the state
            .AddTransition(SomeEvent, Child)

         builder
           .DefineState(Parent)
           .OnEnter<object>(...)
           ...

         builder
           .DefineState(Child).AsSubstateOf(Parent)
           .OnEnter<string>(...)
           ...

         // object passed to Raise will be passed to the Parent state and string argument from the SomeState will be propagated to the Child state
         stateMachine.Raise(SomeEvent, new object()) // will be passed to Child

* Argument for propagating can be got from one of the parents of the active state if the active state itself has no argument.
* Argument will be propagated to all parent states of the newly activated state if they require an argument.
* If a state already has 'tuple' argument, it can be split by two when propagating to the newly activated state (and its parents) depending on their 'enter' actions argument


## Examples

#### Telephone call

      var builder = new Builder<State, Event>(OnException);

      builder
        .DefineState(OffHook)
        .AddTransition(CallDialed, Ringing);

      builder
        .DefineState(Ringing)
        .AddTransition(HungUp, OffHook)
        .AddTransition(CallConnected, Connected);

      builder
        .DefineState(Connected)
        .AddTransition(LeftMessage, OffHook)
        .AddTransition(HungUp, OffHook)
        .AddTransition(PlacedOnHold, OnHold);

      builder
        .DefineState(OnHold)
        .OnEnter(PlayMusic)
        .AddTransition(TakenOffHold, Connected)
        .AddTransition(HungUp, OffHook)
        .AddTransition(PhoneHurledAgainstWall, PhoneDestroyed);

      builder
        .DefineState(PhoneDestroyed);

      var stateMachine = builder.Build(OffHook);

      // ...
      stateMachine.RaiseAsync(CallDialed);

#### Elevator

      public class Elevator
      {
        private readonly StateMachine<States, Events> _elevator;

        public Elevator()
        {
            var builder = new Builder<States, Events>(OnException);

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
