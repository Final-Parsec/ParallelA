using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

public class PathfindingThread
{
	public List<Node> partialPath;
	public List<Node> finalPath;

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
			if (endNode != null){
				while(brotherThread.endNode==null){
				}

				if(id == 1){
					if(endNode != brotherThread.touchedNode){

						List<Node> brotherPath1 = Reconstruct_path (brotherThread.start, touchedNode);
						List<Node> brotherPath2 = Reconstruct_path (brotherThread.start, brotherThread.endNode);

						List<Node> myPath1 = Reconstruct_path (start, endNode);
						List<Node> myPath2 = Reconstruct_path (start, brotherThread.touchedNode);

						int possiblePath1Len = brotherPath1.Count + myPath1.Count;
						int possiblePath2Len = brotherPath2.Count + myPath2.Count;

						if(possiblePath1Len <= possiblePath2Len){
							brotherPath1.Reverse();
							finalPath = brotherPath1.Concat(myPath1).ToList();
						}else{
							brotherPath2.Reverse();
							finalPath = brotherPath2.Concat(myPath2).ToList();
						}
					}else{
						List<Node> brotherPath = Reconstruct_path (brotherThread.start, brotherThread.endNode);
						List<Node> myPath = Reconstruct_path (start, endNode);

						brotherPath.Reverse();
						finalPath = brotherPath.Concat(myPath).ToList();
					}
				}
				done = true;
				return;
			}

			AstarNodeExpansion(openSet, current, current.getCloseNeighbors(), 1);
			//AstarNodeExpansion(openSet, current, current.getDiagnalNeighbors(), (float)Math.Sqrt(2));

			
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
	
	public float Heuristic_cost_estimate (Node goal, Node current)
	{
		
		float dx1 = Math.Abs((current.listIndex.x+1) - (goal.listIndex.x+1));
		float dy1 = Math.Abs((current.listIndex.z+1) - (goal.listIndex.z+1));
		//float dx2 = (start.listIndex.x+1) - (goal.listIndex.x+1);
		//float dy2 = (start.listIndex.z+1) - (goal.listIndex.z+1);
		//float cross = Math.Abs(dx1*dy2 - dx2*dy1);
		
		//float min = Math.Min (dx1, dy1);
		//float max = Math.Max (dx1, dy1);
		
		//return (float)Math.Sqrt(min)*1.4f  + (max - min) ;
		
		return ((dx1 + dy1) * .85f) ;  // Manhattan
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
		path.Add (start);

		return path;
	}
}