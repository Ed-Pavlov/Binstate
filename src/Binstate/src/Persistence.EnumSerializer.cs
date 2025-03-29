using System;

namespace BeatyBit.Binstate;

public partial class Persistence
{
  /// <summary>
  /// Provides serialization and deserialization methods specifically for <see cref="Enum"/> types.
  /// This class is designed to handle the persistence of enum instances by converting them
  /// into a serializable <see cref="Persistence.Item"/> format and vice versa.
  /// </summary>
  public class EnumCustomSerializer : ICustomSerializer
  {
    /// <summary>
    /// Gets the singleton instance of the <see cref="EnumCustomSerializer"/> class.
    /// </summary>
    public static ICustomSerializer Instance { get; } = new EnumCustomSerializer();

    /// <summary>
    /// Serializes an enumeration value into a <see cref="Persistence.Item"/> representation.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the enum to serialize.
    /// Must be an enum type; otherwise, an exception is thrown.
    /// </typeparam>
    /// <param name="value">The enumeration value to serialize.</param>
    /// <returns>A serialized <see cref="Persistence.Item"/> that represents the enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> is not an enum type.</exception>
    public Item Serialize<T>(T value)
    {
      if(value is null) throw new ArgumentNullException(nameof(value));

      var valueType = value.GetType();
      if(! valueType.IsEnum) throw new InvalidOperationException("This serializer can be used only for enum types");
      return new Item(valueType.AssemblyQualifiedName!, value.ToString());
    }

    /// <summary>
    /// Deserializes a <see cref="Persistence.Item"/> back into the corresponding enumeration value.
    /// </summary>
    /// <param name="item">The <see cref="Persistence.Item"/> to deserialize.</param>
    /// <returns>The deserialized enumeration value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="item"/> does not represent an enum type.
    /// </exception>
    public object Deserialize(Item item)
    {
      if(item is null) throw new ArgumentNullException(nameof(item));

      var type = Type.GetType(item.TypeInfo);
      if(type?.IsEnum != true) throw new InvalidOperationException("This serializer can be used only for enum types");
      return Enum.Parse(type, item.Value);
    }
  }
}