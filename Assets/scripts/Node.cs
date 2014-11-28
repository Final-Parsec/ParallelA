using UnityEngine;
using System;
using System.Collections.Generic;

public class Node : IComparable<Node>
{
	public bool isWalkable;

	// Use border enum to access tiles.
	public Node[] borderTiles = new Node[8];
	public Vector3 unityPosition;
	public Vector3 listIndex;

	// A* variables
	public float gScore = float.MaxValue; // undefined 
	public float fScore = float.MaxValue; // undefined 
	public bool isInOpenSet = false;
	public bool isInClosedSet = false;
	public Node parent = null;
	public int checkedByThread = 0; 

	public int CompareTo(Node obj) {
		if (obj == null) return 1;
		
		Node node = obj as Node;
		if (node != null) 
			return this.fScore.CompareTo(node.fScore);
		else 
			throw new ArgumentException("Object is not a Node");
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

