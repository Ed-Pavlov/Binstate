using System;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests
{
  public class BuilderTest : StateMachineTestBase
  {
    [Test]
    public void should_throw_exception_if_initial_state_is_not_defined()
    {
      const string wrongState = "Wrong";
      
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial);
      
      // --act
      Action target = () => builder.Build(wrongState);
      
      // --assert
      target.Should().ThrowExactly<ArgumentException>().WithMessage($"No state '{wrongState}' is defined");
    }
    
    [Test]
    public void should_throw_exception_if_initial_state_has_no_transition()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial);
      
      // --act
      Action target = () => builder.Build(Initial);
      
      // --assert
      target.Should().ThrowExactly<ArgumentException>().WithMessage($"No transitions defined from the initial state");
    }

    [Test]
    public void define_state_should_throw_exception_if_already_defined()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial);
      
      // --act
      Action target = () => builder.DefineState(Initial);

      // --assert
      target.Should().ThrowExactly<ArgumentException>();
    }
    
    [Test]
    public void get_or_define_state_should_return_existent_state_if_already_defined()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      var expected = builder.DefineState(Initial);
      
      // --act
      var actual = builder.GetOrDefineState(Initial);
      
      // --assert
      actual.Should().BeSameAs(expected);
    }
    
    [Test]
    public void should_throw_exception_if_transitions_to_different_states_by_one_event()
    {
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1)
        .AddTransition(Event1, State2);

      // --act
      Action target = () => builder.Build(Initial);
      
      // --assert
      target.Should().ThrowExactly<InvalidOperationException>().WithMessage("Duplicated event *");
    }
    
    [Test]
    public void should_throw_exception_if_transition_refers_not_defined_state()
    {
      const string wrongState = "null_state";
      
      var builder = new Builder<string, int>(OnException);
      
      builder
        .DefineState(Initial)
        .AddTransition(Event1, wrongState);

      // --act
      Action target = () => builder.Build(Initial);

      // --assert
      target.Should().ThrowExactly<InvalidOperationException>()
        .WithMessage($"The transition '{Event1}' from the state '{Initial}' references not defined state '{wrongState}'");
    }
    
    [Test]
    public void should_throw_exception_if_parent_and_child_states_have_not_compatible_enter_arguments_and_enable_loose_relaying_is_false()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);


      builder.DefineState(Initial).AddTransition(Event1, Child);
      
      builder.DefineState(Parent)
        .OnEnter<int>((sm, value) => {});
      
      builder.DefineState(Child).AsSubstateOf(Parent)
        .OnEnter<string>(value => { });

      // --act
      Action target = () => builder.Build(Initial);

      // --assert
      target
        .Should().Throw<InvalidOperationException>()
        .WithMessage($"Parent state '{Parent}' enter action requires argument of type '{typeof(int)}' whereas it's child state '{Child}' requires argument of " +
                     $"not assignable to the parent type '{typeof(string)}'");
    }
    
    [Test]
    public void should_not_throw_exception_if_parent_and_child_states_have_not_compatible_enter_arguments_and_enable_loose_relaying_is_true()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial).AddTransition(Event1, Child);
      
      builder.DefineState(Parent)
        .OnEnter<int>((sm, value) => {});
      
      builder.DefineState(Child).AsSubstateOf(Parent)
        .OnEnter<string>(value => { });

      // --act
      var target = builder.Build(Initial, true);

      // --assert
      target.Should().NotBeNull();
    }
  }
}