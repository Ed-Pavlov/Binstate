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
  /// <returns>A string representation of the serialized value.</returns>
  public string Serialize<T>(T value);

  /// <summary>
  /// Deserializes the specified string into an object instance.
  /// </summary>
  /// <param name="value">The string representing the serialized object.</param>
  /// <returns>An object instance created from the serialized string.</returns>
  public object Deserialize(string value);
}