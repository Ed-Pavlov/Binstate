using System;
using System.IO;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class ArgumentPassingTest : StateMachineTestBase
  {
    [TestCaseSource(nameof(RaiseWays))]
    public void should_pass_argument_to_enter(RaiseWay raiseWay)
    {
      const string expected = "expected";
      var actual = expected + "bad";

      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1);

      builder
        .DefineState(State1)
        .OnEnter<string>((sm, param) => actual = param);

      var target = builder.Build(Initial);

      // --act
      target.Raise(raiseWay, Event1, expected);
      
      // --assert
      actual.Should().Be(expected);
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_pass_argument_if_argument_is_differ_but_assignable_to_enter_action_argument(RaiseWay raiseWay)
    {
      IDisposable actual = null;
      var expected = new MemoryStream();
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial).AddTransition(Event1, State1);
      builder.DefineState(State1).OnEnter<IDisposable>((sm, value) => actual = value);
      
      var target = builder.Build(Initial);

      // --act
      target.Raise(raiseWay, Event1, expected);
      
      // --assert
      actual.Should().BeSameAs(expected); 
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_pass_argument_if_parent_and_child_argument_are_differ_but_assignable_and_enter_with_no_argument_on_the_pass(RaiseWay raiseWay)
    {
      IDisposable actualDisposable = null;
      Stream actualStream = null;
      var expected = new MemoryStream();
      
      // --arrange
      var builder = new Builder<string, string>(OnException);

      builder.DefineState(Initial).AddTransition(Child, Child);
      builder.DefineState(Root).OnEnter<IDisposable>(value => actualDisposable = value);
      builder.DefineState(Parent).AsSubstateOf(Root).OnEnter(sm => {});
      builder.DefineState(Child).AsSubstateOf(Parent).OnEnter<Stream>(value => actualStream = value);

      var target = builder.Build(Initial);
      
      // --act
      target.Raise(raiseWay, Child, expected);

      // --assert
      actualStream.Should().BeSameAs(expected);
      actualDisposable.Should().BeSameAs(expected);
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_throw_exception_if_argument_is_not_assignable_to_enter_action(RaiseWay raiseWay)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      
      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1);
      
      builder
        .DefineState(State1)
        .OnEnter<string>((sm, value) => { });
      
      var stateMachine = builder.Build(Initial);
      
      // --act
      Action target = () => stateMachine.Raise(raiseWay, Event1, 983);

      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"The state '{State1}' requires argument of type '{typeof(string)}' but no argument of compatible type has passed nor relayed");
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_throw_exception_if_argument_specified_and_no_argument_required_by_all_activated_states(RaiseWay raiseWay)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      
      builder
        .DefineState(Initial)
        .AddTransition(Event1, Child);
      
      builder
        .DefineState(Parent)
        .OnEnter(sm => { });

      builder
        .DefineState(Child).AsSubstateOf(Parent)
        .OnEnter(sm => { });
      
      var stateMachine = builder.Build(Initial);
      
      // --act
      Action target = () => stateMachine.Raise(raiseWay, Event1, "argument");

      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"Transition from the state '{Initial}' by the event '{Event1}' will activate following states [{Parent}->{Child}]. No one of them are defined with the enter " +
                     "action accepting an argument, but argument was passed or relayed");
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_throw_exception_if_no_argument_specified_for_enter_action_with_argument(RaiseWay raiseWay)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);
      
      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1);
      
      builder
        .DefineState(State1)
        .OnEnter<int>(value => { });
      
      var stateMachine = builder.Build(Initial);
      
      // --act
      Action target = () => stateMachine.Raise(raiseWay, Event1);

      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"The enter action of the state '{State1}' is configured as required an argument but no argument was specified.");
    }

    [TestCaseSource(nameof(RaiseWays))]
    public void should_throw_exception_if_parent_and_child_state_has_not_assignable_arguments_enable_loose_relaying_is_true_and_argument_is_passed(RaiseWay way)
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);


      builder.DefineState(Initial).AddTransition(Event1, Child);
      
      builder.DefineState(Parent)
        .OnEnter<int>((stateMachine, value) => {});
      
      builder.DefineState(Child).AsSubstateOf(Parent)
        .OnEnter<string>(value => { });

      // --act
      var sm = builder.Build(Initial, true);

      Action target = () => sm.Raise(Event1, "stringArgument");

      // --assert
      target
        .Should().Throw<TransitionException>()
        .WithMessage($"The state '{Parent}' requires argument of type '{typeof(int)}' but no argument of compatible type has passed nor relayed");
    }
    
    
  }
}