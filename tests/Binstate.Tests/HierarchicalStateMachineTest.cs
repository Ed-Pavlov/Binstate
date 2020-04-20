using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Binstate;
using FluentAssertions;
using NUnit.Framework;

namespace Instate.Tests
{
  public class HierarchicalStateMachineTest
  {
    [Test]
    public void should_enter_all_parent_states()
    {
      var actual = new List<string>();

      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      builder
        .DefineState(Initial)
        .AddTransition(Branch1Level3, Branch1Level3);

      builder
        .DefineState(Root)
        .OnEnter(_ => actual.Add(Root));

      builder
        .DefineState(Branch1Level1)
        .AsSubstateOf(Root)
        .OnEnter(_ => actual.Add(Branch1Level1));

      builder
        .DefineState(Branch1Level2)
        .AsSubstateOf(Branch1Level1)
        .OnEnter(_ => actual.Add(Branch1Level2));

      builder
        .DefineState(Branch1Level3)
        .AsSubstateOf(Branch1Level2)
        .OnEnter(_ => actual.Add(Branch1Level3));

      var target = builder.Build(Initial);

      // --act
      target.Raise(Branch1Level3);

      // --assert
      actual.Should().Equal(Root, Branch1Level1, Branch1Level2, Branch1Level3);
    }

    [Test]
    public void should_exit_all_parent_states()
    {
      var actual = new List<string>();
      
      async Task EnterAsync(IStateMachine<string> stateMachine, string state)
      {
        while (stateMachine.InMyState) 
          await Task.Delay(1);
        actual.Add(state+Exit);
      }

      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      builder
        .DefineState(Initial)
        .AddTransition(Branch1Level3, Branch1Level3);

      builder
        .DefineState(Root)
        .OnEnter(_ => EnterAsync(_, Root))
        .OnExit(() => actual.Add(Root));

      builder
        .DefineState(Branch1Level1)
        .AsSubstateOf(Root)
        .OnEnter(_ => EnterAsync(_, Branch1Level1))
        .OnExit(() => actual.Add(Branch1Level1));

      builder
        .DefineState(Branch1Level2)
        .AsSubstateOf(Branch1Level1)
        .OnEnter(_ => EnterAsync(_, Branch1Level2))
        .OnExit(() => actual.Add(Branch1Level2));

      builder
        .DefineState(Branch1Level3)
        .AsSubstateOf(Branch1Level2)
        .OnEnter(_ => EnterAsync(_, Branch1Level3))
        .OnExit(() => actual.Add(Branch1Level3))
        .AddTransition(Free1, Free1);

      builder
        .DefineState(Free1)
        .OnEnter(_ => actual.Add(Free1));

      var target = builder.Build(Initial);
      target.Raise(Branch1Level3);

      // --act
      target.Raise(Free1);

      // --assert
      actual.Should().Equal(Branch1Level3+Exit, Branch1Level3, Branch1Level2+Exit, Branch1Level2, Branch1Level1+Exit, Branch1Level1, Root+Exit, Root, Free1);
    }

    [Test]
    public void should_not_exit_parent_state()
    {
      var actual = new List<string>();
      
      async Task EnterAsync(IStateMachine<string> stateMachine, string state)
      {
        while (stateMachine.InMyState) 
          await Task.Delay(1);
        actual.Add(state+Exit);
      }

      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      builder
        .DefineState(Initial)
        .AddTransition(Branch1Level2, Branch1Level2);

      builder
        .DefineState(Root)
        .OnEnter(_ => EnterAsync(_, Root))
        .OnExit(() => actual.Add(Root));

      builder
        .DefineState(Branch1Level1)
        .AsSubstateOf(Root)
        .OnEnter(_ => EnterAsync(_, Branch1Level1))
        .OnExit(() => actual.Add(Branch1Level1));

      builder
        .DefineState(Branch1Level2)
        .AsSubstateOf(Branch1Level1)
        .OnEnter(_ => EnterAsync(_, Branch1Level2))
        .OnExit(() => actual.Add(Branch1Level2))
        .AddTransition(Branch1Level3, Branch1Level3);

      builder
        .DefineState(Branch1Level3)
        .AsSubstateOf(Branch1Level2)
        .OnEnter(_ => actual.Add(Branch1Level3));
      
      var target = builder.Build(Initial);
      target.Raise(Branch1Level2);

      // --act
      target.Raise(Branch1Level3);

      // --assert
      actual.Should().Equal(Branch1Level3);
    }

    [Test]
    public void should_not_exit_common_root()
    {
      var actual = new List<string>();
      
      async Task RootEnterAsync(IStateMachine<string> stateMachine)
      {
        actual.Add(Root);
        while (stateMachine.InMyState) 
          await Task.Delay(1);
        actual.Add(Root+Exit);
      }

      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      builder
        .DefineState(Initial)
        .AddTransition(Branch1Level2, Branch1Level2);

      builder
        .DefineState(Root)
        .OnEnter(RootEnterAsync)
        .OnExit(() => actual.Add(Root));

      builder
        .DefineState(Branch1Level1)
        .AsSubstateOf(Root)
        .OnExit(() => actual.Add(Branch1Level1));

      builder
        .DefineState(Branch1Level2)
        .AsSubstateOf(Branch1Level1)
        .OnExit(() => actual.Add(Branch1Level2))
        .AddTransition(Branch2Level2, Branch2Level2);

      builder
        .DefineState(Branch2Level1)
        .AsSubstateOf(Root)
        .OnEnter(_ => actual.Add(Branch2Level1));

      builder
        .DefineState(Branch2Level2)
        .AsSubstateOf(Branch2Level1)
        .OnEnter(_ => actual.Add(Branch2Level2));
      
      var target = builder.Build(Initial);
      target.Raise(Branch1Level2);

      // --act
      target.Raise(Branch2Level2);

      // --assert
      actual.Should().Equal(Root, Branch1Level2, Branch1Level1, Branch2Level1, Branch2Level2);      
    }

    [Test]
    public void should_use_parent_transition()
    {
      var actual = new List<string>();

      // --arrange
      var builder = new Builder<string, string>(Console.WriteLine);

      builder
        .DefineState(Initial)
        .AddTransition(Branch1Level2, Branch1Level2);

      builder
        .DefineState(Root)
        .AddTransition(Free1, Free1);

      builder
        .DefineState(Branch1Level1)
        .AsSubstateOf(Root);

      builder
        .DefineState(Branch1Level2)
        .AsSubstateOf(Branch1Level1)
        .OnEnter(_ => actual.Add(Branch1Level2));

      builder
        .DefineState(Free1)
        .OnEnter(_ => actual.Add(Free1));
      
      var target = builder.Build(Initial);
      target.Raise(Branch1Level2);

      // --act
      target.Raise(Free1);

      // --assert
      actual.Should().Equal(Branch1Level2, Free1);      
    }
    
    private const string Initial = nameof(Initial);
    private const string Root = nameof(Root);
    private const string Branch1Level1 = nameof(Branch1Level1);
    private const string Branch1Level2 = nameof(Branch1Level2);
    private const string Branch1Level3 = nameof(Branch1Level3);
    private const string Branch2Level1 = nameof(Branch2Level1);
    private const string Branch2Level2 = nameof(Branch2Level2);
    private const string Free1 = nameof(Free1);
    private const string Exit = nameof(Exit);
  }
}