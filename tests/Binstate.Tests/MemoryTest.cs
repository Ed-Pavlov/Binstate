using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class MemoryTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  [AssertTraffic(AllocatedObjectsCount = 0, Types = new[] {typeof(ValueType1), typeof(ValueType2)})]
  public void should_not_boxing_passed_value_type_arguments(RaiseWay raiseWay)
  {
    var                            expected1   = new ValueType1(389);
    var                            expected2   = new ValueType2(659);
    ValueType1                     actual1     = default;
    ValueType2                     actual2     = default;
    ITuple<ValueType2, ValueType1> actualTuple = null;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<ValueType1>(value => { })
           .AddTransition(Event2, State2);

    builder.DefineState(Parent)
           .OnEnter<ValueType2>(value => actual2 = value);

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<ValueType1>(value => actual1 = value);

    builder.DefineState(State2)
           .AsSubstateOf(Child)
           .OnEnter<ITuple<ValueType2, ValueType1>>(value => actualTuple = value);

    var target = builder.Build(Initial, true);

    // --act
    target.Raise(raiseWay, Event1, expected1); // pass to State1

    target.Relaying<ValueType1>().Raise(raiseWay, Event2, expected2); // pass everywhere

    // --assert
    // actual.Should().Be(expected); -- this method leads boxing
    actual1.Value.Should().Be(expected1.Value);
    actual2.Value.Should().Be(expected2.Value);
    actualTuple!.RelayedArgument.Value.Should().Be(expected1.Value);
    actualTuple.PassedArgument.Value.Should().Be(expected2.Value);
  }

  private readonly struct ValueType1
  {
    public readonly int Value;
    public ValueType1(int value) => Value = value;
  }

  private readonly struct ValueType2
  {
    public readonly int Value;
    public ValueType2(int value) => Value = value;
  }
}