using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PathfindingThread
{
	public List<Node> partialPath;

	public bool done = false;
	public int id;

	private Node start;
	private Node goal;
	public Node endNode = null;
	public Node touchedNode = null;
	public PathfindingThread brotherThread;



	public PathfindingThread(int id)
	{
		this.id = id;

	}

	public void MakeThread(Node start, Node goal)
	{
		this.start = start;
		this.goal = goal;

		ThreadStart startT = new ThreadStart(Astar);
		Thread thread = new Thread(startT);
		thread.Start();
	}

	private void Astar ()
	{
		if (start == null || goal == null){
			partialPath = null;
			return;
		}
		
		if (start == goal)
		{
			partialPath = new List<Node>()
			{
				start
			};
			return;
		}

		start.checkedByThread = id;
		MinHeap<Node> openSet = new MinHeap<Node>();
		openSet.Add(start);
		start.isInOpenSet = true;
		
		start.gScore = 0;
		start.fScore = start.gScore + Heuristic_cost_estimate (start, goal);
		
		while (openSet.Count > 0) {
			// get closest node
			Node current = openSet.RemoveMin ();
			current.isInOpenSet = false;
			current.isInClosedSet = true;
			
			// if its the goal, return
			if (current == goal || endNode != null){
//				while(brotherThread.endNode==null){
//				}

//				if(id == 1){
//					if(endNode != brotherThread.touchedNode)
//						endNode = brotherThread.touchedNode;
//				}

				if (endNode != null){
					partialPath = Reconstruct_path (start, endNode);
					done = true;
					return;
				}

				partialPath = Reconstruct_path (start, goal);
				done = true;
				return;
			}

			AstarNodeExpansion(openSet, current, current.getCloseNeighbors(), 1);
			AstarNodeExpansion(openSet, current, current.getDiagnalNeighbors(), (float)Math.Sqrt(2));

			
		}
		done = true;
		partialPath = null;
	}

	private void AstarNodeExpansion(MinHeap<Node> openSet, Node current, Node[] neighbors, float cost){

		foreach (Node neighbor in neighbors) {
			if(neighbor == null || neighbor.isInClosedSet || !neighbor.isWalkable)
				continue;
			
//			if(endNode != null)
//				break;
//			
//			if (brotherThread.endNode != null){
//				endNode = brotherThread.touchedNode;
//				break;
//			}
//			
			if(neighbor.checkedByThread != 0 && neighbor.checkedByThread != id){
				endNode = current;
				touchedNode = neighbor;
				break;
			}
			
//			float gScoreHolder = current.gScore;
//			if(neighbor.checkedByThread != 0 && neighbor.checkedByThread != id){
//				gScoreHolder = int.MaxValue;
//			}
			// if the new gscore is lower replace it
			float tentativeGscore = current.gScore + cost;
			if (!neighbor.isInOpenSet || tentativeGscore < neighbor.gScore) {
				
				neighbor.parent = current;
				neighbor.gScore = tentativeGscore;
				neighbor.fScore = neighbor.gScore + Heuristic_cost_estimate (neighbor, goal);
				
				// if neighbor's not in the open set add it
				if (!neighbor.isInOpenSet) {
					neighbor.checkedByThread = id;
					openSet.Add (neighbor);
					neighbor.isInOpenSet = true;
					neighbor.isInClosedSet = false;
				}
			}
		}
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
		while (itr.parent != start) {
			path.Add (itr.parent);
			itr = itr.parent;
		}
		return path;
	}
}