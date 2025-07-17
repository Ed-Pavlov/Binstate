using System;
using System.Text.Json;
using BeatyBit.Binstate;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class PrimitiveTypePersistenceTest : StateMachineTestBase
{
  [Test]
  public void should_call_enter_action_of_active_state_on_restore()
  {
    string data;

    // --arrange
    {
      var builder = CreateTarget();

      var sourceMachine = builder.Build(Initial);
      sourceMachine.Raise(GoToChild);
      data = sourceMachine.Serialize();

      ClearRecordedCalls();
    }

    // create the new instance of builder
    var target = CreateTarget();

    // --act
    target.Restore(data);

    // --assert
    A.CallTo(RootEnterAction)
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(ChildEnterAction).MustHaveHappenedOnceExactly());

    A.CallTo(InitialEnterAction).MustNotHaveHappened();
    return;

    Builder<string, int> CreateTarget()
    {
      var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });

      builder
       .GetOrDefineState(Initial)
       .OnEnter(InitialEnterAction)
       .AddTransition(GoToChild, Child);

      builder
       .GetOrDefineState(Root)
       .OnEnter(RootEnterAction);

      builder
       .GetOrDefineState(Child)
       .AsSubstateOf(Root)
       .OnEnter(ChildEnterAction);

      return builder;
    }
  }

  [Test]
  public void should_throw_exception_on_restore_with_invalid_json()
  {
    const string invalidJson = "{ invalid }";

    // --arrange
    var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });
    builder
     .DefineState(Initial)
     .AddTransition(GoToChild, Child);


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
      var builder = CreateTarget();

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(GoToChild, 42);
      serializedData = stateMachine.Serialize();

      ClearRecordedCalls(childEnterWithArgs, rootEnterWithArgs);
    }

    // Act
    var targetBuilder = CreateTarget();
    targetBuilder.Restore(serializedData);

    // Assert
    A.CallTo(() => rootEnterWithArgs(42))
     .MustHaveHappenedOnceExactly()
     .Then(A.CallTo(() => childEnterWithArgs(42)).MustHaveHappenedOnceExactly());

    return;

    Builder<string, int> CreateTarget()
    {
      var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });
      builder
       .DefineState(Initial)
       .AddTransition(GoToChild, Child);

      builder
       .GetOrDefineState<int>(Root)
       .OnEnter(rootEnterWithArgs);

      builder
       .GetOrDefineState<int>(Child)
       .AsSubstateOf(Root)
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

      var validBuilder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });
      validBuilder
       .DefineState(Initial)
       .AddTransition(GoToChild, Child);

      validBuilder.DefineState(Child);

      var stateMachine = validBuilder.Build(Initial);
      serializedData = stateMachine.Serialize();
    }

    // Act & Assert
    var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = false });
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
    return;

    Builder<string, int> CreateTarget()
    {
      var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });
      builder
       .DefineState(Initial)
       .AddTransition(GoToChild, Child);

      builder.DefineState(Child);

      return builder;
    }
  }

  [Test]
  public void should_throw_exception_if_builders_are_not_compatible2()
  {
    string serializedData;

    // Arrange
    {
      var builder = CreateTarget();
      builder.GetOrDefineState<int>(Root);

      var stateMachine = builder.Build(Initial);
      stateMachine.Raise(GoToRoot, 42); // Correct argument type
      serializedData = stateMachine.Serialize();
    }

    // Act
    var targetBuilder = CreateTarget();
    targetBuilder.GetOrDefineState<string>(Root);

    // Assert
    Assert.Throws<ArgumentException>(() => targetBuilder.Restore(serializedData));
    return;

    Builder<string, int> CreateTarget()
    {
      var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });
      builder
       .DefineState(Initial)
       .AddTransition(GoToRoot, Root);

      return builder;
    }
  }

  [Test]
  public void should_handle_empty_serialized_data_gracefully()
  {
    var builder = new Builder<string, int>(OnException, new Builder.Settings { EnableStateMachinePersistence = true });
    builder
     .DefineState(Initial)
     .AddTransition(GoToChild, Child);

    const string emptyData = "{}";

    // Act
    var action = () => builder.Restore(emptyData);

    // Assert
    action.Should().Throw<Exception>();
  }
}