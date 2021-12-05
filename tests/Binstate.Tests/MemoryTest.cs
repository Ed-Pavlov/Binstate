global using static Binstate.Tests.TestCategory;
using System;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace Binstate.Tests;

public class BoxingTest : StateMachineTestBase
{
  [TestCaseSource(nameof(RaiseWays))]
  [Category(MemoryTest)]
  [DotMemoryUnit(FailIfRunWithoutSupport = false)]
  [AssertTraffic(AllocatedObjectsCount = 0, Types = new[] { typeof(ValueType1), typeof(ValueType2), })]
  public void should_not_boxing_passed_value_type_arguments(RaiseWay raiseWay)
  {
    // don't use FakeItEasy due to it boxes value types during comparison

    var                             expected1   = new ValueType1(389);
    var                             expected2   = new ValueType2(659);
    ValueType1                      actual1     = default;
    ValueType2                      actual2     = default;
    ITuple<ValueType2, ValueType1>? actualTuple = null;

    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial).AddTransition(Event1, State1);

    builder.DefineState(State1)
           .OnEnter<ValueType1>(_ => { })
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

    var startPoint = dotMemory.Check();

    // --act
    target.Raise(raiseWay, Event1, expected1); // pass to State1

    target.Relaying<ValueType1>().Raise(raiseWay, Event2, expected2); // pass everywhere

    // --assert
    dotMemory.Check(
      memory =>
        memory.GetTrafficFrom(startPoint)
              .Where(_ => _.Type.Is<ValueType1>() | _.Type.Is<ValueType2>())
              .AllocatedMemory.ObjectsCount
              .Should()
              .Be(0)
    );


    // dont' use actual.Should().Be(expected); due to this method leads boxing
    actual1.Value.Should().Be(expected1.Value);
    actual2.Value.Should().Be(expected2.Value);
    actualTuple!.RelayedArgument.Value.Should().Be(expected1.Value);
    actualTuple.PassedArgument.Value.Should().Be(expected2.Value);
  }

  [Test]
  [Category(MemoryTest)]
  [DotMemoryUnit(FailIfRunWithoutSupport = false)]
  [AssertTraffic(AllocatedObjectsCount = 0, Types = new[] { typeof(ValueType1), })]
  public void should_not_box_value_type_instance_passed_by_reflection()
  {
    // don't use FakeItEasy due to it boxes value types during comparison
    var expected = new ValueType1(938);

    Source source = new Source<ValueType1>(expected);
    Target target = new Target<ValueType1>();

    var method = typeof(Invoker).GetMethod(nameof(Invoker.PassArgument));

    if(method is null) throw new InvalidOperationException();

    var genericMethod = method.MakeGenericMethod(source.GetType().GetGenericArguments()[0]);

    // --act
    genericMethod.Invoke(null, new object[] { source, target, });

    // --assert
    target.Should().BeOfType<Target<ValueType1>>().Which.Arg.Value.Should().Be(expected.Value);
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

  private class Source { }

  private class Source<T> : Source
  {
    public readonly T Arg;
    public Source(T arg) => Arg = arg;
  }

  private class Target { }

  private class Target<T> : Target
  {
    public T? Arg;

    public void Foo(T arg) => Arg = arg;
  }

  private static class Invoker
  {
    public static void PassArgument<T>(Source source, Target target)
    {
      var st = (Source<T>)source;
      var tt = (Target<T>)target;
      tt.Foo(st.Arg);
    }
  }
}