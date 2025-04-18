﻿using System;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Binstate;
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
     .AddTransition(GoToX, wrongState);

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target.Should()
          .ThrowExactly<InvalidOperationException>()
          .WithMessage($"The transition '{GoToX}' from the state '{Initial}' references not defined state '{wrongState}'");
  }

  [Test]
  public void should_throw_exception_if_parent_and_child_states_have_not_compatible_enter_arguments_and_argument_transfer_mode_is_strict()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToX, Child);

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
  public void should_not_throw_exception_if_parent_and_child_states_have_not_compatible_enter_arguments_and_argument_transfer_mode_is_free()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException, new Builder.Options{ArgumentTransferMode = ArgumentTransferMode.Free});

    builder.DefineState(Initial).AddTransition(GoToX, Child);

    builder.DefineState(Parent)
           .OnEnter<int>((sm, value) => { });

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<string>(value => { });

    // --act
    var target = builder.Build(Initial);

    // --assert
    target.Should().NotBeNull();
  }
}