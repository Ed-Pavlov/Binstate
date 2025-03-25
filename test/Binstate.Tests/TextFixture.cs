using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;

namespace Binstate.Tests;

[TestFixture]
public class TextFixture
{
  [OneTimeSetUp]
  public void BeforeAllTestsRun()
  {
    Trace.Listeners.Clear();
    Trace.Listeners.Add(new ConsoleTraceListener());
  }
}