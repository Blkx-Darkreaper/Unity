using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PriorityQueue<K, V> where K : IComparable
{
    public int Count { get { return storage.Count; } }
    protected LinkedList<KeyValuePair<K, V>> storage { get; set; }

    public PriorityQueue()
    {
        this.storage = new LinkedList<KeyValuePair<K, V>>();
    }

    public void Enqueue(K key, V value) {
        KeyValuePair<K, V> pair = new KeyValuePair<K, V>(key, value);

        if (Count == 0)
        {
            storage.AddLast(pair);
            return;
        }

        LinkedListNode<KeyValuePair<K, V>> previousNode = null;
        LinkedListNode<KeyValuePair<K, V>> nextNode = storage.First;

        while (nextNode != null)
        {
            K keyToCompare = nextNode.Value.Key;
            int comparison = key.CompareTo(keyToCompare);
            if (comparison < 0)
            {
                break;
            }

            previousNode = nextNode;
            nextNode = previousNode.Next;
        }

        if (previousNode != null)
        {
            storage.AddAfter(previousNode, pair);
        }
        else
        {
            storage.AddBefore(nextNode, pair);
        }
    }

    public V Peek()
    {
        if (Count == 0)
        {
            return default(V);
        }

        KeyValuePair<K, V> pair = storage.First();
        return pair.Value;
    }

    public V Dequeue()
    {
        if (Count == 0)
        {
            return default(V);
        }

        KeyValuePair<K, V> pair = storage.First();
        storage.RemoveFirst();

        return pair.Value;
    }

    public override string ToString()
    {
        string output = string.Empty;
        if (storage.Count == 0)
        {
            return output;
        }

        LinkedListNode<KeyValuePair<K, V>> nextNode = storage.First;
        while (nextNode != null)
        {
            KeyValuePair<K, V> pair = nextNode.Value;
            output += string.Format("<{0}, {1}>\n", pair.Key.ToString(), pair.Value.ToString());
        }

        return output;
    }
}