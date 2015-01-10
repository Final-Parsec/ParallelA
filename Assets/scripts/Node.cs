using UnityEngine;
using System;
using System.Collections.Generic;

public class Node
{
	public Vector3 unityPosition;
	public int xIndex;
	public int yIndex;

	// Used for stepping through the algorithms
	public bool isCurrent;

	// Only A* variables
	public int gScore = int.MaxValue;
	public int fScore = int.MaxValue;
	public bool isInOpenSet = false;
	public Node parent = null;

	// Only Bidirectional A* variables
	public Dictionary<int, int> gScores = new Dictionary<int, int>();
	public Dictionary<int, int> fScores = new Dictionary<int, int>();
	public Dictionary<int, bool> isInOpenSetOfThread = new Dictionary<int, bool>();
	public Dictionary<int, Node> parents = new Dictionary<int, Node>();
	public int checkedByThread = 0; 

	// Common variables for A* and Bidirectional A*
	public bool isInClosedSet = false;
	public bool isWalkable;


	public int CompareTo(Node node) {
		if (node != null) 
		{

			if(this.fScore.CompareTo(node.fScore) == 0)
				return (this.fScore - this.gScore).CompareTo(node.fScore - node.gScore);
			return this.fScore.CompareTo(node.fScore);
		}
		else 
		{
			throw new ArgumentException("Object is not a Node");
		}
	}

	public int CompareTo(Node node, int threadId) {
		if (node != null) 
		{
			if(this.fScores[threadId].CompareTo(node.fScores[threadId]) == 0)
				return (this.fScores[threadId] - this.gScores[threadId]).CompareTo(node.fScores[threadId] - node.gScores[threadId]);
			return this.fScores[threadId].CompareTo(node.fScores[threadId]);
			
		}
		else 
		{
			throw new ArgumentException("Object is not a Node");
		}
	}
	
	public Node ()
	{
	}

	public Node (bool isWalkable, Vector3 unityPosition, int xIndex, int yIndex)
	{
		this.isWalkable = isWalkable;
		this.unityPosition = unityPosition;
		this.xIndex = xIndex;
		this.yIndex = yIndex;
	}
	
	public Node clone ()
	{
		return new Node (isWalkable, unityPosition, xIndex, yIndex);
	}

	public void setUnityPosition (Vector3 unityPosition)
	{
		this.unityPosition = unityPosition;
	}

	public void MakeWalkable ()
	{
		isWalkable = true;
	}
	
	public List<Node> getNeighbors ()
	{
		List<Node> neighbors = new List<Node>();

		for(int x = xIndex - 1; x <= xIndex + 1; x++)
			for(int y = yIndex - 1; y <= yIndex + 1; y++)
				if(x >= 0 && y >= 0 && x < Map.map.size_x && y < Map.map.size_z && (x != xIndex || y != yIndex))
					neighbors.Add(Map.map.nodes[x,y]);

		return neighbors;
	}

}

