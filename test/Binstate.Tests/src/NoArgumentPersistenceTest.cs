using System;
using System.Text.Json;
using BeatyBit.Binstate;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class NoArgumentPersistenceTest : StateMachineTestBase
{
  [Test]
  public void should_call_enter_action_of_active_state_on_restore()
  {
    string data;

    // --arrange
    {
      var builder = СonfigureRemaining(CreateTarget());

      var sourceMachine = builder.Build(Initial);
      sourceMachine.Raise(GoToChild);
      data = sourceMachine.Serialize();

      ClearRecordedCalls();
    }

    // create the new instance of builder for purity
    var target = СonfigureRemaining(CreateTarget());

    // --act
    target.Restore(data);

    // --assert
    A.CallTo(RootEnterAction)
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(ChildEnterAction).MustHaveHappenedOnceExactly());

    A.CallTo(InitialEnterAction).MustNotHaveHappened();
    return;

    Builder<string, int> СonfigureRemaining(Builder<string, int> builder)
    {
      builder
       .GetOrDefineState(Initial)
       .OnEnter(InitialEnterAction);

      builder
       .GetOrDefineState(Root)
       .OnEnter(RootEnterAction);

      builder
       .GetOrDefineState(Child)
       .OnEnter(ChildEnterAction);

      return builder;
    }
  }

  [Test]
  public void should_throw_exception_on_restore_with_invalid_json()
  {
    // --arrange
    var          builder     = CreateTarget();
    const string invalidJson = "{ invalid }";

    // --act --assert
    Assert.Throws<JsonException>(() => builder.Restore(invalidJson));
  }

  [Test]
  public void should_preserve_arguments_across_serialize_and_restore()
  {
    string serializedData;

    var rootEnterWithArgs  = A.Fake<Action<int>>();
    var childEnterWithArgs = A.Fake<Action<int>>();

    // Arrange
    {
      var builder = СonfigureRemaining(CreateTarget());

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(GoToChild, 42);
      serializedData = stateMachine.Serialize();

      ClearRecordedCalls(childEnterWithArgs, rootEnterWithArgs);
    }

    // Act
    var targetBuilder = СonfigureRemaining(CreateTarget());
    targetBuilder.Restore(serializedData);

    // Assert
    A.CallTo(() => rootEnterWithArgs(42)).MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => childEnterWithArgs(42)).MustHaveHappenedOnceExactly());

    return;

    Builder<string, int> СonfigureRemaining(Builder<string, int> builder)
    {
      builder
       .GetOrDefineState(Root)
       .OnEnter(rootEnterWithArgs);
      builder
       .GetOrDefineState(Child)
       .OnEnter(childEnterWithArgs);

      return builder;
    }

  }

  [Test]
  public void should_throw_exception_on_restore_when_persistence_is_not_enabled()
  {
    string serializedData;

    // Arrange
    {
      var validBuilder = CreateTarget();
      var stateMachine = validBuilder.Build(Initial);
      serializedData = stateMachine.Serialize();
    }

    // Act & Assert
    var builder = new Builder<string, int>(OnException, new Builder.Options { EnableStateMachinePersistence = false });
    Assert.Throws<InvalidOperationException>(() => builder.Restore(serializedData));
  }

  [Test]
  public void should_throw_exception_if_builders_are_not_compatible1()
  {
    string serializedData;

    // Arrange
    {
      var builder      = CreateTarget();
      var stateMachine = builder.Build(Initial);
      serializedData = stateMachine.Serialize();
    }

    // Create a builder with a modified set of states
    var modifiedBuilder = CreateTarget();
    modifiedBuilder.DefineState("NewState");

    // Act & Assert
    Assert.Throws<ArgumentException>(() => modifiedBuilder.Restore(serializedData));
  }

  [Test]
  public void should_throw_exception_if_builders_are_not_compatible2()
  {
    string serializedData;

    // Arrange
    {
      var builder = CreateTarget();
      builder
       .GetOrDefineState(Root)
       .OnEnter<int>(_ => { });

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(GoToRoot, 42); // Correct argument type
      serializedData = stateMachine.Serialize();
    }

    // Act
    var targetBuilder = CreateTarget();
    targetBuilder
     .GetOrDefineState(Root)
     .OnEnter<string>(_ => { });

    // Assert
    Assert.Throws<ArgumentException>(() => targetBuilder.Restore(serializedData));
  }

  [Test]
  public void should_handle_empty_serialized_data_gracefully()
  {
    var builder = CreateTarget();

    const string emptyData = "{}";

    // Act
    var action = () => builder.Restore(emptyData);

    // Assert
    action.Should().Throw<Exception>();
  }

  private static Builder<string, int> CreateTarget() => CreateBaseBuilder(new Builder.Options { EnableStateMachinePersistence = true });
}