using UnityEngine;
using System;
using System.Collections.Generic;

public class Node
{
	//TODO: remove borderTiles, unityPosition, and list Index...
	public Node[] borderTiles = new Node[8];
	public Vector3 unityPosition;
	public Vector3 listIndex;

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

	public Node (bool isWalkable, Vector3 unityPosition, Vector3 listIndex)
	{
		this.isWalkable = isWalkable;
		this.unityPosition = unityPosition;
		this.listIndex = listIndex;
	}
	
	public Node clone ()
	{
		return new Node (isWalkable, unityPosition, listIndex);
	}

	public void setUnityPosition (Vector3 unityPosition)
	{
		this.unityPosition = unityPosition;
	}

	public void MakeWalkable ()
	{
		isWalkable = true;
	}

	
	public Node[] getCloseNeighbors ()
	{
		return new Node[4] {borderTiles [(int)Border.downLeft],
								borderTiles [(int)Border.downRight],
								borderTiles [(int)Border.upLeft],
								borderTiles [(int)Border.upRight]};
	}

	public List<Node> getDiagnalWalkableNeighbors ()
	{
		List<Node> returnList = new List<Node>();
		
		if(borderTiles [(int)Border.Left]!=null && borderTiles [(int)Border.Left].isWalkable){
			if(borderTiles [(int)Border.Up]!=null &&borderTiles [(int)Border.Up].isWalkable)
				returnList.Add(borderTiles [(int)Border.upLeft]);
			if(borderTiles [(int)Border.Down]!=null &&borderTiles [(int)Border.Down].isWalkable)
				returnList.Add(borderTiles [(int)Border.downLeft]);
		}

		if(borderTiles [(int)Border.Right]!=null &&borderTiles [(int)Border.Right].isWalkable){
			if(borderTiles [(int)Border.Up]!=null &&borderTiles [(int)Border.Up].isWalkable)
				returnList.Add(borderTiles [(int)Border.upRight]);
			if(borderTiles [(int)Border.Down]!=null &&borderTiles [(int)Border.Down].isWalkable)
				returnList.Add(borderTiles [(int)Border.downRight]);
		}
		
		return returnList;
	}

	public Node[] getDiagnalNeighbors ()
	{
		return new Node[4] {borderTiles [(int)Border.Left],
								borderTiles [(int)Border.Right],
								borderTiles [(int)Border.Down],
								borderTiles [(int)Border.Up]};
	}
}

