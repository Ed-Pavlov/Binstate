using System;

namespace Binstate.Tests.Util;

public static class WithExtension
{
  public static T With<T>(this T obj, Action<T> action)
  {
    action(obj);
    return obj;
  }
}