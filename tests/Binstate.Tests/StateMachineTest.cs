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
      var builder = new Builder<string, string>();

      builder
        .AddState(Initial)
        .AddTransition(Event1, "null_state");
      
      // --act
      Action action = () => builder.Build(Initial);
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("references not defined state"));
    }

    [Test]
    public void should_fail_if_transitions_reference_one_state()
    {
      var builder = new Builder<string, string>();

      builder
        .AddState(Initial)
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
      var builder = new Builder<string, string>();

      builder
        .AddState(Initial)
        .AddTransition(Event1, State1);

      builder
        .AddState(State1)
        .OnEnter(_ => actual.Add(State1));

      var stateMachine = builder.Build(Initial);

      // --act
      stateMachine.Raise(Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(State1);
    }
    
    private static IEnumerable<TestCaseData> raise_terminate_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Action<StateMachine<string, string>>(_ => _.Raise(Terminate))).SetName("Raise");
      yield return new TestCaseData(new Action<StateMachine<string, string>>(_ => _.RaiseAsync(Terminate).Wait())).SetName("RaiseAsync");
    }
    
    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_enter_before_call_exit(Action<StateMachine<string, string>> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, string>();
      builder
        .AddState(Initial)
        .AddTransition(Event1, State1);

      builder
        .AddState(State1)
        .OnEnter((_ =>
        {
          Thread.Sleep(299);
          actual.Add(OnEnter);
        }))
        .OnExit(() => actual.Add(OnExit)) 
        .AddTransition(Terminate, Terminated);
      
      builder.AddState(Terminated);

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, OnExit);
    }

    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_async_enter_before_call_exit(Action<StateMachine<string, string>> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, string>();
      builder.AddState(Initial).AddTransition(Event1, State1);

      builder
        .AddState(State1)
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
      
      builder.AddState(Terminated);

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, OnExit);
    }

    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_enter_before_call_next_enter(Action<StateMachine<string, string>> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, string>();
      builder.AddState(Initial).AddTransition(Event1, State1);

      builder
        .AddState(State1)
        .OnEnter(_ =>
        {
          Thread.Sleep(287);
          actual.Add(OnEnter);
        })
        .AddTransition(Terminate, Terminated);
      
      builder
        .AddState(Terminated)
        .OnEnter(_ => actual.Add(Terminated));

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, Terminated);
    }

    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_async_enter_before_call_next_enter(Action<StateMachine<string, string>> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, string>();
      builder.AddState(Initial).AddTransition(Event1, State1);

      builder
        .AddState(State1)
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
        .AddState(Terminated)
        .OnEnter(_ => actual.Add(Terminated));

      var target = builder.Build(Initial);
      target.Raise(Event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, Terminated);
    }
    
    private static IEnumerable<TestCaseData> raise_terminated_with_param_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Action<StateMachine<string, string>, int>((_, param) => _.Raise(Terminate, param))).SetName("Raise");
      yield return new TestCaseData(new Action<StateMachine<string, string>, int>((_, param) => _.RaiseAsync(Terminate, param).Wait())).SetName("RaiseAsync");
    }
    
    [TestCaseSource(nameof(raise_terminated_with_param_source))]
    public void should_pass_parameter_to_enter(Action<StateMachine<string, string>, int> raiseTerminated)
    {
      const int expected = 5;
      var actual = expected - 139;
      
      // --arrange
      var builder = new Builder<string, string>();
     
      builder
        .AddState(State1)
        .AddTransition<int>(Terminate, Terminated);

      builder
        .AddState(Terminated)
        .OnEnter<int>((_, param) => actual = param);

      var stateMachine = builder.Build(State1);

      // --act
      raiseTerminated(stateMachine, expected);
      
      // --assert
      actual.Should().Be(expected);
    }

    [Test]
    public void should_call_exit_and_enter_on_reentering()
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, string>();
      
      builder.AddState(Initial).AddTransition(Event1, State1);
      builder
        .AddState(State1)
        .OnEnter(_ => actual.Add("Enter"))
        .OnExit(() => actual.Add("Exit"))
        .AllowReentrancy(Event1);

      var target = builder.Build(Initial);
      target.Raise(Event1);

      // --act
      target.Raise(Event1);
      
      // --assert
      actual.Should().BeEquivalentTo("Enter", "Exit", "Enter");
    }
  }
}