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
      var builder = new Builder();

      builder
        .AddState(initial)
        .AddTransition(event1, "null_state");
      
      // --act
      Action action = () => builder.Build(initial);
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("references nonexistent state"));
    }

    [Test]
    public void should_fail_if_triggers_reference_one_state()
    {
      var builder = new Builder();

      builder
        .AddState(initial)
        .AddTransition(event1, state1)
        .AddTransition(event1, terminated);

      // --act
      Action action = () => builder.Build(initial);
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("is already added to state"));
    }
    
    [Test]
    public void should_change_state_on_trigger()
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder();

      builder
        .AddState(initial)
        .AddTransition(event1, state1);

      builder
        .AddState(state1)
        .OnEnter(_ => actual.Add(state1));

      var stateMachine = builder.Build(initial);

      // --act
      stateMachine.Raise(event1);
      
      // --assert
      actual.Should().BeEquivalentTo(state1);
    }
    
    private static IEnumerable<TestCaseData> raise_terminate_source()
    {
      // using blocking and Async.Wait in order test should not exit before firing trigger is completely handled
      yield return new TestCaseData(new Action<StateMachine>(_ => _.Raise(terminate))).SetName("Raise");
      yield return new TestCaseData(new Action<StateMachine>(_ => _.RaiseAsync(terminate).Wait())).SetName("RaiseAsync");
    }
    
    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_enter_before_call_exit(Action<StateMachine> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder();
      builder
        .AddState(initial)
        .AddTransition(event1, state1);

      builder
        .AddState(state1)
        .OnEnter((_ =>
        {
          Thread.Sleep(299);
          actual.Add(OnEnter);
        }))
        .OnExit(() => actual.Add(OnExit)) 
        .AddTransition(terminate, terminated);
      
      builder.AddState(terminated);

      var target = builder.Build(initial);
      target.Raise(event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, OnExit);
    }

    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_async_enter_before_call_exit(Action<StateMachine> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder();
      builder.AddState(initial).AddTransition(event1, state1);

      builder
        .AddState(state1)
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
        .AddTransition(terminate, terminated);
      
      builder.AddState(terminated);

      var target = builder.Build(initial);
      target.Raise(event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, OnExit);
    }

    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_enter_before_call_next_enter(Action<StateMachine> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder();
      builder.AddState(initial).AddTransition(event1, state1);

      builder
        .AddState(state1)
        .OnEnter(_ =>
        {
          Thread.Sleep(287);
          actual.Add(OnEnter);
        })
        .AddTransition(terminate, terminated);
      
      builder
        .AddState(terminated)
        .OnEnter(_ => actual.Add(terminated));

      var target = builder.Build(initial);
      target.Raise(event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, terminated);
    }

    [TestCaseSource(nameof(raise_terminate_source))]
    public void should_finish_async_enter_before_call_next_enter(Action<StateMachine> raiseTerminated)
    {
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder();
      builder.AddState(initial).AddTransition(event1, state1);

      builder
        .AddState(state1)
        .OnEnter((async _ =>
        {
          while (_.InMyState)
          {
            await Task.Delay(160);
            await Task.Delay(348);
          }
          actual.Add(OnEnter);
        }))
        .AddTransition(terminate, terminated);
      
      builder
        .AddState(terminated)
        .OnEnter(_ => actual.Add(terminated));

      var target = builder.Build(initial);
      target.Raise(event1);
      
      // --act
      raiseTerminated(target);
      
      // --assert
      actual.Should().BeEquivalentTo(OnEnter, terminated);
    }
    
    private static IEnumerable<TestCaseData> raise_terminated_with_param_source()
    {
      // using blocking and Async.Wait in order test should not exit before firing trigger is completely handled
      yield return new TestCaseData(new Action<StateMachine, int>((_, param) => _.Raise(terminate, param))).SetName("Raise");
      yield return new TestCaseData(new Action<StateMachine, int>((_, param) => _.RaiseAsync(terminate, param).Wait())).SetName("RaiseAsync");
    }
    
    [TestCaseSource(nameof(raise_terminated_with_param_source))]
    public void should_pass_parameter_to_enter(Action<StateMachine, int> raiseTerminated)
    {
      const int expected = 5;
      var actual = expected - 139;
      
      // --arrange
      var builder = new Builder();
     
      builder
        .AddState(state1)
        .AddTransition<int>(terminate, terminated);

      builder
        .AddState(terminated)
        .OnEnter<int>((_, param) => actual = param);

      var stateMachine = builder.Build(state1);

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
      var builder = new Builder();
      
      builder.AddState(initial).AddTransition(event1, state1);
      builder
        .AddState(state1)
        .OnEnter(_ => actual.Add("Enter"))
        .OnExit(() => actual.Add("Exit"))
        .AllowReentrancy(event1);

      var target = builder.Build(initial);
      target.Raise(event1);

      // --act
      target.Raise(event1);
      
      // --assert
      actual.Should().BeEquivalentTo("Enter", "Exit", "Enter");
    }
  }
}