using System;
using System.Collections.Generic;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class TransitionTest : StateMachineTestBase
  {
    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void should_call_action_on_transition(Func<StateMachine<string, int>, int, bool> raise)
    {
      const string Exit = "Exit";
      const string Transaction = "Transaction";
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).OnExit(() => actual.Add(Exit)).AddTransition(Event1, State1, () => actual.Add(Transaction));
      builder.DefineState(State1).OnEnter(() => actual.Add(State1));
      
      var target = builder.Build(Initial);
      
      // --act
      raise(target, Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(Exit, Transaction, State1);
    }

    [TestCaseSource(nameof(raise_and_raise_async_source))]
    public void raise_should_return_false_if_no_transition_found(Func<StateMachine<string, int>, int, bool> raise)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));

      var target = builder.Build(Initial);

      // --act
      var actual = raise(target, Event2);
      
      // --assert
      actual.Should().BeFalse();
    }

    [Test]
    public void controller_should_return_false_if_no_transition_found()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial).OnEnter(OnEnterInitialState).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));
      
      // --act
      builder.Build(Initial);

      // --assert
      static void OnEnterInitialState(IStateMachine<int> stateMachine)
      {
        stateMachine.RaiseAsync(Event2).Should().BeFalse();
      }
    }
  }
}