using System.Net;
using System.Threading.Tasks;
using Binstate;

namespace Instate.Tests.example
{
  public partial class Example
  {
    public class GameTracker
    {
      // states
      const string WaitingForGame = nameof(WaitingForGame);
      const string TrackingGame = nameof(TrackingGame);
      const string Terminated = nameof(Terminated);
      
      // events
      const string GameStarted = nameof(GameStarted);
      const string GameFinished = nameof(GameFinished);
      const string Terminate = nameof(Terminate);

      public GameTracker()
      {
        var builder = new Builder();

        builder
          .AddState(WaitingForGame)
          .OnEnter(WaitForGame)
          .AddTransition<string>(GameStarted, TrackingGame)
          .AddTransition(Terminate, Terminated);

        builder
          .AddState(TrackingGame)
          .OnEnter<string>(TrackGame)
          .AddTransition(GameFinished, WaitingForGame)
          .AddTransition(Terminate, Terminated);
      }
      
      private async Task WaitForGame(IStateMachine stateMachine)
      {
        while (stateMachine.InMyState)
        {
          var result = await HttGetRequest();
          var opponentName = GetOpponentName(result);
          if(opponentName != null)
            stateMachine.RaiseAsync(GameStarted, opponentName);
        }
      }

      private async Task TrackGame(IStateMachine stateMachine, string opponentName)
      {
        while (stateMachine.InMyState)
        {
          // track game
          if(IsGameFinished())
            stateMachine.RaiseAsync(GameFinished);
        }
      }
    
      private async Task<HttpWebResponse> HttGetRequest() => null;

      private string GetOpponentName(HttpWebResponse _) => null;
      private bool IsGameFinished() => false;
    }
  } 
}