using System;

namespace BeatyBit.Binstate;

public partial class Builder
{
  /// <summary>
  /// Configuration options for the state machine builder.
  /// </summary>
  public class Options
  {
    /// <summary>
    /// Specifies whether the 'default' value can be used as a valid State ID when the type represented by TState
    /// is a <see cref="ValueType"/>, such as <see cref="Enum"/>.
    /// This is necessary if, for some reason, you have to use a type as TState that you did not create and cannot modify.
    /// In all other cases, it is recommended to disallow the use of the default value as a valid value to prevent errors.
    /// </summary>
    public bool AllowDefaultValueAsStateId { get; set; } = false;

    /// <summary>
    /// The mode of transferring arguments to the newly activated states.
    /// See <see cref="BeatyBit.Binstate.ArgumentTransferMode"/> for details.
    /// </summary>
    public ArgumentTransferMode ArgumentTransferMode { get; set; } = ArgumentTransferMode.Strict;
  }
}