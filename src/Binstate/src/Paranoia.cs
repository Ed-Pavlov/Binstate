using System;

namespace BeatyBit.Binstate;

internal static class Paranoia
{
  public static InvalidOperationException GetException(string reason)
    => new InvalidOperationException("This exception should never be thrown, because " + reason);

  public static InvalidOperationException GetInvalidTargetException(IState targetState)
    => new InvalidOperationException($"all verifications should be performed in the caller part. Target state = {targetState}");
}