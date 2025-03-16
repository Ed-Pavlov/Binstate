using System.Net;
using System.Threading.Tasks;
using BeatyBit.Binstate;

// ReSharper disable All

#pragma warning disable 1998

namespace Binstate.Tests;

public partial class Example
{
  public class GameTracker
  {
    // states
    private const string WaitingForGame = nameof(WaitingForGame);
    private const string TrackingGame   = nameof(TrackingGame);
    private const string Terminated     = nameof(Terminated);

    // events
    private const string GameStarted  = nameof(GameStarted);
    private const string GameFinished = nameof(GameFinished);
    private const string Terminate    = nameof(Terminate);

    public GameTracker()
    {
      var builder = new Builder<string, string>(OnException);

      builder
       .DefineState(WaitingForGame)
       .OnEnter(WaitForGame)
       .AddTransition(GameStarted, TrackingGame)
       .AddTransition(Terminate,   Terminated);

      builder
       .DefineState(TrackingGame)
       .OnEnter<string>(TrackGame)
       .AddTransition(GameFinished, WaitingForGame)
       .AddTransition(Terminate,    Terminated);
    }

    private async Task WaitForGame(IStateController<string> stateController)
    {
      while(stateController.InMyState)
      {
        var result       = await HttGetRequest();
        var opponentName = GetOpponentName(result);

        if(opponentName != null)
          stateController.RaiseAsync(GameStarted, opponentName);
      }
    }

    private async Task TrackGame(IStateController<string> stateController, string opponentName)
    {
      while(stateController.InMyState)
      {
        await Task.Delay(100); // do some work

        // track game
        if(IsGameFinished())
          stateController.RaiseAsync(GameFinished);
      }
    }

    private async Task<HttpWebResponse> HttGetRequest() => null!;

    private string GetOpponentName(HttpWebResponse _) => null!;
    private bool   IsGameFinished()                   => false;
  }
}