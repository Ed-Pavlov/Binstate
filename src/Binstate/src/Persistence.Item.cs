using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

public partial class Persistence
{
  /// <summary>
  /// Represents a serializable item with type information and value.
  /// Used to persist state machine state or arguments.
  /// </summary>
  public class Item
  {
    /// <inheritdoc cref="Item"/>
    public Item(string typeInfo, string value)
    {
      TypeInfo = typeInfo;
      Value    = value;
    }

    /// <summary>
    /// String description of the <see cref="Value"/> type.
    /// </summary>
    public string TypeInfo { get; set; }

    /// <summary>
    /// Serialized value.
    /// </summary>
    public string Value { get; set; }
  }

  internal static Item CreateItem<T>(T value, ICustomSerializer? customSerializer)
  {
    if(value is null) throw new ArgumentNullException(nameof(value));

    var valueType = value.GetType();

    if(( valueType.IsPrimitive || valueType == typeof(string) ) && valueType.FullName is not null)
      return new Item(valueType.FullName, value.ToString());

    if(customSerializer is null)
      throw new InvalidOperationException("If not primitive types used as State ID or State argument, then custom serializer must be provided");

    return customSerializer.Serialize(value);
  }

  private static object Deserialize(this Item item, ICustomSerializer? customSerializer)
  {
    if(item.IsPrimitive(out var type))
    {
      var converter = TypeDescriptor.GetConverter(type);
      return converter.ConvertFromString(item.Value) ?? throw Paranoia.GetException($"Failed to deserialize {item.Value} to {type}");
    }

    if(customSerializer is null)
      throw new InvalidOperationException("If not primitive types used as arguments or state ID, then the valid custom serializer must be provided");

    return customSerializer.Deserialize(item);
  }

  private static bool IsPrimitive(this Item item, [NotNullWhen(true)] out Type? type)
  {
    type = Type.GetType(item.TypeInfo);
    return type is not null && ( type.IsPrimitive || type == typeof(string) );
  }
}