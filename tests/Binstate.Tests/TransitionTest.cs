using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests
{
  public class TransitionTest : StateMachineTestBase
  {
    [TestCaseSource(nameof(RaiseWays))]
    public void should_call_action_on_transition_between_exit_and_enter(RaiseWay raiseWay)
    {
      const string exit = "Exit";
      const string transition = "Transition";
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial)
        .OnExit(() => actual.Add(exit))
        .AddTransition(Event1, State1, () => actual.Add(transition));
      
      builder.DefineState(State1)
        .OnEnter(() => actual.Add(State1));
      
      var target = builder.Build(Initial);
      
      // --act
      target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(exit, transition, State1);
    }
    
    [TestCaseSource(nameof(RaiseWays))]
    public void raise_should_return_false_if_no_transition_found(RaiseWay raiseWay)
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);
      builder.DefineState(Initial).AddTransition(State1, State1);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));

      var target = builder.Build(Initial);

      // --act
      var actual = target.Raise(raiseWay, "WrongEvent");
      
      // --assert
      actual.Should().BeFalse();
    }

    [Test]
    public void controller_should_return_false_if_no_transition_found()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);
      builder.DefineState(Initial).OnEnter(OnEnterInitialState)
        .AddTransition(State1, State1);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));
      
      builder.Build(Initial);

      static void OnEnterInitialState(IStateMachine<string> stateMachine)
      {
        // --act
        var actual = stateMachine.RaiseAsync("WrongEvent");
        
        // --assert
        actual.Should().BeFalse();
      }
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_transit_via_dynamic_transition(RaiseWay raiseWay)
    {
      
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      var first = true;
      builder
        .DefineState(Initial)
        .AddTransition(Event1, () =>
        {
          var state = first ? State1 : State2;
          first = false;
          return state;
        });
      
      builder
        .DefineState(State1).AsSubstateOf(Initial)
        .OnEnter(_ => actual.Add(State1));
      
      builder
        .DefineState(State2)
        .OnEnter(_ => actual.Add(State2));

      var target = builder.Build(Initial);
      
      // --act
      target.Raise(raiseWay, Event1);
      target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(State1, State2);
    }
    
    [TestCaseSource(nameof(RaiseWays))]
    public void raise_should_return_false_if_dynamic_transition_returns_null(RaiseWay raiseWay)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      builder.DefineState(Initial)
        .AddTransition(Event1, () => null);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));

      var target = builder.Build(Initial);

      // --act
      var actual = target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeFalse();
    }

    [Test]
    public void controller_should_return_false_if_dynamic_transition_returns_null()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);
      builder.DefineState(Initial).OnEnter(OnEnterInitialState)
        .AddTransition(State1, () => null);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));
      
      builder.Build(Initial);

      static void OnEnterInitialState(IStateMachine<string> stateMachine)
      {
        // --act
        var actual = stateMachine.RaiseAsync("WrongEvent");
        
        // --assert
        actual.Should().BeFalse();
      }
    }
    
    [TestCaseSource(nameof(RaiseWays))]
    public void should_use_parent_transition(RaiseWay raiseWay)
    {
      var actual = new List<string>();

      // --arrange
      var builder = new Builder<string, string>(OnException);

      builder.DefineState(Initial).AddTransition(Child, Child);

      builder.DefineState(Parent)
        .AddTransition(State1, State1, () => actual.Add(Parent));
      
      builder.DefineState(Child).AsSubstateOf(Parent);
      builder.DefineState(State1)
        .OnEnter(_ => actual.Add(State1));
      
      var target = builder.Build(Initial);
      target.Raise(raiseWay, Child);

      // --act
      target.Raise(raiseWay, State1);

      // --assert
      actual.Should().Equal(Parent, State1);      
    }
  }
}