using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BeatyBit.Binstate;
using BeatyBit.Bits;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
public class ConfigurationTest : StateMachineTestBase
{
  [Test]
  public void should_throw_exception_if_pass_null_to_as_substate_of()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    // --act
    Action target = () => builder.DefineState(Initial).AsSubstateOf(null!);

    // --assert
    target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*parentStateId*");
  }

  [Test]
  public void should_throw_exception_if_pass_null_to_define_state()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    // --act
    Action target = () => builder.DefineState(null!);

    // --assert
    target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*stateId*");
  }

  [Test]
  public void should_throw_exception_if_pass_null_to_get_or_define_state()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    // --act
    Action target = () => builder.GetOrDefineState(null!);

    // --assert
    target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*stateId*");
  }

  [Test]
  public void should_throw_exception_if_pass_null_initial_state_id()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AllowReentrancy(GoToX);

    // --act
    Action target = () => builder.Build(null!);

    // --assert
    target.Should().ThrowExactly<ArgumentNullException>().WithMessage("*initialStateId*");
  }

  [Test]
  public void on_enter_should_check_arguments_for_null()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);
    var config  = builder.DefineState(Initial);

#pragma warning disable 8625

    var actions = new Action[]
    {
      () => config.OnEnter((Action)null!),
      () => config.OnEnter((Func<Task>)null!),

      () => config.OnEnter((Action<object>)null!),
      () => config.OnEnter((Func<object, Task>)null!),


      () => config.OnEnter((Action<IStateController<string>>)null!),
      () => config.OnEnter((Func<IStateController<string>, Task>)null!),
    };

    // --assert
    foreach(var action in actions)
      action.Should().ThrowExactly<ArgumentNullException>();
  }

  [Test]
  public void on_exit_should_check_arguments_for_null()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);
    var config  = builder.DefineState<object>(Initial);

    // --act
    Action target01 = () => config.OnExit((Action<object>)null!);

    // --assert
    target01.Should().ThrowExactly<ArgumentNullException>();
  }

  [Test]
  public void on_enter_should_not_accept_async_void_method()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);
    var config  = builder.DefineState(Initial);

#pragma warning disable 1998
    async void AsyncMethod1()                                      { }
    async void AsyncMethod2(object                   _)            { }
    async void AsyncMethod3(object                   _, object __) { }
    async void AsyncMethod4(IStateController<string> _)                        { }
    async void AsyncMethod5(IStateController<string> _, object __)             { }
    async void AsyncMethod6(IStateController<string> _, object __, object ___) { }
#pragma warning restore 1998

    // --act
    Action target1 = () => config.OnEnter(AsyncMethod1);
    Action target2 = () => config.OnEnter(AsyncMethod2);
    Action target4 = () => config.OnEnter(AsyncMethod4);

    // --assert
    target1.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target2.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target4.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
  }

  [Test]
  public void add_transition_should_check_arguments_for_null()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);
    var config  = builder.DefineState(Initial);

    // --act
#pragma warning disable 8625, 8622
    var actions = new Action[]
    {
      // Basic transitions
      () => config.AddTransition(null,    Initial),
      () => config.AddTransition(Initial, null),
      () => config.AddTransition<int>(Initial, Initial, (Transition<Unit, int>.Action<string, string>)null!),

      // Conditional transitions
      () => config.AddConditionalTransition(null,    Initial, () => true),
      () => config.AddConditionalTransition(Initial, null,    () => true),
      () => config.AddConditionalTransition(Initial, Initial, (Func<bool>)null!),
      () => config.AddConditionalTransition(Initial, Initial, (Transition<Unit, Unit>.Guard)null!),
      () => config.AddConditionalTransition<int>(Initial, Initial, (Transition<Unit, int>.Guard)null!),
      () => config.AddConditionalTransition<int>(null,    Initial, (g) => true),
      () => config.AddConditionalTransition<int>(Initial, null,    (g) => true),

      // Dynamic transitions
      () => config.AddDynamicTransition(null,    () => "func"),
      () => config.AddDynamicTransition(Initial, (Func<string?>)null!),
      () => config.AddDynamicTransition(null,    StateSelector),
      () => config.AddDynamicTransition(Initial, (Transition<Unit, Unit>.StateSelector<string, string>)null!),
      () => config.AddDynamicTransition<int>(null,    () => "func",                                               (a) => { }),
      () => config.AddDynamicTransition<int>(Initial, (Func<string?>)null!,                                       (a) => { }),
      () => config.AddDynamicTransition<int>(Initial, () => "func",                                               null!),
      () => config.AddDynamicTransition<int>(null,    StateSelectorInt,                                           (a) => { }),
      () => config.AddDynamicTransition<int>(Initial, (Transition<Unit, int>.StateSelector<string, string>)null!, (a) => { }),
      () => config.AddDynamicTransition<int>(Initial, StateSelectorInt,                                           null!),

      // Reentrancy
      () => config.AllowReentrancy(null),
      () => config.AllowReentrancy<int>(null,    (a) => { }),
      () => config.AllowReentrancy<int>(Initial, null!)
    };
#pragma warning restore 8625, 8622

    // --assert
    foreach(var action in actions)
      action.Should().ThrowExactly<ArgumentNullException>();

    return;

    bool StateSelectorInt(Transition<Unit, int>.Context<string, string> _, out string s)
    {
      s = "state";
      return true;
    }

    bool StateSelector(Transition<Unit, Unit>.Context<string, string> _, out string s)
    {
      s = "state";
      return true;
    }
  }

  [Test]
  public void define_state_should_throw_exception_on_define_already_defined_state()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.DefineState(Initial);

    // --act
    Action target = () => builder.DefineState(Initial);

    // --assert
    target.Should().ThrowExactly<ArgumentException>();
  }

  [Test]
  public void get_or_define_state_should_return_existent_state_if_already_defined()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    var expected = builder.DefineState(Initial);

    // --act
    var actual = builder.GetOrDefineState(Initial);

    // --assert
    actual.Should().BeSameAs(expected);
  }

  [Test]
  public void get_or_define_state_should_define_state_if_still_no()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);

    builder.GetOrDefineState(Initial).AllowReentrancy(GoToX);

    // --act
    var actual = builder.Build(Initial);

    // --assert
    actual.Should().NotBeNull();
  }
}