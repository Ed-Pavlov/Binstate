using System;
using System.Threading;
using System.Threading.Tasks;
using Binstate;
using NUnit.Framework;

namespace Instate.Tests
{
  public class Tests
  {
    [Test]
    public void Test1()
    {
      var initial = "Initial";
      var terminated = "Terminated";
      
      var trigger1 = "trigger1";
      var terminate = "terminate";
      
      Builder
        .AddState(initial)
        .AddTransition(trigger1, "state1");

      
      Builder
        .AddState("state1")
        .OnEntry(State1)
        .AddTransition(terminate, terminated);

      Builder
        .AddState(terminated)
        .OnEntry(_ => Console.WriteLine("Terminated"));

      var stateMachine = Builder.Build(initial); // all validations
      stateMachine.Fire(trigger1);

      Thread.Sleep(1000);
      
      stateMachine.Fire(terminate);
    }

    private async Task State1(IStateMachine stateMachine)
    {
      Console.WriteLine("Enter state1");
      while (stateMachine.InMyState) 
        await Task.Delay(5000);
      Console.WriteLine("Exit state1");
    }
  }
}