using System;
using System.Threading.Tasks;

namespace Binstate.Tests;

public partial class Example
{
  public void TelephoneCall()
  {
    // events
    const string callDialed             = nameof(callDialed);
    const string hungUp                 = nameof(hungUp);
    const string callConnected          = nameof(callConnected);
    const string leftMessage            = nameof(leftMessage);
    const string placedOnHold           = nameof(placedOnHold);
    const string takenOffHold           = nameof(takenOffHold);
    const string phoneHurledAgainstWall = nameof(phoneHurledAgainstWall);

    // states
    const string offHook        = nameof(offHook);
    const string ringing        = nameof(ringing);
    const string connected      = nameof(connected);
    const string onHold         = nameof(onHold);
    const string phoneDestroyed = nameof(phoneDestroyed);

    var builder = new Builder<string, string>(OnException);

    builder
     .DefineState(offHook)
     .AddTransition(callDialed, ringing);

    builder
     .DefineState(ringing)
     .AddTransition(hungUp,        offHook)
     .AddTransition(callConnected, connected);

    builder
     .DefineState(connected)
     .AddTransition(leftMessage,  offHook)
     .AddTransition(hungUp,       offHook)
     .AddTransition(placedOnHold, onHold);

    builder
     .DefineState(onHold)
     .OnEnter(PlayMusic)
     .AddTransition(takenOffHold,           connected)
     .AddTransition(hungUp,                 offHook)
     .AddTransition(phoneHurledAgainstWall, phoneDestroyed);

    builder
     .DefineState(phoneDestroyed);

    var stateMachine = builder.Build(offHook);
  }

  private static Task PlayMusic(IStateController<string> stateController) => Task.Run(
    () =>
    {
      while(stateController.InMyState)
      {
        // play music
      }
    }
  );

  private static void OnException(Exception exception) { }
}