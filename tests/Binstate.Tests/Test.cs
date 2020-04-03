 using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Binstate;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Instate.Tests
{
  public class Tests : StateMachineTestBase
  {
    [Test]
    public void should_fail_if_references_not_registered_state()
    {
      var builder = new Builder();

      builder.AddState(initial).AddTransition(trigger1, "null_state");
      
      // --act
      Action action = () => builder.Build();
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("references nonexistent state"));
    }

    [Test]
    public void should_fail_if_several_triggers_reference_one_state()
    {
      var builder = new Builder();

      builder
        .AddState(initial)
        .AddTransition(trigger1, state1)
        .AddTransition(trigger1, terminated);

      // --act
      Action action = () => builder.Build();
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("is already added to state"));
    }
    
    [Test]
    public void should_change_state_on_trigger()
    {
      // --arrange
      var state1Enter = new Mock<Action>();

      var builder = new Builder();

      builder.InitialState.AddTransition(start, initial);
      builder
        .AddState(initial)
        .AddTransition(trigger1, state1);

      builder
        .AddState(state1)
        .OnEnter(_ => state1Enter.Object());

      var stateMachine = builder.Build();
      stateMachine.Fire(start);

      // --act
      stateMachine.Fire(trigger1);
      
      // --assert
      state1Enter.Verify(action => action(), Times.Once);
    }
    
    private static IEnumerable<TestCaseData> should_finish_enter_before_call_exit_source()
    {
      // using blocking and Async.Wait in order test should not exit before firing trigger is completely handled
      yield return new TestCaseData(new Action<StateMachine>(_ => _.Fire(terminate))).SetName("Fire");
      yield return new TestCaseData(new Action<StateMachine>(_ => _.FireAsync(terminate).Wait())).SetName("FireAsync");
    }
    
    [TestCaseSource(nameof(should_finish_enter_before_call_exit_source))]
    public void should_finish_enter_before_call_exit(Action<StateMachine> changeToTerminated)
    {
      var onEnterExited = false;
      // --arrange
      var builder = new Builder();
      builder.InitialState.AddTransition(trigger1, state1);

      builder
        .AddState(state1)
        .OnEnter((_ =>
        {
          Thread.Sleep(299);
          onEnterExited = true;
        }))
        .OnExit(OnExit) 
        .AddTransition(terminate, terminated);
      
      builder.AddState(terminated);

      var target = builder.Build();
      target.Fire(trigger1);
      
      // --act
      changeToTerminated(target);
      
      // --assert
      void OnExit() => onEnterExited.Should().BeTrue();
    }
    
    [TestCaseSource(nameof(should_finish_enter_before_call_exit_source))]
    public void should_finish_enter_before_call_next_enter(Action<StateMachine> changeToTerminated)
    {
      var onEnterExited = false;
      // --arrange
      var builder = new Builder();
      builder.InitialState.AddTransition(trigger1, state1);

      builder
        .AddState(state1)
        .OnEnter(_ =>
        {
          Thread.Sleep(287);
          onEnterExited = true;
        })
        .AddTransition(terminate, terminated);
      
      builder
        .AddState(terminated)
        .OnEnter(OnTerminatedEnter);

      var target = builder.Build();
      target.Fire(trigger1);
      
      // --act
      changeToTerminated(target);
      
      // --assert
      void OnTerminatedEnter(IStateMachine _) => onEnterExited.Should().BeTrue();
    }
    
    [TestCaseSource(nameof(should_finish_enter_before_call_exit_source))]
    public void should_finish_async_enter_before_call_exit(Action<StateMachine> changeToTerminated)
    {
      var onEnterExited = false;
      // --arrange
      var builder = new Builder();
      builder.InitialState.AddTransition(trigger1, state1);

      builder
        .AddState(state1)
        .OnEnter((async _ =>
        {
          while (_.InMyState)
          {
            await Task.Delay(160);
            await Task.Delay(348);
          }
          onEnterExited = true;
        }))
        .OnExit(OnExit) 
        .AddTransition(terminate, terminated);
      
      builder.AddState(terminated);

      var target = builder.Build();
      target.Fire(trigger1);
      
      // --act
      changeToTerminated(target);
      
      // --assert
      void OnExit() => onEnterExited.Should().BeTrue();
    }
    
    [TestCaseSource(nameof(should_finish_enter_before_call_exit_source))]
    public void should_finish_async_enter_before_call_next_enter(Action<StateMachine> changeToTerminated)
    {
      var onEnterExited = false;
      // --arrange
      var builder = new Builder();
      builder.InitialState.AddTransition(trigger1, state1);

      builder
        .AddState(state1)
        .OnEnter((async _ =>
        {
          while (_.InMyState)
          {
            await Task.Delay(160);
            await Task.Delay(348);
          }
          onEnterExited = true;
        }))
        .AddTransition(terminate, terminated);
      
      builder
        .AddState(terminated)
        .OnEnter(OnTerminatedEnter);

      var target = builder.Build();
      target.Fire(trigger1);
      
      // --act
      changeToTerminated(target);
      
      // --assert
      void OnTerminatedEnter(IStateMachine _) => onEnterExited.Should().BeTrue();
    }
    
    private static IEnumerable<TestCaseData> should_exit_current_state_before_entering_next_source_with_parameter()
    {
      // using blocking and Async.Wait in order test should not exit before firing trigger is completely handled
      yield return new TestCaseData(new Action<StateMachine>(_ => _.Fire(terminate, 5))).SetName("Blocking fire param");
      yield return new TestCaseData(new Action<StateMachine>(_ => _.FireAsync(terminate, 5).Wait())).SetName("Async fire param");
    }
    
    [TestCaseSource(nameof(should_exit_current_state_before_entering_next_source_with_parameter))]
    public void should_exit_current_state_before_entering_next_with_parameter(Action<StateMachine> fireTrigger)
    {
      // --arrange
      var builder = new Builder();
     
      var state1Exit = new Mock<Action>();
      
      builder.InitialState.AddTransition(start, state1);
      builder
        .AddState(state1)
        .OnExit(state1Exit.Object)
        .AddTransition<int>(terminate, terminated);

      builder
        .AddState(terminated)
        .OnEnter<int>((_, i) => 
            state1Exit.Verify(action => action.Invoke(), Times.Once) // --assert, when call enter of terminated, exit of state1 should be already called
        );

      var stateMachine = builder.Build();
      stateMachine.Fire(start);
      
      // --act
      fireTrigger(stateMachine);
    }
  }
}