[![Nuget](https://img.shields.io/nuget/dt/Binstate)](https://www.nuget.org/packages/Binstate/)
[![TeamCity](https://edpavlov.beta.teamcity.com/app/rest/builds/buildType:(id:Binstate_Build)/statusIcon)](https://edpavlov.beta.teamcity.com/viewType.html?buildTypeId=Binstate_Build&guest=1)
___

# Binstate

Binstate *(pronounced as "Be in state")* is a simple but yet powerful state machine for .NET. Thread safe. Supports async methods. Supports hierarchically nested states.

## Features

### Thread safety

The state machine is fully thread safe and allows calling any method from any thread.

### Control on what thread enter and exit actions are executed

Binstate don't use it's own thread to execute transitions.

`Raise(event)` 
executes an 'exit' action of the current state and an 'enter' action of the new state on the current thread. 
If an 'enter' action is blocking `Raise` will block until enter action finishes.
`RaiseAsync(event)` uses `Task.Run` to execute a transition.

It gives an application a full control on the threading model of the state machine.   

### Support async methods

Supports async methods as an 'enter' action of the state. Binstate guarantees that an async 'enter' action will finish before calling an 'exit'
 action of the current state and an 'enter' action of the new state. An async method should return `Task`, `async void` methods are not supported. 

### Conditional transitions using C# not DSL
    
Instead of introducing conditional transition into state machine's DSL like

    .If(CallDialled, Ringing, () => IsValidNumber)
    .If(CallDialled, Beeping, () => !IsValidNumber);

or 

    .If(CheckOverload).Goto(MovingUp)
    .Otherwise().Execute(AnnounceOverload)

Binstate allows using C#     

    .AddTransition(CallDialed, () => IsValidNumber ? Ringing : Beeping)
      
    .AddTransition(GoUp, () =>
      {
        if(CheckOverload) return MovingUp;
        AnnounceOverload();
        return null; // no transition will be executed
      });
      
### Safe checking if state machine still in the state

The current state of the state machine is not exposed publicly. No knowledge which state to check - less errors.

not `TState CurrentState{ get; }` but `bool InMyState {get;}`
    
    private static Task PlayMusic(IStateMachine<State> stateMachine)
    {
      return Task.Run(() =>
      {
        while (stateMachine.InMyState)
        {
          // play music
        }
      });
    }    
    
### Changing a state from an 'enter' action

      private async Task TrackGame(IStateMachine<State> stateMachine, string opponentName)
      {
        while (stateMachine.InMyState)
        {
          // track game
          if(IsGameFinished())
            stateMachine.RaiseAsync(GameFinished);
        }
      }  
      
 ### Enter actions with parameters
 
         builder
          .DefineState(WaitingForGame)
          .OnEnter(WaitForGame)
          .AddTransition<string>(GameStarted, TrackingGame)
          ...
 
         builder
           .DefineState(TrackingGame)
           .OnEnter<string>(TrackGame)
           ...
### Hierarchically nested states
Supports hierarchically nested states, see "Elevator" example.

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
