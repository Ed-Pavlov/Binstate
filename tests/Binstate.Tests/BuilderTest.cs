using System;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
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
    public void define_state_should_throw_if_already_defined()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      var expected = builder.DefineState(Initial);
      
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
    public void should_fail_if_transitions_reference_one_state()
    {
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(Initial)
        .AddTransition(Event1, State1)
        .AddTransition(Event1, Terminated);

      // --act
      Action action = () => builder.Build(Initial);
      
      // --assert
      action.Should().ThrowExactly<InvalidOperationException>().Where(_ => _.Message.Contains("Duplicated event"));
    }
  }
}