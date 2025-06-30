using System;
using BeatyBit.Binstate;
using Binstate.Tests.Util;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class InitialStateTest : StateMachineTestBase
{
  [Test]
  public void should_call_enter_of_initial_state()
  {
    var onEnter = A.Fake<Action>();

    // --arrange
    var target = new Builder<string, int>(OnException);

    target.DefineState(Initial).OnEnter(onEnter).AddTransition(GoToX, StateX);
    target.DefineState(StateX);

    // --act
    target.Build(Initial, "arg");

    // --assert
    A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void should_pass_argument_to_initial_state_enter_action()
  {
    const string expected = "expected";
    var          onEnter  = A.Fake<Action<string>>();

    // --arrange
    var target = new Builder<string, int>(OnException);

    target.DefineState<string>(Initial).OnEnter(onEnter).AddTransition(GoToX, StateX);
    target.DefineState(StateX);

    // --act
    target.Build(Initial, expected);

    // --assert
    A.CallTo(() => onEnter(expected)).MustHaveHappenedOnceAndOnly();
  }

  [Test]
  public void should_pass_argument_to_initial_and_its_parents_states(
    [ValueSource(nameof(EnterExit))] Action<Builder<string, int>.ConfiguratorOf.IEnterAction<string>, Action<string>> setupRoot,
    [ValueSource(nameof(EnterExit))] Action<Builder<string, int>.ConfiguratorOf.IEnterAction<string>, Action<string>> setupParent)
  {
    const string expected     = "expected";
    var          onEnter      = A.Fake<Action<string>>();
    var          parentAction = A.Fake<Action<string>>();
    var          rootAction   = A.Fake<Action<string>>();

    // --arrange
    var target = new Builder<string, int>(OnException);

    target.DefineState<string>(Root).With(_ => setupRoot(_, rootAction));
    target.DefineState<string>(Parent).AsSubstateOf(Root).With(_ => setupParent(_, parentAction));
    target.DefineState<string>(Initial).AsSubstateOf(Parent).OnEnter(onEnter).AddTransition(GoToX, StateX);
    target.DefineState(StateX);

    // --act
    var sm = target.Build(Initial, expected);
    sm.Raise(GoToX); // exit initial state

    // --assert
    A.CallTo(() => rootAction(expected)).MustHaveHappenedOnceAndOnly();
    A.CallTo(() => parentAction(expected)).MustHaveHappenedOnceAndOnly();
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
    target.Should().ThrowExactly<ArgumentException>().WithMessage("No transitions defined from the initial state*");
  }

  [Test]
  public void should_use_parent_transition_if_transition_from_initial_state_is_not_set()
  {
    var onEnterX = A.Fake<Action>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Parent).AddTransition(GoToX, StateX);
    builder.DefineState(Initial).AsSubstateOf(Parent);
    builder.DefineState(StateX).OnEnter(onEnterX);
    var sm = builder.Build(Initial);

    // --act
    sm.Raise(GoToX);

    // --assert
    A.CallTo(() => onEnterX()).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void should_throw_exception_if_initial_state_requires_argument_but_no_argument_is_specified()
  {
    // --arrange
    var builder = new Builder<string, int>(_ => {Assert.Fail(_.ToString()); });

    builder.DefineState<string>(Initial).AllowReentrancy(GoToX);

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage("The state*");
  }

  [Test]
  public void should_throw_exception_if_parent_of_initial_state_requires_argument_but_no_argument_is_specified()
  {
    // --arrange
    var builder = new Builder<string, int>(_ => { });

    builder.DefineState<string>(Parent).AllowReentrancy(GoToX);
    builder.DefineState(Initial).AsSubstateOf(Parent);

    // --act
    Action target = () => builder.Build(Initial);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage("The state*");
  }
}