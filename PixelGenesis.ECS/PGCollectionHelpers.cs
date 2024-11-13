using System.Runtime.CompilerServices;

namespace PixelGenesis.ECS;

public static class PGCollectionHelpers
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "values")]
    extern static V[] GetValuesArray<K, V>(SortedList<K, V> @this) where K : notnull;

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "keys")]
    extern static K[] GetKeysArray<K, V>(SortedList<K, V> @this) where K : notnull;

    public static Span<K> KeysAsSpan<K, V>(this SortedList<K, V> list) where K : notnull
    {
        var backingValueArray = GetKeysArray(list);
        return backingValueArray.AsSpan();
    }

    public static Span<V> ValuesAsSpan<K,V>(this SortedList<K,V> list) where K : notnull
    {
        var backingValueArray = GetValuesArray(list);
        return backingValueArray.AsSpan();
    }

    public static ref V? GetValueRefOrAddDefault<K, V>(this SortedList<K, V?> list, K key, out bool existed) where K : notnull
    {
        var keys = GetKeysArray(list);

        var index = Array.BinarySearch(keys, key);
                
        if(index < 0)
        {
            // this part needs to be optimized later
            existed = false;
            var value = default(V);
            ref var refValue = ref value;
            list.Add(key, refValue);
            index = Array.BinarySearch(keys, key);
        }

        var values = GetValuesArray(list);
        existed = true;
        return ref values[index];
    }

}
