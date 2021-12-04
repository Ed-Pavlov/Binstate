using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
public class ArgumentRelayingTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  public void should_relay_state_argument_to_the_next_state(RaiseWay raiseWay)
  {
    const int expected = 3987;
    var       actual   = expected - 3;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<int>(_ => { })
           .AddTransition(Event2, State2);

    builder.DefineState(State2)
           .OnEnter<int>(value => actual = value);

    var target = builder.Build(Initial);
    target.Raise(Event1, expected);

    // --act
    target.Relaying<int>().Raise(raiseWay, Event2);

    // --assert
    actual.Should().Be(expected);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_relay_argument_from_parent_of_active_state(RaiseWay raiseWay)
  {
    const int expected = 3987;
    var       actual   = expected - 87;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, Child);

    builder.DefineState(Parent)
           .OnEnter<int>(_ => { });

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .AddTransition(Event2, State2);

    builder.DefineState(State2)
           .OnEnter<int>(value => actual = value);

    var target = builder.Build(Initial);
    target.Raise(Event1, expected);

    // --act
    target.Relaying<int>().Raise(raiseWay, Event2);

    // --assert
    actual.Should().Be(expected);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_null_if_active_state_has_no_argument_and_relayArgumentIsRequired_is_false(RaiseWay raiseWay)
  {
    var actual = "bad";

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);
    builder.DefineState(State1).OnEnter<string>(value => actual = value);

    var stateMachine = builder.Build(Initial);

    // --act
    const bool relayArgumentIsRequired = false;
#pragma warning disable CS0618
    stateMachine.Relaying<string>(relayArgumentIsRequired).Raise(raiseWay, Event1);
#pragma warning restore CS0618

    // --assert
    actual.Should().BeNull();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_not_relay_state_argument_on_no_relaying_raise(RaiseWay raiseWay)
  {
    var entered = false;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<int>(_ => { })
           .AddTransition(Event2, State2);

    builder.DefineState(State2)
           .OnEnter(() => entered = true);

    var target = builder.Build(Initial);
    target.Raise(Event1, 93);

    // --act
    target.Raise(raiseWay, Event2);

    // --assert
    entered.Should().BeTrue();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_not_relay_state_argument_on_no_relaying_raise_with_argument(RaiseWay raiseWay)
  {
    const int relayed  = 3987;
    const int expected = relayed - 389;
    var       actual   = relayed - 3;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<int>(_ => { })
           .AddTransition(Event2, State2);

    builder.DefineState(State2)
           .OnEnter<int>(value => actual = value);

    var target = builder.Build(Initial);
    target.Raise(Event1, relayed);

    // --act
    target.Raise(raiseWay, Event2, expected);

    // --assert
    actual.Should().Be(expected);
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_both_passed_and_relayed_arguments_with_variance_conversion(RaiseWay raiseWay)
  {
    const string expectedPassedValue  = "stringValue";
    
    var expectedRelayedValue = new MemoryStream();
    var onEnter              = A.Fake<Action<ITuple<object, IDisposable>>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<Stream>(_ => { })
           .AddTransition(Event2, State2);

    builder.DefineState(State2)
           .OnEnter<ITuple<object, IDisposable>>(onEnter);

    var target = builder.Build(Initial);

    target.Raise(raiseWay, Event1, expectedRelayedValue); // attach an argument to State1

    // --act
    target.Relaying<Stream>().Raise(raiseWay, Event2, expectedPassedValue); // pass and relay arguments to State2 

    // --assert

    var expectedTuple = Tuple.Of<object, IDisposable>(expectedPassedValue, expectedRelayedValue);
    A.CallTo(() => onEnter(expectedTuple)).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_pass_relayed_passed_and_tuple_arguments_depending_on_enter_type(RaiseWay raiseWay)
  {
    const string expectedPassed  = "stringValue";
    var          expectedRelayed = new MemoryStream();

    var onEnterChild  = A.Fake<Action<IDisposable>>();
    var onEnterRoot   = A.Fake<Action<object>>();
    var onEnterParent = A.Fake<Action<ITuple<object, IDisposable>>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<MemoryStream>(_ => { })
           .AddTransition(Event2, Child);

    builder.DefineState(Root)
           .OnEnter<object>(onEnterRoot);

    builder.DefineState(Parent)
           .AsSubstateOf(Root)
           .OnEnter<ITuple<object, IDisposable>>(onEnterParent);

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<IDisposable>(onEnterChild);

    var target = builder.Build(Initial, true);
    target.Raise(raiseWay, Event1, expectedRelayed); // attach MemoryStream to State1

    // --act
    target.Relaying<MemoryStream>().Raise(raiseWay, Event2, expectedPassed); // pass string and relay MemoryStream to Child

    // --assert
    A.CallTo(() => onEnterRoot(expectedPassed)).MustHaveHappenedOnceExactly();
    A.CallTo(() => onEnterChild(expectedRelayed)).MustHaveHappenedOnceExactly();
    A.CallTo(() => onEnterParent(Tuple.Of<object, IDisposable>(expectedPassed, expectedRelayed))).MustHaveHappenedOnceExactly();
  }

  [TestCaseSource(nameof(RaiseWays))]
  public void should_throw_exception_if_target_state_has_no_argument_and_relaying_is_called(RaiseWay raiseWay)
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<int>(_ => { })
           .AddTransition(Event2, State2);

    builder.DefineState(State2)
           .OnEnter(() => { });

    var stateMachine = builder.Build(Initial);
    stateMachine.Raise(Event1, 93);

    // --act
    Action target = () => stateMachine.Relaying<int>().Raise(raiseWay, Event2);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage(
             $"Transition from the state '{State1}' by the event '{Event2}' will activate following states [{State2}]. No one of them are defined "
           + "with the enter action accepting an argument, but argument was passed or relayed");
  }

  [Test]
  public void should_throw_exception_if_no_argument_for_relaying()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);
    builder.DefineState(State1);

    var stateMachine = builder.Build(Initial);

    // --act
    Action target = () => stateMachine.Relaying<int>().Raise(Event1, 93);

    // --assert
    target.Should()
          .ThrowExactly<TransitionException>()
          .WithMessage("Raise with relaying argument is called from the state w/o an attached value and a backup argument for relay is not provided");
  }
}