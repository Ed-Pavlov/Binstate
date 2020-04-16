using System.Threading.Tasks;
using Binstate;

namespace Instate.Tests.example
{
  public partial class Example
  {
    public void TelephoneCall()
    {
      // events
      const string CallDialed = nameof(CallDialed);
      const string HungUp = nameof(HungUp);
      const string CallConnected = nameof(CallConnected);
      const string LeftMessage = nameof(LeftMessage);
      const string PlacedOnHold = nameof(PlacedOnHold);
      const string TakenOffHold = nameof(TakenOffHold);
      const string PhoneHurledAgainstWall = nameof(PhoneHurledAgainstWall);
      
      // states
      const string OffHook = nameof(OffHook);
      const string Ringing = nameof(Ringing);
      const string Connected = nameof(Connected);
      const string OnHold = nameof(OnHold);
      const string PhoneDestroyed = nameof(PhoneDestroyed);
      
      var builder = new Builder<string, string>();

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
    }

    private static Task PlayMusic(IStateMachine<string> stateMachine)
    {
      return Task.Run(() =>
      {
        while (stateMachine.InMyState)
        {
          // play music
        }
      });
    }
  }
}