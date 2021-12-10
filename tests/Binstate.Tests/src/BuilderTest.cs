using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class BuilderTest : StateMachineTestBase
{
  [Test]
  public void should_throw_exception_if_transition_refers_not_defined_state()
  {
    const string wrongState = "null_state";

    var builder = new Builder<string, int>(OnException);

    builder
     .DefineState(Initial)
     .AddTransition(GoToStateX, wrongState);

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target.Should()
          .ThrowExactly<InvalidOperationException>()
          .WithMessage($"The transition '{GoToStateX}' from the state '{Initial}' references not defined state '{wrongState}'");
  }

  [Test]
  public void should_throw_exception_if_parent_and_child_states_have_not_compatible_enter_arguments_and_enable_loose_relaying_is_false()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, Child);

    builder.DefineState(Parent)
           .OnEnter<int>((sm, value) => { });

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<string>(value => { });

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target
     .Should()
     .Throw<InvalidOperationException>()
     .WithMessage( $"Parent state '{Parent}' requires argument of type '{typeof(int)}' whereas it's child state '{Child}'*");
  }

  [Test]
  public void should_not_throw_exception_if_parent_and_child_states_have_not_compatible_enter_arguments_and_enable_loose_relaying_is_true()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, Child);

    builder.DefineState(Parent)
           .OnEnter<int>((sm, value) => { });

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<string>(value => { });

    // --act
    var target = builder.Build(Initial, ArgumentTransferMode.Free);

    // --assert
    target.Should().NotBeNull();
  }
}