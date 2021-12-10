using System;
using FakeItEasy;
using NUnit.Framework;

namespace Binstate.Tests;

public class ArgumentTypeTest : StateMachineTestBase
{
  [Test]
  [Description("If argument type is set by OnExit action, no parameters, Enter and Transition should work")]
  public void set_type_in_exit()
  {
    var onEnter = A.Fake<Action>();
    var onTransition = A.Fake<Action>();
    // --arrange
    var target = new Builder<string, int>(OnException);

    target.DefineState(Initial)
          .OnEnter(onEnter)
          .OnExit<string>(Console.WriteLine)
          .AddTransition(GoToStateX, StateX, onTransition);

    target.DefineState(StateX);

    // --act
    var sm = target.Build(Initial, "arg");
    sm.Raise(GoToStateX);

    // --assert
    A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
    A.CallTo(() => onEnter()).MustHaveHappenedOnceExactly();
  }
}