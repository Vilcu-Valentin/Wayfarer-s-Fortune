using System;
using System.Collections.Generic;

/// <summary>
/// A simple binary heap-based priority queue.
/// </summary>
/// <typeparam name="TElement">Type of the elements.</typeparam>
/// <typeparam name="TPriority">Type of the priority.</typeparam>
public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TElement Element, TPriority Priority)> heap = new List<(TElement, TPriority)>();

    public int Count => heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        heap.Add((element, priority));
        HeapifyUp(heap.Count - 1);
    }

    public TElement Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("The priority queue is empty.");

        TElement element = heap[0].Element;
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return element;
    }

    public bool Contains(TElement element)
    {
        foreach (var item in heap)
        {
            if (EqualityComparer<TElement>.Default.Equals(item.Element, element))
                return true;
        }
        return false;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (heap[index].Priority.CompareTo(heap[parent].Priority) < 0)
            {
                Swap(index, parent);
                index = parent;
            }
            else
                break;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = heap.Count - 1;
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left <= lastIndex && heap[left].Priority.CompareTo(heap[smallest].Priority) < 0)
                smallest = left;
            if (right <= lastIndex && heap[right].Priority.CompareTo(heap[smallest].Priority) < 0)
                smallest = right;

            if (smallest != index)
            {
                Swap(index, smallest);
                index = smallest;
            }
            else
                break;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}
