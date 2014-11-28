using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
	private ObjectManager _ObjectManager;
	public List<Node> pathToDestination = null;
	
	public void InitMap()
	{
		// initialize pathfinding variables
		foreach (Node node in _ObjectManager.Map.nodes) {
			node.gScore = int.MaxValue;
			node.fScore = int.MaxValue;
			node.parent = null;
			node.isInOpenSet = false;
			node.isInClosedSet = false;
			node.checkedByThread = 0;
		}
	}
	
	public List<Node> Astar (Node start, Node goal)
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
		start.fScore = start.gScore + Heuristic_cost_estimate (start, goal);
		
		
		
		while (openSet.Count > 0) {
			// get closest node
			Node current = openSet.RemoveMin ();
			if (current == goal)
				return Reconstruct_path (start, goal);
			current.isInOpenSet = false;
			current.isInClosedSet = true;

			//			
			//			// look at the neighbors of the node
			foreach (Node neighbor in current.getCloseNeighbors()) {
				if(neighbor == null || neighbor.isInClosedSet || !neighbor.isWalkable)
					continue;

					
				// if the new gscore is lower replace it
				float tentativeGscore = current.gScore + 1;
				if (!neighbor.isInOpenSet || tentativeGscore < neighbor.gScore) {
					
					neighbor.parent = current;
					neighbor.gScore = tentativeGscore;
					neighbor.fScore = neighbor.gScore + Heuristic_cost_estimate (neighbor, goal);

					// if neighbor's not in the open set add it
					if (!neighbor.isInOpenSet) {
						openSet.Add (neighbor);
						neighbor.isInOpenSet = true;
						neighbor.isInClosedSet = false;
					}
				}
			}

			foreach (Node neighbor in current.getDiagnalNeighbors()) {
				if(neighbor == null || neighbor.isInClosedSet || !neighbor.isWalkable)
					continue;
				
				
				// if the new gscore is lower replace it
				float tentativeGscore = current.gScore + (float)Math.Sqrt(2);
				if (!neighbor.isInOpenSet || tentativeGscore < neighbor.gScore) {
					
					neighbor.parent = current;
					neighbor.gScore = tentativeGscore;
					neighbor.fScore = neighbor.gScore + Heuristic_cost_estimate (neighbor, goal);
					
					// if neighbor's not in the open set add it
					if (!neighbor.isInOpenSet) {
						openSet.Add (neighbor);
						neighbor.isInOpenSet = true;
						neighbor.isInClosedSet = false;
					}
				}
			}
			
		}
		// Fail
		return null;
	}
	
	void Awake(){
		_ObjectManager = ObjectManager.GetInstance ();
	}
	
	public float Heuristic_cost_estimate (Node start, Node goal)
	{




		float xComponent = Math.Abs ((start.listIndex.x+1) - (goal.listIndex.x+1));
		float zComponent = Math.Abs ((start.listIndex.z+1) - (goal.listIndex.z+1));

		float min = Math.Min (xComponent, zComponent);
		float max = Math.Max (xComponent, zComponent);

		return (float)Math.Sqrt(min)* (float)Math.Sqrt(2)  + (max - min);
		
		float hyp = (float)Math.Sqrt (Math.Pow (xComponent, 2) + Math.Pow (zComponent, 2));
		
		//return hyp * .9f;  // Euclidian
		
		//return ((xComponent + zComponent) * .7);  // Manhattan
	}
	
	
	/// <summary>
	/// Reconstruct_path the specified start and goal.
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	private List<Node> Reconstruct_path (Node start, Node goal)
	{
		List<Node> path = new List<Node> ();
		path.Add (goal);
		
		Node itr = goal;
		while (itr != null && itr.parent != start) {
			path.Add (itr.parent);
			itr = itr.parent;
		}
		return path;
	}
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}