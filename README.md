# Binstate

Simple but yet powerful state machine. Thread safe. Supports async methods.

     
## Features

**Thread safe**

The state machine is fully thread safe and allows calling any method from any thread.

**Control on what thread enter and exit actions are executed**

Binstate don't use it's own thread to execute transitions.

`Raise(event)` 
will execute exit action of the current state and enter action of the new state on the current thread. 
If enter action is blocking `Raise` will block until enter action finishes.
`RaiseAsync(event)` uses `Task.Run` to execute transition.

 It gives an application full control on threading model of the state machine.   

**Support async methods**

Supports using async methods as enter action of the state. Binstate guarantees that async enter action will finis before calling exit action of the current state and enter action of the new state. Async method should return `Task`, `async void` methods are not supported. 

**Conditional transitions using C# not DSL**
    
Instead of introducing conditional transition into state machine DSL like

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
      
**Safe checking if state machine still in the state**

The current state of the state machine is not exposed. No knowledge which state to check - less errors.

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
    
**Changing state from enter action**

      private async Task TrackGame(IStateMachine<State> stateMachine, string opponentName)
      {
        while (stateMachine.InMyState)
        {
          // track game
          if(IsGameFinished())
            stateMachine.RaiseAsync(GameFinished);
        }
      }  
      
 **Enter actions with parameters**
 
         builder
          .DefineState(WaitingForGame)
          .OnEnter(WaitForGame)
          .AddTransition<string>(GameStarted, TrackingGame)
          ...
 
         builder
           .DefineState(TrackingGame)
           .OnEnter<string>(TrackGame)
           ...


### Example

**Telephone call**

      var builder = new Builder<State, Event>();

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