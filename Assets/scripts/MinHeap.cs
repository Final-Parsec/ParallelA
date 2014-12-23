using System.Collections.Generic;
using System;

public class MinHeap
{
	//The heap is treated like it is 1 based not 0
	public List<Node> heap;
	private object threadId;
	
	public MinHeap (Node root)
	{
		threadId = null;
		heap = new List<Node> ();
		heap.Add (root);
	}

	public MinHeap (Node root, int threadId)
	{
		this.threadId = threadId;
		heap = new List<Node> ();
		heap.Add (root);
	}
	
	/// <summary>
	/// Gets the root.
	/// </summary>
	/// <returns>The root.</returns>
	public Node GetRoot ()
	{
		Node root = heap [0];
		heap [0] = heap [heap.Count - 1];
		heap.RemoveAt (heap.Count - 1);
		MinHeapify (0);
		return root;
	}

	
	/// <summary>
	/// Adds an element to the heap and bubbles up.
	/// </summary>
	/// <param name="element">element to add</param>
	public void Add (Node element)
	{
		heap.Add (element); 
		BubbleUp(heap.Count - 1);
	}

	private void BubbleUp(int index)
	{
		if (index >= heap.Count)
			return;

		int child = index;
		while (child > 0) {
			
			int parent = (child + 1) / 2 - 1;

			if(threadId==null)
			{
				if (heap[parent].CompareTo(heap[child]) < 0)
					break;
			}
			else
			{
				if (heap[parent].CompareTo(heap[child], (int)threadId) < 0)
					break;
			}
			
			Swap (parent, child);
			
			child = parent;
		}
	}
	
	/// <summary>
	/// Mins the heapify.
	/// </summary>
	/// <param name="index">Index.</param>
	public void MinHeapify (int index)
	{
		int left = 2 * (index+1) - 1;
		int right = 2 * (index+1) - 1 + 1;
		int smallest;


		if(threadId==null)
		{
			if (left < heap.Count && heap [left].CompareTo(heap [index]) < 0)
				smallest = left;
			else
				smallest = index;
			
			if (right < heap.Count && heap [right].CompareTo(heap [smallest]) < 0)
				smallest = right;
			
			if (smallest != index) {
				Swap (index, smallest);
				MinHeapify (smallest);
			}
		}
		else
		{
			if (left < heap.Count && heap [left].CompareTo(heap [index], (int)threadId) < 0)
				smallest = left;
			else
				smallest = index;
			
			if (right < heap.Count && heap [right].CompareTo(heap [smallest], (int)threadId) < 0)
				smallest = right;
			
			if (smallest != index) {
				Swap (index, smallest);
				MinHeapify (smallest);
			}
		}
		
		

	}
	
	/// <summary>
	/// Swap the specified list items, a and b.
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The beta component.</param>
	private void Swap (int a, int b)
	{
		Node temp = heap [a];
		heap [a] = heap [b];
		heap [b] = temp;
	}

	public void Reevaluate(Node element)
	{
		int index = heap.IndexOf(element);

		BubbleUp(index);
	}

	public Node Peek()
	{
		if (heap.Count <= 0)
			return null;

		return heap[0];
	}

	public int Count()
	{
		return heap.Count;
	}
}

