# Binstate

Binstate is a simple but yet powerful state machine for .NET. Thread safe. Supports async methods. Supports hierarchically nested states.

#### See documentation for full details
https://github.com/Ed-Pavlov/Binstate#readme

## Features

* ### Thread safety
  The state machine is fully thread safe and allows calling any method from any thread.

* ### Control on what thread enter and exit actions are executed

  Binstate don't use it's own thread to execute transitions. It gives an application full control over the threading model of the state machine.

* ### Support async methods

* ### Hierarchically nested states
  Supports hierarchically nested states, see "Elevator" example.

* ### Conditional transitions using C# not DSL

  Instead of introducing conditional transition into state machine's DSL like

  ❌

      .If(CallDialled, Ringing, () => IsValidNumber)
      .If(CallDialled, Beeping, () => !IsValidNumber);

      // or
      .If(CheckOverload).Goto(MovingUp)
      .Otherwise().Execute(AnnounceOverload)

  Binstate allows using C#

  ✔️

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

* ### Relaying arguments attached to a state through states upon activation
  * Argument for relaying can be gotten from one of the parent of the active state if the active state itself has no argument.
  * Argument will be relayed to all parent states of the newly activated state if they require an argument.
  * If a state already has 'tuple' argument, it can be split by two when relaying to the newly activated state (and its parents) depending on their 'enter' actions argument