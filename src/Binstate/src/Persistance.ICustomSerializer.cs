namespace BeatyBit.Binstate;

/// <summary>
/// Defines serialization and deserialization methods for custom data handling.
/// </summary>
public interface ICustomSerializer
{
  /// <summary>
  /// Serializes the specified value into its string representation.
  /// </summary>
  /// <typeparam name="T">The type of the value to be serialized.</typeparam>
  /// <param name="value">The value to serialize.</param>
  public Persistence.Item Serialize<T>(T value);

  /// <summary>
  /// Deserializes the specified string into an object instance.
  /// </summary>
  /// <returns>An object instance created from the serialized string.</returns>
  public object Deserialize(Persistence.Item item);
}