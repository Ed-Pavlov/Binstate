using System.Collections.Generic;

namespace Binstate
{
  public class TransitionConfig
  {
    internal readonly List<Transition> Transitions = new List<Transition>();

    public TransitionConfig AddTransition(object trigger, object state)
    {
      Transitions.Add(new Transition(trigger, null, state, false));
      return this;
    }

    public TransitionConfig AddTransition<TParameter>(object trigger, object state, bool parameterCanBeNull = false)
    {
      Transitions.Add(new Transition(trigger, null, state, parameterCanBeNull));
      return this;
    }
  }
}