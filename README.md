# Binstate

Simple but yet powerful state machine. Thread safe. Supports async methods.

### Example

**Telephone call**

      var builder = new Builder();

      builder
        .AddState(OffHook)
        .AddTransition(CallDialed, Ringing);
      
      builder
        .AddState(Ringing)
        .AddTransition(HungUp, OffHook)
        .AddTransition(CallConnected, Connected);
      
      builder
        .AddState(Connected)
        .AddTransition(LeftMessage, OffHook)
        .AddTransition(HungUp, OffHook)
        .AddTransition(PlacedOnHold, OnHold);
      
      builder
        .AddState(OnHold)
        .OnEnter(PlayMusic)
        .AddTransition(TakenOffHold, Connected)
        .AddTransition(HungUp, OffHook)
        .AddTransition(PhoneHurledAgainstWall, PhoneDestroyed);

      builder
        .AddState(PhoneDestroyed);

      var stateMachine = builder.Build(OffHook);
      
      // ... 
      stateMachine.RaiseAsync(CallDialed);
      
### Features
      
**Safe checking if state machine still in the state**

No knowledge which state to check, less errors

    private static Task PlayMusic(IStateMachine stateMachine)
    {
      return Task.Run(() =>
      {
        while (stateMachine.InMyState)
        {
          // play music
        }
      });
    }      
      
 **Enter actions with parameters**
 
         builder
          .AddState(WaitingForGame)
          .OnEnter(WaitForGame)
          .AddTransition<string>(GameStarted, TrackingGame)
          ...
 
         builder
           .AddState(TrackingGame)
           .OnEnter<string>(TrackGame)
           ...
           
**Changing state from enter action**

      private async Task TrackGame(IStateMachine stateMachine, string opponentName)
      {
        while (stateMachine.InMyState)
        {
          // track game
          if(IsGameFinished())
            stateMachine.RaiseAsync(GameFinished);
        }
      }
