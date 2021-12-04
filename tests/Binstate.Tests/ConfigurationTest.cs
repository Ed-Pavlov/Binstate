using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
  public void should_throw_exception_if_pass_null_initial_state_id_to_build()
  {
    // --arrange
    var builder = new Builder<string, int>(OnException);
    builder.DefineState(Initial).AllowReentrancy(Event1);

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

    // --act
    Action target01 = () => config.OnEnter((Action) null!);
    Action target02 = () => config.OnEnter((Func<Task>) null!);

    Action target03 = () => config.OnEnter((Action<object>) null!);
    Action target04 = () => config.OnEnter((Func<object, Task>) null!);

    Action target05 = () => config.OnEnter((Action<object, object>) null!);
    Action target06 = () => config.OnEnter((Func<object, object, Task>) null!);

    Action target07 = () => config.OnEnter((Action<IStateMachine<string>>) null!);
    Action target08 = () => config.OnEnter((Func<IStateMachine<string>, Task>) null!);

    Action target09 = () => config.OnEnter((Action<IStateMachine<string>, object>) null!);
    Action target10 = () => config.OnEnter((Func<IStateMachine<string>, object, Task>) null!);

    Action target11 = () => config.OnEnter((Action<IStateMachine<string>, object, object>) null!);
    Action target12 = () => config.OnEnter((Func<IStateMachine<string>, object, object, Task>) null!);
#pragma warning restore 8625

    // --assert
    target01.Should().ThrowExactly<ArgumentNullException>();
    target02.Should().ThrowExactly<ArgumentNullException>();
    target03.Should().ThrowExactly<ArgumentNullException>();
    target04.Should().ThrowExactly<ArgumentNullException>();
    target05.Should().ThrowExactly<ArgumentNullException>();
    target06.Should().ThrowExactly<ArgumentNullException>();
    target07.Should().ThrowExactly<ArgumentNullException>();
    target08.Should().ThrowExactly<ArgumentNullException>();
    target09.Should().ThrowExactly<ArgumentNullException>();
    target10.Should().ThrowExactly<ArgumentNullException>();
    target11.Should().ThrowExactly<ArgumentNullException>();
    target12.Should().ThrowExactly<ArgumentNullException>();
  }

  [Test]
  public void on_enter_should_not_accept_async_void_method()
  {
    // --arrange
    var builder = new Builder<string, string>(OnException);
    var config  = builder.DefineState(Initial);

#pragma warning disable 1998
    async void AsyncMethod1()                                   { }
    async void AsyncMethod2(object                _)            { }
    async void AsyncMethod3(object                _, object __) { }
    async void AsyncMethod4(IStateMachine<string> _)                        { }
    async void AsyncMethod5(IStateMachine<string> _, object __)             { }
    async void AsyncMethod6(IStateMachine<string> _, object __, object ___) { }
#pragma warning restore 1998

    // --act
    Action target1 = () => config.OnEnter(AsyncMethod1);
    Action target2 = () => config.OnEnter<object>(AsyncMethod2);
    Action target3 = () => config.OnEnter<object, object>(AsyncMethod3);
    Action target4 = () => config.OnEnter(AsyncMethod4);
    Action target5 = () => config.OnEnter<object>(AsyncMethod5);
    Action target6 = () => config.OnEnter<object, object>(AsyncMethod6);

    // --assert
    target1.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target2.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target3.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target4.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target5.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
    target6.Should().ThrowExactly<ArgumentException>().WithMessage("'async void' methods are not supported, use Task return type for async method");
  }

  [Test]
  public void add_transition_should_check_arguments_for_null()
  {
    static bool GetState(out string? _)
    {
      _ = null;
      return false;
    }

    // --arrange
    var builder = new Builder<string, string>(OnException);
    var config  = builder.DefineState(Initial);

    // --act
#pragma warning disable 8625
    Action target1 = () => config.AddTransition(null, Initial);
    Action target2 = () => config.AddTransition(Initial, null, null!);
    Action target3 = () => config.AddTransition(null, () => "func");
    Action target4 = () => config.AddTransition(Initial, (Func<string>) null!);
    Action target5 = () => config.AddTransition(null, GetState);
    Action target6 = () => config.AddTransition(Initial, (GetState<string>) null!);
#pragma warning restore 8625

    // --assert
    target1.Should().ThrowExactly<ArgumentNullException>();
    target2.Should().ThrowExactly<ArgumentNullException>();
    target3.Should().ThrowExactly<ArgumentNullException>();
    target4.Should().ThrowExactly<ArgumentNullException>();
    target5.Should().ThrowExactly<ArgumentNullException>();
    target6.Should().ThrowExactly<ArgumentNullException>();
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

    builder.GetOrDefineState(Initial).AllowReentrancy(Event1);

    // --act
    var actual = builder.Build(Initial);

    // --assert
    actual.Should().NotBeNull();
  }
}