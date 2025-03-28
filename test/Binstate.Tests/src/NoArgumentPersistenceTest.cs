using System;
using System.Text.Json;
using BeatyBit.Binstate;
using FakeItEasy;
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

  public enum MyEnum
  {
    None,
    One,
    Two
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
    }

    // Act
    var targetBuilder = СonfigureRemaining(CreateTarget());
    targetBuilder.Restore(serializedData);

    // Assert
    A.CallTo(() => rootEnterWithArgs(42)).MustHaveHappenedOnceExactly();
    return;

    Builder<string, int> СonfigureRemaining(Builder<string, int> builder)
    {
      builder
       .GetOrDefineState(Child)
       .OnEnter(childEnterWithArgs);

      return builder;
    }

  }

  [Test]
  public void should_throw_exception_on_restore_when_persistence_is_not_enabled()
  {
    var builder = new Builder<string, int>(_ => { }, new Builder.Options { EnableStateMachinePersistence = false });

    string serializedData;

    // Arrange
    {
      var validBuilder = CreateTarget();
      var stateMachine = validBuilder.Build(Initial);
      serializedData = stateMachine.Serialize();
    }

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => builder.Restore(serializedData));
  }

  [Test]
  public void should_throw_exception_if_state_is_missing_in_serialized_data()
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
    modifiedBuilder.GetOrDefineState("NewState");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => modifiedBuilder.Restore(serializedData));
  }

  [Test]
  public void should_throw_exception_on_argument_type_mismatch()
  {
    string serializedData;

    // Arrange
    {
      var builder = CreateTarget();
      builder
       .GetOrDefineState(Root)
       .OnEnter((int x) => { });

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(GoToRoot, 42); // Correct argument type
      serializedData = stateMachine.Serialize();
    }

    // Act
    var targetBuilder = CreateTarget();
    targetBuilder.GetOrDefineState(Root).OnEnter((string x) => { });

    // Assert
    Assert.Throws<InvalidOperationException>(() => targetBuilder.Restore(serializedData));
  }

  [Test]
  public void should_throw_exception_on_signature_mismatch()
  {
    string serializedData;

    // Arrange
    {
      var builder      = CreateTarget();
      var stateMachine = builder.Build(Initial);
      serializedData = stateMachine.Serialize();
    }

    // Modify serialized data to have a different signature
    var tamperedData = serializedData.Replace("originalSignature", "tamperedSignature");

    // Act & Assert
    var targetBuilder = CreateTarget();
    Assert.Throws<InvalidOperationException>(() => targetBuilder.Restore(tamperedData));
  }

  [Test]
  public void should_handle_empty_serialized_data_gracefully()
  {
    var builder = CreateTarget();

    const string emptyData = "{}";

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => builder.Restore(emptyData));
  }

  private static Builder<string, int> CreateTarget() => CreateBaseBuilder(new Builder.Options { EnableStateMachinePersistence = true });
}