using System;
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
    public void should_transit_using_dynamic_transition(RaiseWay raiseWay)
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
    public void should_transit_using_dynamic_transition_using_value_type_default(RaiseWay raiseWay)
    {
      const int initialStateId = 1;
      const int stateId1 = 0; // default value
      const int stateId2 = 38;

      var first = true;
      bool DynamicTransition(out int state)
      {
        state = first ? stateId1 : stateId2;
        first = false;
        return true;
      }

      
      var actual = new List<int>();
      
      // --arrange
      var builder = new Builder<int, int>(OnException);
     
      builder
        .DefineState(initialStateId)
        .AddTransition(Event1, DynamicTransition);
      
      builder
        .DefineState(stateId1).AsSubstateOf(initialStateId)
        .OnEnter(_ => actual.Add(stateId1));
      
      builder
        .DefineState(stateId2)
        .OnEnter(_ => actual.Add(stateId2));

      var target = builder.Build(initialStateId);
      
      // --act
      target.Raise(raiseWay, Event1);
      target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeEquivalentTo(stateId1, stateId2);
    }
    
    [TestCaseSource(nameof(RaiseWays))]
    public void raise_should_return_false_if_dynamic_transition_returns_false_value_type(RaiseWay raiseWay)
    {
      const int initialStateId = 1;
      const int stateId = 2;
      
      static bool DynamicTransition(out int state)
      {
        state = stateId;
        return false;
      }
      
      // --arrange
      var builder = new Builder<int, int>(OnException);

      builder.DefineState(initialStateId)
        .AddTransition(Event1, DynamicTransition);
      builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

      var target = builder.Build(initialStateId);

      // --act
      var actual = target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeFalse();
    }
    
    [TestCaseSource(nameof(RaiseWays))]
    public void raise_should_return_false_if_dynamic_transition_returns_false_reference_type(RaiseWay raiseWay)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      static bool DynamicTransition(out string stateId)
      {
        stateId = State1;
        return false;
      }

      builder.DefineState(Initial)
        .AddTransition(Event1, DynamicTransition);
      builder.DefineState(State1).OnEnter(() => Assert.Fail("No transition should be performed"));

      var target = builder.Build(Initial);

      // --act
      var actual = target.Raise(raiseWay, Event1);
      
      // --assert
      actual.Should().BeFalse();
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
    
    [TestCaseSource(nameof(RaiseWays))]
    public void raise_should_return_false_if_dynamic_transition_returns_default(RaiseWay raiseWay)
    {
      const int initialStateId = 1;
      const int stateId = 2;
      
      // --arrange
      var builder = new Builder<int, int>(OnException);
      builder.DefineState(initialStateId)
        .AddTransition(Event1, () => default);
      
      builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));

      var target = builder.Build(initialStateId);

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
        var actual = stateMachine.RaiseAsync(State1);
        
        // --assert
        actual.Should().BeFalse();
      }
    }
    
    [Test]
    public void controller_should_return_false_if_dynamic_transition_returns_default()
    {
      const int initialStateId = 1;
      const int stateId = 2;
      
      // --arrange
      var builder = new Builder<int, int>(OnException);
      builder.DefineState(initialStateId).OnEnter(OnEnterInitialState)
        .AddTransition(Event1, () => default);
      builder.DefineState(stateId).OnEnter(() => Assert.Fail("No transition should be performed"));
      
      builder.Build(initialStateId);

      static void OnEnterInitialState(IStateMachine<int> stateMachine)
      {
        // --act
        var actual = stateMachine.RaiseAsync(Event1);
        
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

    [TestCaseSource(nameof(RaiseWays))]
    public void should_catch_user_action_exception_and_report(RaiseWay raiseWay)
    {
      Exception reportedException = null;
      
      // --arrange
      var builder = new Builder<string, string>(exc => reportedException = exc);
      builder.DefineState(Initial)
        .AddTransition(State1, State1, () => throw new TestException());

      builder.DefineState(State1);

      var target = builder.Build(Initial);

      // --act
      var actual = target.Raise(raiseWay, State1);

      // --assert
      actual.Should().BeTrue();
      reportedException.Should().BeOfType<TestException>();
    }
    
    [Test]
    public void should_throw_exception_if_transitions_to_different_states_by_one_event()
    {
      var builder = new Builder<string, int>(OnException);

      var config = builder
        .DefineState(Initial)
        .AddTransition(Event1, State1);

      // --act
      Action target = () => config.AddTransition(Event1, State2);;
      
      // --assert
      target.Should().ThrowExactly<ArgumentException>().WithMessage($"An item with the same key has already been added*");
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_not_perform_transition_if_dynamic_transition_throws_exception(RaiseWay raiseWay)
    {
      Exception actual = null;
      // --arrange
      var builder = new Builder<string, int>(exc => actual = exc);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, () => throw new TestException());

      var target = builder.Build(Initial);

      // --act
      var result = target.Raise(raiseWay, Event1);

      // --assert
      result.Should().BeFalse();
      actual.Should().BeOfType<TestException>();
    }
  }
}