using System;

namespace Binstate
{
  public class StateMachineException : Exception
  {
    public StateMachineException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}