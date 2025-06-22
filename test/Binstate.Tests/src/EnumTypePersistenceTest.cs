using System;
using BeatyBit.Binstate;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

public class EnumTypePersistenceTest : StateMachineTestBase
{
  [Test]
  public void should_work_with_enum_serializer_if_enum_is_used_as_state_id()
  {
    var    enterAction = A.Fake<Action<int>>();
    string serializedData;

    // --arrange
    {
      var builder = new Builder<TestEnum, int>(OnException, new Builder.Options { EnableStateMachinePersistence = true });
      builder
       .DefineState<int>(TestEnum.A)
       .AllowReentrancy(1);

      var stateMachine = builder.Build(TestEnum.A, 32);
      serializedData = stateMachine.Serialize(Persistence.EnumCustomSerializer.Instance);
    }

    // --act
    {
      var target = new Builder<TestEnum, int>(OnException, new Builder.Options { EnableStateMachinePersistence = true });
      target
       .DefineState<int>(TestEnum.A)
       .OnEnter(enterAction)
       .AllowReentrancy(1);

      target.Restore(serializedData, Persistence.EnumCustomSerializer.Instance);
    }

    // --assert
    A.CallTo(() => enterAction(32)).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void should_work_with_enum_serializer_if_enum_is_used_as_state_argument()
  {
    var    enterAction = A.Fake<Action<TestEnum>>();
    string serializedData;

    // --arrange
    {
      var builder = new Builder<string, int>(OnException, new Builder.Options { EnableStateMachinePersistence = true });
      builder
       .DefineState<TestEnum>(Initial)
       .AllowReentrancy(GoToInitial);

      var stateMachine = builder.Build(Initial, TestEnum.A);
      serializedData = stateMachine.Serialize(Persistence.EnumCustomSerializer.Instance);
    }

    // --act
    {
      var target = new Builder<string, int>(OnException, new Builder.Options { EnableStateMachinePersistence = true });
      target
       .DefineState<TestEnum>(Initial)
       .OnEnter(enterAction)
       .AllowReentrancy(GoToInitial);

      target.Restore(serializedData, Persistence.EnumCustomSerializer.Instance);
    }

    // --assert
    A.CallTo(() => enterAction(TestEnum.A)).MustHaveHappenedOnceExactly();
  }

  [Test]
  public void should_throw_exception_if_not_primitive_type_is_used_as_state_id()
  {
    // --arrange
    var builder = new Builder<TestEnum, int>(OnException, new Builder.Options { EnableStateMachinePersistence = true });
    builder
     .DefineState<int>(TestEnum.A)
     .OnEnter((int _) => { })
     .AllowReentrancy(1);

    var stateMachine = builder.Build(TestEnum.A, 32);

    // --act
    var action = () => stateMachine.Serialize();

    // --assert
    action.Should().Throw<InvalidOperationException>();
  }

  [Test]
  public void should_throw_exception_if_not_primitive_type_is_used_as_state_argument()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException, new Builder.Options { EnableStateMachinePersistence = true });
    builder
     .DefineState<TestEnum>(Initial)
     .AllowReentrancy(GoToInitial);

    var stateMachine = builder.Build(Initial, TestEnum.A);

    // --act
    var action = () => stateMachine.Serialize();

    // --assert
    action.Should().Throw<InvalidOperationException>();
  }

  // ReSharper disable once MemberCanBePrivate.Global - it's needed by FakeItEasy
  public enum TestEnum { None, A, B }
}