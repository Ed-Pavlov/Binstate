 using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Binstate;
using FluentAssertions;
 using NUnit.Framework;

namespace Instate.Tests
{
  public class StateMachineTest : StateMachineTestBase
  {
    [Test]
    public void should_fail_if_transition_to_unknown_state()
    {
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, "null_state");
      
      // --act
      Action action = () => builder.Build(Initial);
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("references not defined state"));
    }

    [Test]
    public void should_fail_if_transitions_reference_one_state()
    {
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1)
        .AddTransition(Event1, Terminated);

      // --act
      Action action = () => builder.Build(Initial);
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("Duplicated event"));
    }
    
    [Test]
    public void should_change_state_on_event()
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter(_ => actual.Add(State1));

      var stateMachine = builder.Build(Initial);

      // --act
      stateMachine.Raise(Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(State1);
    }
    
    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_finish_enter_before_call_exit(Func<StateMachine<string, int>, int, bool> raise)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter(_ =>
        {
          Thread.Sleep(299);
          actual.Add(OnEnter);
        })
        .OnExit(() => actual.Add(OnExit)) 
        .AddTransition(Terminate, Terminated);
      
      builder.DefineState(Terminated);

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raise(target, Terminate);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, OnExit);
    }

    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_finish_async_enter_before_call_exit(Func<StateMachine<string, int>, int, bool> raise)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter(async _ =>
        {
          while (_.InMyState)
          {
            await Task.Delay(160);
            await Task.Delay(348);
          }
          actual.Add(OnEnter);
        })
        .OnExit(() => actual.Add(OnExit)) 
        .AddTransition(Terminate, Terminated);
      
      builder.DefineState(Terminated);

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raise(target, Terminate);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, OnExit);
    }

    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_finish_enter_before_call_next_enter(Func<StateMachine<string, int>, int, bool> raise)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter(_ =>
        {
          Thread.Sleep(287);
          actual.Add(OnEnter);
        })
        .AddTransition(Terminate, Terminated);
      
      builder
        .DefineState(Terminated)
        .OnEnter(_ => actual.Add(Terminated));

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raise(target, Terminate);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, Terminated);
    }

    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_finish_async_enter_before_call_next_enter(Func<StateMachine<string, int>, int, bool> raise)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter((async _ =>
        {
          while (_.InMyState)
          {
            await Task.Delay(160);
            await Task.Delay(348);
          }
          actual.Add(OnEnter);
        }))
        .AddTransition(Terminate, Terminated);
      
      builder
        .DefineState(Terminated)
        .OnEnter(_ => actual.Add(Terminated));

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raise(target, Terminate);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, Terminated);
    }

    [Test]
    public void should_call_exit_and_enter_on_reentering()
    {
      const string Enter = "Enter";
      const string Exit = "Exit";
      
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      
      builder
        .DefineState(State1)
        .OnEnter(_ => actual.Add(Enter))
        .OnExit(() => actual.Add(Exit))
        .AllowReentrancy(Event1);

      var target = builder.Build(Initial);
      target.Raise(Event1);

      // --act
      target.Raise(Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(Enter, Exit, Enter);
    }

    [Test]
    public void should_transit_via_dynamic_transition([Values(State1, State2)] string targetState)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      string DynamicTransition() => targetState;

      builder
        .DefineState(Initial)
        .AddTransition(Event1, DynamicTransition);
      
      builder
        .DefineState(State1)
        .OnEnter(_ => actual.Add(State1));
      
      builder
        .DefineState(State2)
        .OnEnter(_ => actual.Add(State2));

      var target = builder.Build(Initial);
      
      // --act
      target.Raise(Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(targetState);
    }
    
    [Test]
    public void should_not_transit_if_dynamic_transition_returns_null()
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, () => null);
      
      builder
        .DefineState(State1)
        .OnEnter(_ => actual.Add(State1));
      
      var target = builder.Build(Initial);
      
      // --act
      target.Raise(Event1);
      
      // --assert
      actual.Should().BeEmpty();
    }
  }
}