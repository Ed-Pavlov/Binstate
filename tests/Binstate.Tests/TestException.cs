using System;

namespace Binstate.Tests
{
  public class TestException : Exception
  {
    public TestException()
    { }

    public TestException(string message) : base(message)
    { }
  }
}