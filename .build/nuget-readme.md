# Binstate


**Binstate** is a simple but yet powerful thread-safe, hierarchical state machine for .NET.
Features include support for async methods, argument passing, state serialization, and more.

#### See documentation for full details
https://github.com/Ed-Pavlov/Binstate#readme

## Features

### Thread safety

The state machine is fully thread safe and allows calling any method from any thread.

### Control on what thread enter and exit actions are executed

**Binstate** doesn't use its own thread to execute actions.
It gives an application full control over the threading model of the state machine, and only you decide will it be a passive or an active state machine.

* ### Support async methods

      .OnEnter(
        new Func<Task<string>>( async () =>
        {
          var result = await HttGetRequest();
          return GetOpponentName(result);
        }
       )

* ### Hierarchically nested states

      builder
        .DefineState(States.OnFloor)
        .AsSubstateOf(States.Healthy) // set parent state
        .OnEnter(AnnounceFloor)

* ### Conditional transitions using C# not DSL

      .AddTransition(CallDialed, () => IsValidNumber ? Ringing : Beeping)

      .AddTransition(GoUp, () =>
        {
          if(CheckOverload) return MovingUp;
          AnnounceOverload();
          return null; // no transition will be executed
        });

* ### Enter/Exit/OnTransition actions with arguments

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
