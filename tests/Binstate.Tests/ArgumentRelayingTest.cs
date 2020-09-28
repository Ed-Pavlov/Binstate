using System;
using System.IO;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class ArgumentRelayingTest : StateMachineTestBase
  {
    [TestCaseSource(nameof(using_relay_raise_and_raise_async_source))]
    public void should_relay_state_argument_to_the_next_state(RelayingRaise<string, int> relayingRaise)
    {
      const int expected = 3987;
      var actual = expected - 3;

      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter<int>(value => actual = value);

      var target = builder.Build(Initial);
      target.Raise(Event1, expected);

      // --act
      relayingRaise(target, Event2);
      
      // --assert
      actual.Should().Be(expected);
    }

    [Test]
    public void should_pass_relay_and_passed_arguments_depending_on_enter_type()
    {
      const int expectedRelayed = 3987;
      const string expectedPassed = "stringValue";
      
      var actualRelayed = 0;
      string actualPassed = null;
      ITuple<string, int> actualTuple = null;

      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message), true);
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, Child);
      
      builder.DefineState(Root).OnEnter<string>(value => actualPassed = value);
      builder.DefineState(Parent).AsSubstateOf(Root).OnEnter<ITuple<string, int>>(value => actualTuple = value);
      builder.DefineState(Child).AsSubstateOf(Parent).OnEnter<int>(value => actualRelayed = value);
      
      var target = builder.Build(Initial);
      target.Raise(Event1, expectedRelayed);
      
      // --act
      target.Relaying<int>().Raise(Event2, expectedPassed);
      
      // --assert
      actualPassed.Should().Be(expectedPassed);
      actualRelayed.Should().Be(expectedRelayed);
      actualTuple.PassedArgument.Should().Be(expectedPassed);
      actualTuple.RelayedArgument.Should().Be(expectedRelayed);
    }

    [Test]
    public void should_pass_both_passed_and_relayed_arguments_with_variance_conversion()
    {
      var expectedDisposable = new MemoryStream();
      const string expectedObject = "stringValue";
      
      ITuple<object, IDisposable> actual = null;

      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<Stream>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter<ITuple<object, IDisposable>>(value => actual = value);

      var target = builder.Build(Initial);
      
      target.Raise(Event1, expectedDisposable);

      // --act
      target.Relaying<Stream>().Raise(Event2, expectedObject);
      
      // --assert
      actual.PassedArgument.Should().Be(expectedObject);
      actual.RelayedArgument.Should().Be(expectedDisposable);
    }

    [TestCaseSource(nameof(using_relay_raise_and_raise_async_source))]
    public void should_relay_argument_from_parent_of_active_state(RelayingRaise<string, int> relayingRaise)
    {
      const int expected = 3987;
      var actual = expected - 87;
      
      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, Child);
      builder.DefineState(Parent).OnEnter<int>(_ => { });
      builder.DefineState(Child).AsSubstateOf(Parent).AddTransition(Event2, State2);
      
      builder.DefineState(State2).OnEnter<int>(value => actual = value);

      var target = builder.Build(Initial);
      target.Raise(Event1, expected);

      // --act
      relayingRaise(target, Event2);
      
      // --assert
      actual.Should().Be(expected);
    }
    
    [Test]
    public void should_pass_null_if_target_state_has_no_argument()
    {
      var actual = "bad";
      
      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<string>(value => actual = value);

      var stateMachine = builder.Build(Initial);

      // --act
      stateMachine.Relaying<string>(false).Raise(Event1);
      
      // --assert
      actual.Should().BeNull();
    }
    
    [TestCaseSource(nameof(using_relay_raise_and_raise_async_source))]
    public void should_fail_if_target_state_has_no_argument(RelayingRaise<string, int> relayingRaise)
    {
      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter(() => {});

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(Event1, 93);

      // --act
      Action target = () => relayingRaise(stateMachine, Event2);
      
      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"Transition from the state '{State1}' by the event '{Event2}' will activate following states [{State2}]. No one of them are defined " +
                     "with the enter action accepting an argument, but argument was passed or relayed");
    }
    
    [TestCaseSource(nameof(raise_and_raise_async_with_argument_source))]
    public void should_not_relay_state_argument_on_simple_raise(RaiseArgument<string, int> raise)
    {
      const int relayed = 3987;
      const int expected = relayed - 389;
      var actual = relayed - 3;

      // --arrange
      var builder = new Builder<string, int>(_ => Assert.Fail(_.Message));
      
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<int>(_ => {}).AddTransition(Event2, State2);
      builder.DefineState(State2).OnEnter<int>(value => actual = value);

      var target = builder.Build(Initial);
      target.Raise(Event1, relayed);

      // --act
      raise(target, Event2, expected);
      
      // --assert
      actual.Should().Be(expected);
    }
    
    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_not_relay_state_argument_on_simple_raise2(Raise<string, int> raise)
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