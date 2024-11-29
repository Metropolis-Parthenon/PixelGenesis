using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.ECS.DataStructures;

public sealed class LRUCache<K, T>(int capacity, IEqualityComparer<K>? comparer = default, Action<T>? onDestroy = default) : IEnumerable<KeyValuePair<K, T>> where K : notnull
{
    Dictionary<K, LinkedListNode<KeyValuePair<K, T>>> Dictionary
        = new Dictionary<K, LinkedListNode<KeyValuePair<K, T>>>(capacity, comparer ?? EqualityComparer<K>.Default);

    LinkedList<KeyValuePair<K, T>> List = new LinkedList<KeyValuePair<K, T>>();

    SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

    public void Add(K key, T value)
    {
        GetOrAdd(key, (_) => value);
    }

    public async ValueTask<T> GetOrAddAsync(K key, Func<K, CancellationToken, ValueTask<T>> factory, CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            lock (Dictionary)
            {
                if (Dictionary.TryGetValue(key, out var node))
                {
                    MoveToFirst(node);

                    return node.Value.Value;
                }
            }
            var value = await factory(key, cancellationToken);
            return GetOrAdd(key, (_) => value);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public T GetOrAdd(K key, Func<K, T> factory)
    {
        lock (Dictionary)
        {
            if (Dictionary.TryGetValue(key, out var node))
            {
                MoveToFirst(node);

                return node.Value.Value;
            }
            else
            {
                if (List.Count == capacity)
                {
                    var last = List.Last;

                    if (last is not null)
                    {
                        Dictionary.Remove(last.Value.Key, out _);
                        onDestroy?.Invoke(last.Value.Value);
                    }
                    RemoveLast();
                }
                var newNode = new LinkedListNode<KeyValuePair<K, T>>(new KeyValuePair<K, T>(key, factory(key)));
                AddFirst(newNode);
                Dictionary.TryAdd(key, newNode);

                return newNode.Value.Value;
            }
        }
    }

    void AddFirst(LinkedListNode<KeyValuePair<K, T>> node)
    {
        List.AddFirst(node);
    }

    void MoveToFirst(LinkedListNode<KeyValuePair<K, T>> node)
    {
        List.Remove(node);
        List.AddFirst(node);
    }

    void RemoveLast()
    {
        List.RemoveLast();
    }

    public IEnumerator<KeyValuePair<K, T>> GetEnumerator()
    {
        return Dictionary.Select(x => new KeyValuePair<K, T>(x.Key, x.Value.Value.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

