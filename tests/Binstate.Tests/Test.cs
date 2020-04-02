 using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Binstate;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using NUnit.Framework;

namespace Instate.Tests
{
  public class Tests
  {
    const string initial = "Initial";
    
    const string trigger1 = "trigger1";
    const string state1 = "state1";
    const string trigger2 = "trigger2";
    const string state2 = "state2";
    const string trigger3 = "trigger3";
    const string state3 = "state3";

    const string terminated = "Terminated";
    const string terminate = "terminate";

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
    public void should_call_enter_initial_state_on_start()
    {
      // --arrange
      var onEnter = new Mock<Action<IStateMachine>>();
      var builder = new Builder();
      builder.AddState(initial).OnEnter(onEnter.Object);

      var stateMachine = builder.Build();
      
      // --act
      stateMachine.Start(initial);
      
      // --assert
      onEnter.Verify(_ => _.Invoke(It.IsNotNull<IStateMachine>()), Times.Once);
    }

    [Test]
    public void should_change_state_on_trigger()
    {
      // --arrange
      var state1Enter = new Mock<Action>();

      var builder = new Builder();

      builder
        .AddState(initial)
        .AddTransition(trigger1, state1);

      builder
        .AddState(state1)
        .OnEnter(_ => state1Enter.Object());

      var stateMachine = builder.Build();
      stateMachine.Start(initial);

      // --act
      stateMachine.Fire(trigger1);
      
      // --assert
      state1Enter.Verify(action => action(), Times.Once);
    }
    
    [Test]
    public void should_call_exit_current_state_on_stop()
    {
      // --arrange
      var onExit = new Mock<Action>();
      var builder = new Builder();
      builder.AddState(initial).OnExit(onExit.Object);

      var stateMachine = builder.Build();
      stateMachine.Start(initial);
      
      // --act
      stateMachine.Stop();
      
      // --assert
      onExit.Verify(_ => _.Invoke(), Times.Once);
    }

    private static IEnumerable<TestCaseData> should_exit_current_state_before_entering_next_source()
    {
      // using blocking and Async.Wait in order test should not exit before firing trigger is completely handled
      yield return new TestCaseData(new Action<StateMachine>(_ => _.Fire(terminate))).SetName("Blocking fire");
      yield return new TestCaseData(new Action<StateMachine>(_ => _.FireAsync(terminate).Wait())).SetName("Async fire");
    }
    
    [TestCaseSource(nameof(should_exit_current_state_before_entering_next_source))]
    public void should_exit_current_state_before_entering_next(Action<StateMachine> fireTrigger)
    {
      // --arrange
      var builder = new Builder();
     
      var state1Exit = new Mock<Action>();
      
      builder
        .AddState(state1)
        .OnExit(state1Exit.Object)
        .AddTransition(terminate, terminated);

      builder
        .AddState(terminated)
        .OnEnter(_ => 
          state1Exit.Verify(action => action.Invoke(), Times.Once) // --assert, when call enter of terminated, exit of state1 should be already called
          );

      var stateMachine = builder.Build();
      stateMachine.Start(state1);
      
      // --act
      fireTrigger(stateMachine);
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
      stateMachine.Start(state1);
      
      // --act
      fireTrigger(stateMachine);
    }
    }
}