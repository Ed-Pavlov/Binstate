using System;
using System.Collections.Generic;
using System.IO;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class OnEnterTest : StateMachineTestBase
  {
    private static IEnumerable<TestCaseData> raise_terminated_with_argument_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Action<StateMachine<string, int>, int>((_, param) => _.Raise(Terminate, param))).SetName("Raise");
      yield return new TestCaseData(new Action<StateMachine<string, int>, int>((_, param) => _.RaiseAsync(Terminate, param).Wait())).SetName("RaiseAsync");
    }

    [TestCaseSource(nameof(raise_terminated_with_argument_source))]
    public void should_pass_argument_to_enter(Action<StateMachine<string, int>, int> raiseTerminated)
    {
      const int Expected = 5;
      var actual = Expected - 139;

      // --arrange
      var builder = new Builder<string, int>(Console.WriteLine);

      builder
        .DefineState(State1)
        .AddTransition<int>(Terminate, Terminated);

      builder
        .DefineState(Terminated)
        .OnEnter<int>((_, param) => actual = param);

      var stateMachine = builder.Build(State1);

      // --act
      raiseTerminated(stateMachine, Expected);

      // --assert
      actual.Should().Be(Expected);
    }

    [TestCaseSource(nameof(raise_terminated_with_argument_source))]
    public void should_pass_argument_to_simple_enter(Action<StateMachine<string, int>, int> raiseTerminated)
    {
      const int Expected = 5;
      var actual = Expected - 139;

      // --arrange
      var builder = new Builder<string, int>(Console.WriteLine);

      builder
        .DefineState(State1)
        .AddTransition<int>(Terminate, Terminated);

      builder
        .DefineState(Terminated)
        .OnEnter<int>((param) => actual = param);

      var stateMachine = builder.Build(State1);

      // --act
      raiseTerminated(stateMachine, Expected);

      // --assert
      actual.Should().Be(Expected);
    }

    [Test]
    public void should_not_accept_async_void_enter_action()
    {
      var builder = new Builder<string, int>(Console.WriteLine);

      var state = builder.DefineState(State1);
      Action action = () => state.OnEnter(AsyncVoidMethod);
      action.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(Config<string, int>.Enter.AsyncVoidMethodNotSupported);
    }

    [Test]
    public void should_not_accept_async_void_simple_enter_action()
    {
      var builder = new Builder<string, int>(Console.WriteLine);

      var state = builder.DefineState(State1);
      Action action = () => state.OnEnter(SimpleAsyncVoidMethod);
      action.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(Config<string, int>.Enter.AsyncVoidMethodNotSupported);
    }

    [Test]
    public void should_throw_exception_if_transition_argument_is_not_assignable_to_enter_action_argument()
    {
      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      const string Initial = "Initial";
      const string Working = "Working";
      builder.DefineState(Initial).AddTransition<int>(Working, Working);
      builder.DefineState(Working).OnEnter<string>(value => { });

      // --act
      Action target = () => builder.Build(Initial);

      // --assert
      target
        .Should().Throw<InvalidOperationException>()
        .WithMessage("The enter action argument of type 'System.String' is not assignable from the transition argument of type 'System.Int32'. " +
                     "See transition 'Working' from the state 'Initial' to the state 'Working'");
    }
    
    [Test]
    public void should_throw_exception_if_transition_has_argument_but_enter_action_has_not()
    {
      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      const string Initial = "Initial";
      const string Working = "Working";
      builder.DefineState(Initial).AddTransition<int>(Working, Working);
      builder.DefineState(Working).OnEnter(() => { });

      // --act
      Action target = () => builder.Build(Initial);

      // --assert
      target
        .Should().Throw<InvalidOperationException>()
        .WithMessage("The transition 'Working' from the state 'Initial' to the state 'Working' requires argument of type 'System.Int32' " +
                     "but enter action of the target state defined without argument");
    }
    
    [Test]
    public void should_throw_exception_if_transition_has_not_argument_but_enter_action_has()
    {
      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      const string Initial = "Initial";
      const string Working = "Working";
      builder.DefineState(Initial).AddTransition(Working, Working);
      builder.DefineState(Working).OnEnter<int>(value => { });

      // --act
      Action target = () => builder.Build(Initial);

      // --assert
      target
        .Should().Throw<InvalidOperationException>()
        .WithMessage("The transition 'Working' from the state 'Initial' to the state 'Working' doesn't require argument but enter action of the target state" +
                     " requires an argument of type 'System.Int32'");
    }
    
    [Test]
    public void should_pass_argument_if_transition_argument_is_differ_but_assignable_to_enter_action_argument()
    {
      IDisposable actual = null;
      var expected = new MemoryStream();
      
      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      const string Initial = "Initial";
      const string Working = "Working";
      builder.DefineState(Initial).AddTransition<Stream>(Working, Working);
      builder.DefineState(Working).OnEnter<IDisposable>(value => actual = value);
      
      var target = builder.Build(Initial);

      // --act
      target.Raise(Working, expected);
      
      // --assert
      actual.Should().BeSameAs(expected); 
    }

    private static async void AsyncVoidMethod(IStateMachine<int> _)
    {
    }

    private static async void SimpleAsyncVoidMethod()
    {
    }
  }
}