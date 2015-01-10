using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PathfindingSequentialA
{
	public List<Node> pathToDestination = null;
	public Thread thread;
	public bool done = false;

	//Tracing
	int goBackToStep;
	private int GoBackToStep
	{
		set
		{
			goBackToStep = value;
			if( goBackToStep <= 0)
				goBackToStep = 0;
		}
	}
	public bool goBack = false;
	private Node start, goal;

	public PathfindingSequentialA(Node start, Node goal)
	{
		this.start = start;
		this.goal = goal;
	}

	public void MakeThread()
	{
		ThreadStart startT = new ThreadStart(Astar);
		thread = new Thread(startT);
		//thread.Priority = ThreadPriority.Highest;
		thread.Start();
	}

	public void Astar ()
	{
		if (start == null || goal == null){
			done = true;
			return;
		}
		
		if (start == goal)
		{
			done = true;
			return;
		}

		MinHeap openSet = new MinHeap(start);
		start.isInOpenSet = true;
		
		start.gScore = 0;
		start.fScore = start.gScore + Heuristic_cost_estimate (goal, start);
		
		int numSteps = 0;
		Node current = null;
		while (openSet.Count() > 0) {

			current = openSet.GetRoot ();

			current.isCurrent = true;
			current.isInOpenSet = false;
			current.isInClosedSet = true;

			ControlLogic(current, numSteps);
			numSteps++;
			current.isCurrent = false;


			if (current == goal)
			{
				pathToDestination = Reconstruct_path (start, goal);
				done = true;
				return;
			}

			foreach (Node neighbor in current.getNeighbors()) {
				if(neighbor == null || !neighbor.isWalkable || neighbor.isInClosedSet)
					continue;
				
				// if the new gscore is lower replace it
				int tentativeGscore = current.gScore + Heuristic_cost_estimate (current, neighbor);
				
				if (!neighbor.isInOpenSet || tentativeGscore < neighbor.gScore) {
					
					neighbor.parent = current;
					neighbor.gScore = tentativeGscore;
					neighbor.fScore = neighbor.gScore + Heuristic_cost_estimate (goal, neighbor);
					
					if (!neighbor.isInOpenSet){
						openSet.Add (neighbor);
						neighbor.isInOpenSet = true;
					}
					else
					{
						openSet.Reevaluate(neighbor);
					}
				}
			}
		}
		// Fail
		done = true;
		return;
	}


	private void ControlLogic(Node current, int numSteps)
	{
		if(Map.map.TraceThroughOn && Map.map.ContinueToSelectedNode && current == Map.map.selectedNode){
			Map.map.ContinueToSelectedNode = false;
		}
		//Wait for user to hit step if in step through mode
		while(Map.map.TraceThroughOn && !Map.map.StepForward && !Map.map.exiting)
		{
			if(goBack)
			{
				if(numSteps == goBackToStep)
				{
					goBack = false;
				}
				break;
			}
			else if(Map.map.StepBack )
			{
				current.isCurrent = false;
				Map.map.StepBack = false;
				goBack = true;
				GoBackToStep = numSteps-2;
				
				Map.map.InitMap();
				Astar();
				return;
			}
			
			if(Map.map.ContinueToSelectedNode && Map.map.selectedNode != null && Map.map.selectedNode.isWalkable)
				break;
		}
		Map.map.StepForward = false;
	}

	public int Heuristic_cost_estimate (Node goal, Node current)
	{

		int dx1 = (int)Math.Abs((current.xIndex) - (goal.xIndex));
		int dy1 = (int)Math.Abs((current.yIndex) - (goal.yIndex));

		if (dx1 > dy1)
			return 14*dy1 + 10*(dx1-dy1);
		else
			return 14*dx1 + 10*(dy1-dx1);
	}
	
	
	/// <summary>
	/// Reconstruct_path the specified start and goal.
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	public static List<Node> Reconstruct_path (Node start, Node goal)
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