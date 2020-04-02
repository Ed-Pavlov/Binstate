using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public static class Config
  {
    public class Transition
    {
      internal readonly List<Binstate.Transition> Transitions = new List<Binstate.Transition>();

      public Transition AddTransition([NotNull] object trigger, [NotNull] object state)
      {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        if (state == null) throw new ArgumentNullException(nameof(state));
        
        Transitions.Add(new Binstate.Transition(trigger, null, state, false));
        return this;
      }

      public Transition AddTransition<TParameter>([NotNull] object trigger, [NotNull] object state, bool parameterCanBeNull = false)
      {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        if (state == null) throw new ArgumentNullException(nameof(state));
        
        Transitions.Add(new Binstate.Transition(trigger, typeof(TParameter), state, parameterCanBeNull));
        return this;
      }
    }
    
    public class Exiting : Transition
    {
      
      [CanBeNull] internal EnterInvoker Enter;
      [CanBeNull] internal Action Exit;


      public Transition OnExit([NotNull] Action onExit)
      {
        Exit = onExit ?? throw new ArgumentNullException(nameof(onExit));
        return this;
      }
    }
  
    public class Entering : Exiting
    {
      private const string AsyncVoidMethodNotSupported = "'async void' methods are not supported, use Task return type for async method";
      
      internal readonly object State;

      internal Entering([NotNull] object state) => State = state ?? throw new ArgumentNullException(nameof(state)); 

      public Exiting OnEnter([NotNull] Action<IStateMachine> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        if(IsAsyncMethod(onEnter.Method)) throw new InvalidOperationException(AsyncVoidMethodNotSupported);

        Enter = EnterInvoker.Create(onEnter);
        return this;
      }

      public Exiting OnEnter([NotNull] Func<IStateMachine, Task> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        
        Enter = EnterInvoker.Create(onEnter);
        return this;
      }

      public Exiting OnEnter<T>([NotNull] Action<IStateMachine, T> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        if(IsAsyncMethod(onEnter.Method)) throw new InvalidOperationException(AsyncVoidMethodNotSupported);
      
        Enter = EnterInvoker.Create(onEnter);
        return this;
      }

      public Exiting OnEnter<T>([NotNull] Func<IStateMachine, T, Task> onEnter)
      {
        if (onEnter == null) throw new ArgumentNullException(nameof(onEnter));
        
        Enter = EnterInvoker.Create(onEnter);
        return this;
      }
    
      private static bool IsAsyncMethod(MemberInfo method) => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }
  }
}