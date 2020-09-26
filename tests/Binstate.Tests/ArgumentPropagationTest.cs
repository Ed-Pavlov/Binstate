using System;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class ArgumentPropagationTest : StateMachineTestBase
  {
    [TestCaseSource(nameof(propagate_and_propagate_async_source))]
    public void should_propagate_state_argument_to_the_next_state(Propagate<string, int> propagate)
    {
      const int Expected = 3987;
      var actual = Expected - 3;

      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter<int>(value => actual = value);

      var target = builder.Build(Initial);
      target.Raise(Event1, Expected);

      // --act
      propagate(target, Event2);
      
      // --assert
      actual.Should().Be(Expected);
    }
    
    [TestCaseSource(nameof(propagate_and_propagate_async_source))]
    public void should_fail_if_target_state_has_no_argument(Propagate<string, int> propagate)
    {
      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter(() => {});

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(Event1, 93);

      // --act
      Action target = () => propagate(stateMachine, Event2);
      
      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"Transition from the state '{State1}' by the event '{Event2}' will activate following states [{State2}]. No one of them are defined " +
                     "with the enter action accepting an argument, but argument was propagated");
    }
    
    [TestCaseSource(nameof(raise_and_raise_async_with_argument_source))]
    public void should_not_propagate_state_argument_on_simple_raise(RaiseArgument<string, int> raise)
    {
      const int Propagated = 3987;
      const int Expected = Propagated - 389;
      var actual = Propagated - 3;

      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter<int>(value => actual = value);

      var target = builder.Build(Initial);
      target.Raise(Event1, Propagated);

      // --act
      raise(target, Event2, Expected);
      
      // --assert
      actual.Should().Be(Expected);
    }
    
    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_not_propagate_state_argument_on_simple_raise2(Raise<string, int> raise)
    {
      var entered = false;
      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter(() => entered = true);

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(Event1, 93);

      // --act
      raise(stateMachine, Event2);
      
      // --assert
      entered.Should().BeTrue();
    }
  }
}