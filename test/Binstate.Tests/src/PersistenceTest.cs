using System.Text;
using BeatyBit.Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class PersistenceTest
{
  [Test]
  public void should_call_enter_action_on_restore()
  {
    string data;

    // --arrange
    {
      var builder = CreateNoArgumentBuilder(out _);
      var sourceMachine = builder.Build(State.Root);
      sourceMachine.Raise(Event.Enable);
      data = sourceMachine.Serialize();
    }

    var target = CreateNoArgumentBuilder(out var receiver);

    // --act
    target.Restore(data);

    // --assert
    receiver.GetString().Should().Be("Enter Healthy;Enter On;");
  }

  [Test]
  public void should_work_after_restore()
  {
    string data;

    // --arrange
    {
      var builder = CreateNoArgumentBuilder(out _);
      var sourceMachine = builder.Build(State.Root);
      sourceMachine.Raise(Event.Enable);
      sourceMachine.Raise(Event.Disable);
      data = sourceMachine.Serialize();
    }

    var target = CreateNoArgumentBuilder(out var receiver);
    var targetStateMachine = target.Restore(data);
    receiver.GetString().Should().Be("Enter Healthy;Enter Off;");

    // --act
    targetStateMachine.Raise(Event.Enable);
    targetStateMachine.Raise(Event.Disable);
    targetStateMachine.Raise(Event.Hit);

    // --assert

    receiver.GetString().Should().Be("Enter Healthy;Enter Off;Enter On;Enter Off;Enter Broken;");
  }

  private static Builder<string, int> CreateNoArgumentBuilder(out TestReceiver receiver)
  {
    var output = new TestReceiver();
    receiver = output;

    var target = new Builder<string, int>(_ => Assert.Fail(_.ToString()), new Builder.Options { EnableStateMachinePersistence = true });

    target
     .DefineState(State.Root)
     .OnEnter(() => output.Write("Enter Root;"))
     .AddTransition(Event.Enable, State.On)
     .AddTransition(Event.Hit,    State.Broken);

    target
     .DefineState(State.Healthy)
     .OnEnter(() => output.Write("Enter Healthy;"))
     .AddTransition(Event.Hit, State.Broken);

    target
     .DefineState(State.On)
     .AsSubstateOf(State.Healthy)
     .OnEnter(() => output.Write("Enter On;"))
     .AddTransition(Event.Disable, State.Off);

    target
     .DefineState(State.Off)
     .AsSubstateOf(State.Healthy)
     .OnEnter(() => output.Write("Enter Off;"))
     .AddTransition(Event.Enable, State.On);

    target
     .DefineState(State.Broken)
     .OnEnter(() => output.Write("Enter Broken;"));

    return target;
  }

  private class TestReceiver
  {
    private readonly StringBuilder _stringBuilder = new();

    public void   Write(string value) => _stringBuilder.Append(value);
    public string GetString()         => _stringBuilder.ToString();
  }

  private static class State
  {
    public const string Root    = "root";
    public const string Healthy = "healthy";
    public const string On      = "on";
    public const string Off     = "off";
    public const string Broken  = "broken";
  }

  private static class Event
  {
    public const int Enable  = 1;
    public const int Disable = 2;
    public const int Hit     = 3;
  }
}