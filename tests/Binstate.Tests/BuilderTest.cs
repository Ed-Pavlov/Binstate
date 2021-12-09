using System;
using System.Diagnostics.CodeAnalysis;
using Binstate.Tests.Util;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class BuilderTest : StateMachineTestBase
{
  [Test]
  public void should_pass_argument_to_initial_state_enter_action()
  {
    const string expected = "expected";
    var          onEnter  = A.Fake<Action<string>>();

    // --arrange
    var target = new Builder<string, int>(OnException);

    target.DefineState(Initial).OnEnter(onEnter).AddTransition(GoToStateX, StateX);
    target.DefineState(StateX);

    // --act
    target.Build(Initial, expected);

    // --assert
    A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceAndOnly();
  }

  [Test]
  public void should_pass_argument_to_initial_and_its_parents_states()
  {
    const string expected = "expected";
    var          onEnter  = A.Fake<Action<string>>();
    var          onEnterParent  = A.Fake<Action<string>>();
    var          onEnterRoot  = A.Fake<Action<string>>();

    // --arrange
    var target = new Builder<string, int>(OnException);

    target.DefineState(Root).OnEnter(onEnterRoot);
    target.DefineState(Parent).AsSubstateOf(Root).OnEnter(onEnterParent);
    target.DefineState(Initial).AsSubstateOf(Parent).OnEnter(onEnter).AddTransition(GoToStateX, StateX);
    target.DefineState(StateX);

    // --act
    target.Build(Initial, expected);

    // --assert
    A.CallTo(() => onEnterRoot(expected)).MustHaveHappenedOnceAndOnly();
    A.CallTo(() => onEnterParent(expected)).MustHaveHappenedOnceAndOnly();
    A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceAndOnly();
  }

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
    target.Should().ThrowExactly<ArgumentException>().WithMessage("No transitions defined from the initial state");
  }

  [Test]
  public void should_throw_exception_if_initial_state_requires_argument_but_no_argument_is_specified()
  {
    // --arrange
    var builder = new Builder<string, int>(_ =>{});

    builder.DefineState(Initial).OnEnter<string>(_ => { }).AllowReentrancy(GoToStateX);

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage("The state*");
  }

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
     .WithMessage(
        $"Parent state '{Parent}' requires argument of type '{typeof(int)}' whereas it's child state '{Child}' requires argument of "
      + $"not assignable to the parent type '{typeof(string)}'"
      );
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

  [Test]
  public void should_throw_exception_if_transition_references_not_defined_state()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(GoToStateX, Child);

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target.Should().ThrowExactly<InvalidOperationException>($"The transition '{GoToStateX}' from the state '{Initial}' references not defined state '{Child}'");
  }
}