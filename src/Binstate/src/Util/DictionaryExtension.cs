using System;
using System.Collections.Generic;

namespace Binstate;

internal static class DictionaryExtension
{
  public static TValue? GetValueSafe<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
  {
    if(dictionary is null) throw new ArgumentNullException(nameof(dictionary));
    if(key is null) throw new ArgumentNullException(nameof(key));

    return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
  }

  public static TValue GetOrCreateValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createValue)
  {
    if(dictionary is null) throw new ArgumentNullException(nameof(dictionary));
    if(key is null) throw new ArgumentNullException(nameof(key));
    if(createValue is null) throw new ArgumentNullException(nameof(createValue));

    if(! dictionary.TryGetValue(key, out var value))
    {
      value = createValue();
      dictionary.Add(key, value);
    }

    return value;
  }
}