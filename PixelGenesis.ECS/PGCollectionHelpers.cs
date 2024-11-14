using System.Runtime.CompilerServices;

namespace PixelGenesis.ECS;

internal static class SortedListAccessor<TKey, TValue> where TKey : notnull
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "values")]
    internal extern static ref TValue[] GetValuesArray(SortedList<TKey, TValue> @this);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "keys")]
    internal extern static ref TKey[] GetKeysArray(SortedList<TKey, TValue> @this);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "comparer")]
    internal extern static ref IComparer<TKey> GetComparer(SortedList<TKey, TValue> @this);
}

public static class PGCollectionHelpers
{    
    public static Span<K> KeysAsSpan<K, V>(this SortedList<K, V> list) where K : notnull
    {
        var backingValueArray = SortedListAccessor<K,V>.GetKeysArray(list);
        return backingValueArray.AsSpan().Slice(0, list.Count);
    }

    public static Span<V> ValuesAsSpan<K,V>(this SortedList<K,V> list) where K : notnull
    {        
        var backingValueArray = SortedListAccessor<K, V>.GetValuesArray(list);
        return backingValueArray.AsSpan().Slice(0, list.Count);
    }

    public static ref V? GetValueRefOrAddDefault<K, V>(this SortedList<K, V?> list, K key, out bool existed) where K : notnull
    {
        var keys = list.KeysAsSpan();
        var comparer = SortedListAccessor<K,V>.GetComparer(list);

        var index = keys.BinarySearch(key, comparer);

        existed = true;
        if (index < 0)
        {
            // this part needs to be optimized later
            existed = false;
            var value = default(V);            
            list.Add(key, value);
            keys = list.KeysAsSpan();
            index = keys.BinarySearch(key, comparer);
        }

        var values = list.ValuesAsSpan();        
        return ref values[index];
    }

}
