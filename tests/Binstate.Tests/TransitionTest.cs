using System.Collections.Generic;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class TransitionTest : StateMachineTestBase
  {
    [Test]
    public void should_call_action_on_transition()
    {
      const string Exit = "Exit";
      const string Transaction = "Transaction";
      var actual = new List<string>();
      
      // --arrange
      var builder = new Builder<string, string>(OnException);
      builder.DefineState(Initial).OnExit(() => actual.Add(Exit)).AddTransition(State1, State1, () => actual.Add(Transaction));
      builder.DefineState(State1).OnEnter(() => actual.Add(State1));
      
      var target = builder.Build(Initial);
      
      // --act
      target.Raise(State1);
      
      // --assert
      actual.Should().BeEquivalentTo(Exit, Transaction, State1);
    }
  }
}