using System;
using System.Diagnostics.CodeAnalysis;
using FakeItEasy;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class MemoryTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  [AssertTraffic(AllocatedObjectsCount = 0, Types = new[] {typeof(ValueType1), typeof(ValueType2)})]
  public void should_not_boxing_passed_value_type_arguments(RaiseWay raiseWay)
  {
    var expected1     = new ValueType1(389);
    var expected2     = new ValueType2(659);
    var onEnterChild  = A.Fake<Action<ValueType1>>();
    var onEnterParent = A.Fake<Action<ValueType2>>();
    var onEnterState2 = A.Fake<Action<ITuple<ValueType2, ValueType1>>>();

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<ValueType1>(_ => { })
           .AddTransition(Event2, State2);

    builder.DefineState(Parent)
           .OnEnter<ValueType2>(onEnterParent);

    builder.DefineState(Child)
           .AsSubstateOf(Parent)
           .OnEnter<ValueType1>(onEnterChild);

    builder.DefineState(State2)
           .AsSubstateOf(Child)
           .OnEnter<ITuple<ValueType2, ValueType1>>(onEnterState2);

    var target = builder.Build(Initial, true);

    // --act
    target.Raise(raiseWay, Event1, expected1); // pass to State1

    target.Relaying<ValueType1>().Raise(raiseWay, Event2, expected2); // pass everywhere

    // --assert
    A.CallTo(() => onEnterParent(expected2)).MustHaveHappenedOnceExactly();
    A.CallTo(() => onEnterChild(expected1)).MustHaveHappenedOnceExactly();
    A.CallTo(() => onEnterState2(Tuple.Of(expected2, expected1))).MustHaveHappenedOnceExactly();
  }

  internal readonly struct ValueType1
  {
    public readonly int Value;
    public ValueType1(int value) => Value = value;
  }

  internal readonly struct ValueType2
  {
    public readonly int Value;
    public ValueType2(int value) => Value = value;
  }
}