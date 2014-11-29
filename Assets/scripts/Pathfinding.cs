using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding
{
	public List<Node> pathToDestination = null;

	public static List<Node> Astar (Node start, Node goal)
	{
		if (start == null || goal == null)
			return null;
		
		if (start == goal)
		{
			return new List<Node>()
			{
				start
			};
		}
		
		// initialize pathfinding variables
		//		foreach (Node node in _ObjectManager.Map.nodes) {
		//			node.gScore = float.MaxValue;
		//			node.fScore = float.MaxValue;
		//			node.parent = null;
		//			node.isInOpenSet = false;
		//			node.isInClosedSet = false;
		//		}
		
		MinHeap<Node> openSet = new MinHeap<Node>();
		openSet.Add(start);
		start.isInOpenSet = true;
		
		start.gScore = 0;
		start.fScore = start.gScore + Heuristic_cost_estimate (start, goal, start);
		
		
		
		while (openSet.Count > 0) {
			// get closest node
			Node current = openSet.RemoveMin ();
			if (current == goal)
				return Reconstruct_path (start, goal);
			current.isInOpenSet = false;
			current.isInClosedSet = true;

			AstarNodeExpansion(start, goal, openSet, current, current.getCloseNeighbors(), 1f);
			//AstarNodeExpansion(start, goal, openSet, current, current.getDiagnalNeighbors(), (float)Math.Sqrt(2));
			
		}
		// Fail
		return null;
	}

	private static void AstarNodeExpansion(Node start, Node goal, MinHeap<Node> openSet, Node current, Node[] neighbors, float cost){
		
		foreach (Node neighbor in neighbors) {
			if(neighbor == null || !neighbor.isWalkable)
				continue;

			// if the new gscore is lower replace it
			float tentativeGscore = current.gScore + cost;
			if (!neighbor.isInOpenSet && tentativeGscore < neighbor.gScore) {
				
				neighbor.parent = current;
				neighbor.gScore = tentativeGscore;
				neighbor.fScore = neighbor.gScore + Heuristic_cost_estimate (start, goal, neighbor);

				if (!neighbor.isInOpenSet){
					openSet.Add (neighbor);
					neighbor.isInOpenSet = true;
					neighbor.isInClosedSet = false;
				}
			}
		}
	}

	public static float Heuristic_cost_estimate (Node start, Node goal, Node current)
	{

		float dx1 = Math.Abs((current.listIndex.x+1) - (goal.listIndex.x+1));
		float dy1 = Math.Abs((current.listIndex.z+1) - (goal.listIndex.z+1));
		//float dx2 = (start.listIndex.x+1) - (goal.listIndex.x+1);
		//float dy2 = (start.listIndex.z+1) - (goal.listIndex.z+1);
		//float cross = Math.Abs(dx1*dy2 - dx2*dy1);

		//float min = Math.Min (dx1, dy1);
		//float max = Math.Max (dx1, dy1);

		//return (float)Math.Sqrt(min)*1.4f  + (max - min) ;
		
		return ((dx1 + dy1) * .8f);  // Manhattan
	}
	
	
	/// <summary>
	/// Reconstruct_path the specified start and goal.
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	private static List<Node> Reconstruct_path (Node start, Node goal)
	{
		List<Node> path = new List<Node> ();
		path.Add (goal);
		
		Node itr = goal;
		while (itr != null && itr.parent != start) {
			path.Add (itr.parent);
			itr = itr.parent;
		}
		path.Add (start);

		return path;
	}
}