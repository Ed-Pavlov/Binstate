// ReSharper disable All
#pragma warning disable CS1591

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
  public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

  public bool ReturnValue { get; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal  sealed class DoesNotReturnAttribute : Attribute { }