using System;
using System.Collections.Generic;
using System.IO;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class EnterActionTest : StateMachineTestBase
  {
    private static IEnumerable<TestCaseData> raise_terminated_with_argument_source()
    {
      // using blocking and Async.Wait in order test should not exit before raising an event is completely handled
      yield return new TestCaseData(new Action<StateMachine<string, int>, int>((_, param) => _.Raise(Terminate, param))).SetName("Raise");
      yield return new TestCaseData(new Action<StateMachine<string, int>, int>((_, param) => _.RaiseAsync(Terminate, param).Wait())).SetName("RaiseAsync");
    }

    [Test]
    public void should_call_enter_of_initial_state()
    {
      var entered = false;
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial).OnEnter(() => entered = true).AddTransition(Event1, () => null);
      
      // --act
      builder.Build(Initial);
      
      // --assert
      entered.Should().BeTrue();
    }

    [Test]
    public void should_throw_exception_if_initial_state_enter_requires_argument()
    {
      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder.DefineState(Initial).OnEnter<int>(_ => throw new InvalidOperationException()).AddTransition(Event1, () => null);
      
      // --act
      Action target = () => builder.Build(Initial);
      
      // --assert
      target.Should().ThrowExactly<TransitionException>().WithMessage("The enter action of the initial state must not require argument.");
    }

    [TestCaseSource(nameof(raise_terminated_with_argument_source))]
    public void should_pass_argument_to_enter(Action<StateMachine<string, int>, int> raiseTerminated)
    {
      const int expected = 5;
      var actual = expected - 139;

      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(State1)
        .AddTransition(Terminate, Terminated);

      builder
        .DefineState(Terminated)
        .OnEnter<int>((_, param) => actual = param);

      var stateMachine = builder.Build(State1);

      // --act
      raiseTerminated(stateMachine, expected);

      // --assert
      actual.Should().Be(expected);
    }

    [TestCaseSource(nameof(raise_terminated_with_argument_source))]
    public void should_pass_argument_to_simple_enter(Action<StateMachine<string, int>, int> raiseTerminated)
    {
      const int expected = 5;
      var actual = expected - 139;

      // --arrange
      var builder = new Builder<string, int>(OnException);

      builder
        .DefineState(State1)
        .AddTransition(Terminate, Terminated);

      builder
        .DefineState(Terminated)
        .OnEnter<int>(param => actual = param);

      var stateMachine = builder.Build(State1);

      // --act
      raiseTerminated(stateMachine, expected);

      // --assert
      actual.Should().Be(expected);
    }

    [Test]
    public void should_not_accept_async_void_enter_action()
    {
      var builder = new Builder<string, int>(OnException);

      var state = builder.DefineState(State1);
      Action action = () => state.OnEnter(AsyncVoidMethod);
      action.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be("'async void' methods are not supported, use Task return type for async method");
    }

    [Test]
    public void should_not_accept_async_void_simple_enter_action()
    {
      var builder = new Builder<string, int>(OnException);

      var state = builder.DefineState(State1);
      Action action = () => state.OnEnter(SimpleAsyncVoidMethod);
      action.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be("'async void' methods are not supported, use Task return type for async method");
    }

    [Test]
    public void should_throw_exception_if_argument_is_not_assignable_to_enter_action()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);
      
      builder.DefineState(Initial).AddTransition(State1, State1);
      builder.DefineState(State1).OnEnter<int>(value => { });
      var stateMachine = builder.Build(Initial);
      
      // --act
      Action target = () => stateMachine.Raise(State1, "argument");

      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"The state '{State1}' requires argument of type '{typeof(int)}' but no argument of compatible type has passed nor relayed");
    }
    
    [Test]
    public void should_throw_exception_if_no_argument_specified_for_to_enter_action_with_argument()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);
      
      builder.DefineState(Initial).AddTransition(State1, State1);
      builder.DefineState(State1).OnEnter<int>(value => { });
      var stateMachine = builder.Build(Initial);
      
      // --act
      Action target = () => stateMachine.Raise(State1);

      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"The enter action of the state '{State1}' is configured as required an argument but no argument was specified.");
    }
    
    [Test]
    public void should_throw_exception_if_argument_specified_for_enter_action_wo_argument()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);
      
      builder.DefineState(Initial).AddTransition(State1, State1);
      builder.DefineState(State1).OnEnter(() => { });
      var stateMachine = builder.Build(Initial);
      
      // --act
      Action target = () => stateMachine.Raise(State1, "argument");

      // --assert
      target.Should().ThrowExactly<TransitionException>()
        .WithMessage($"Transition from the state '{Initial}' by the event '{State1}' will activate following states [*]. No one of them are defined " +
                     "with the enter action accepting an argument, but argument was passed or relayed");
    }
    
    [Test]
    public void should_pass_argument_if_argument_is_differ_but_assignable_to_enter_action_argument()
    {
      IDisposable actual = null;
      var expected = new MemoryStream();
      
      // --arrange
      var builder = new Builder<string, string>(OnException);

      builder.DefineState(Initial).AddTransition(State1, State1);
      builder.DefineState(State1).OnEnter<IDisposable>(value => actual = value);
      
      var target = builder.Build(Initial);

      // --act
      target.Raise(State1, expected);
      
      // --assert
      actual.Should().BeSameAs(expected); 
    }

    [Test]
    public void should_fail_build_if_parent_and_child_states_have_not_compatible_enter_arguments()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);

      
      builder.DefineState(Parent).OnEnter<int>(value => {}).AddTransition(Child, () => null);
      builder.DefineState(Child).AsSubstateOf(Parent).OnEnter<string>(value => { });

      // --act
      Action target = () => builder.Build(Parent);

      // --assert
      target
        .Should().Throw<InvalidOperationException>()
        .WithMessage("Parent state 'Parent' enter action requires argument of type 'System.Int32' whereas it's child state 'Child' requires argument of " +
                     "not assignable to the parent type 'System.String'");
    }
    
    [Test]
    public void should_fail_build_if_parent_and_child_states_have_not_compatible_enter_arguments2()
    {
      // --arrange
      var builder = new Builder<string, string>(OnException);

      builder.DefineState(Initial).OnEnter<int>(value => {}).AddTransition(Parent, () => null);
      builder.DefineState(Parent).AsSubstateOf(Initial).OnEnter(value => {});
      builder.DefineState(Child).AsSubstateOf(Parent).OnEnter<string>(value => { });

      // --act
      Action target = () => builder.Build(Initial);

      // --assert
      target
        .Should().Throw<InvalidOperationException>()
        .WithMessage($"Parent state '{Initial}' enter action requires argument of type 'System.Int32' whereas it's child state '{Child}' requires argument of " +
                     "not assignable to the parent type 'System.String'");
    }
    
    [Test]
    public void should_pass_argument_if_parent_argument_is_differ_but_assignable_from_child_argument()
    {
      IDisposable actualDisposable = null;
      Stream actualStream = null;
      var expected = new MemoryStream();
      
      // --arrange
      var builder = new Builder<string, string>(OnException);

      builder.DefineState(Initial).AddTransition(Child, Child);
      builder.DefineState(Root).OnEnter<IDisposable>(value => actualDisposable = value);
      builder.DefineState(Parent).AsSubstateOf(Root).OnEnter(value => {});
      builder.DefineState(Child).AsSubstateOf(Parent).OnEnter<Stream>(value => actualStream = value);

      var target = builder.Build(Initial);
      
      // --act
      target.Raise(Child, expected);

      // --assert
      actualStream.Should().BeSameAs(expected);
      actualDisposable.Should().BeSameAs(expected);
    }
    
#pragma warning disable CS1998
    private static async void AsyncVoidMethod(IStateMachine<int> _)
    {
    }

    private static async void SimpleAsyncVoidMethod()
    {
    }
#pragma warning restore
  }
}