using System;
using System.Collections.Generic;
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
    
    private static async void AsyncVoidMethod(IStateMachine<int> _){}
    private static async void SimpleAsyncVoidMethod(){}
  }
}