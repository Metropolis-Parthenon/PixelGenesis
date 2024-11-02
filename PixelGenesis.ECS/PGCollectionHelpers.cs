using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.ECS;

public static class PGCollectionHelpers
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "values")]
    extern static V[] GetValuesArray<K, V>(SortedList<K, V> @this) where K : notnull;


    public static Span<V> ValuesAsSpan<K,V>(this SortedList<K,V> list) where K : notnull
    {
        var backingValueArray = GetValuesArray(list);
        return backingValueArray.AsSpan();
    }
}
