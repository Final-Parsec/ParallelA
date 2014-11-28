/// Minimal generic min heap and priority queue implementation in C#, in less than 100 lines of code

using System;
using System.Collections.Generic;

class MinHeap<T> where T : IComparable<T>
{
	private List<T> array = new List<T>();
	
	public void Add(T element)
	{
		array.Add(element);
		int c = array.Count - 1;
		while (c > 0 && array[c].CompareTo(array[c / 2]) == -1)
		{
			T tmp = array[c];
			array[c] = array[c / 2];
			array[c / 2] = tmp;
			c = c / 2;
		}
	}
	
	public T RemoveMin()
	{
		T ret = array[0];
		array[0] = array[array.Count - 1];
		array.RemoveAt(array.Count - 1);
		
		int c = 0;
		while (c < array.Count)
		{
			int min = c;
			if (2 * c + 1 < array.Count && array[2 * c + 1].CompareTo(array[min]) == -1)
				min = 2 * c + 1;
			if (2 * c + 2 < array.Count && array[2 * c + 2].CompareTo(array[min]) == -1)
				min = 2 * c + 2;
			
			if (min == c)
				break;
			else
			{
				T tmp = array[c];
				array[c] = array[min];
				array[min] = tmp;
				c = min;
			}
		}
		
		return ret;
	}
	
	public T Peek()
	{
		return array[0];
	}
	
	public int Count
	{
		get
		{
			return array.Count;
		}
	}
}

class PriorityQueue<T>
{
	internal class HeapNode : IComparable<HeapNode>
	{
		public int Priority;
		public T O;
		public int CompareTo(HeapNode other)
		{
			return Priority.CompareTo(other.Priority);
		}
	}
	
	private MinHeap<HeapNode> minHeap = new MinHeap<HeapNode>();
	
	public void Add(int priority, T element)
	{
		minHeap.Add(new HeapNode() { Priority = priority, O = element });
	}
	
	public T RemoveMin()
	{
		return minHeap.RemoveMin().O;
	}
	
	public T Peek()
	{
		return minHeap.Peek().O;
	}
	
	public int Count
	{
		get
		{
			return minHeap.Count;
		}
	}
}