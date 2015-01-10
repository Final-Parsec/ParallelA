using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;

public class PathfindingBidirectionalA
{
	public static bool finished = false;
	public static System.Object L = int.MaxValue;
	public static int[] threadIds = new int[]
	{
		1,2
	};
	
	public int id;
	public Node start;
	private Node goal;
	public int F = 0;
	public Node endNode = null;
	public PathfindingBidirectionalA brotherThread;
	public Thread thread;

	//Tracing
	bool goBack;
	int goBackToStep;
	private int GoBackToStep
	{
		set
		{
			goBackToStep = value;
			if( goBackToStep <= 0)
				goBackToStep = 0;
		}
		get
		{
			return goBackToStep;
		}
	}

	public PathfindingBidirectionalA(Node start, Node goal)
	{
		this.start = start;
		this.goal = goal;
		
	}
	
	public void MakeThread(int id)
	{
		this.id = id;
		ThreadStart startT;
		startT = new ThreadStart(Astar);

		thread = new Thread(startT);
		thread.Priority = System.Threading.ThreadPriority.Highest;
		thread.Start();
	}

	private void Astar()
	{
		start.isInOpenSetOfThread[id] = true;
		MinHeap openSet = new MinHeap(start, id);
		openSet.Add(start);

		
		start.gScores[id] = 0;
		start.fScores[id] = start.gScores[id] + Heuristic_cost_estimate (goal, start);

		int numSteps = 0;
		while(!finished)
		{
			Node current = openSet.GetRoot ();
			current.isInOpenSetOfThread[id] = false;
			current.isInClosedSet = true;

			current.isCurrent = true;
			ControlLogic(current, numSteps);
			numSteps++;
			current.isCurrent = false;
			
			if(current.checkedByThread == 0)
			{
				if(current.fScores[id] < (int)L && current.gScores[id] + brotherThread.F - brotherThread.Heuristic_cost_estimate(start, current) < (int)L)
				{
					foreach(Node neighbor in current.getNeighbors()){
						if(neighbor != null && neighbor.isWalkable)
						{
							int cost = Heuristic_cost_estimate(current, neighbor);
							if(neighbor.checkedByThread == 0 && neighbor.gScores[id] > current.gScores[id] + cost)
							{

								neighbor.gScores[id] = current.gScores[id] + cost;
								neighbor.fScores[id] = neighbor.gScores[id] + Heuristic_cost_estimate(goal, neighbor);
								neighbor.parents[id] = current;
								if(!neighbor.isInOpenSetOfThread[id])
								{
									neighbor.isInOpenSetOfThread[id] = true;
									openSet.Add(neighbor);
								}
								else{
									openSet.Reevaluate(neighbor);
								}
								if(neighbor.gScores[brotherThread.id] != int.MaxValue && neighbor.gScores[brotherThread.id]+ neighbor.gScores[id]< (int)L)
								{
									lock(L)
									{
										if(neighbor.gScores[brotherThread.id]+ neighbor.gScores[id]< (int)L)
										{
											L = neighbor.gScores[brotherThread.id]+ neighbor.gScores[id];
											endNode = current;
											brotherThread.endNode = neighbor;
										}
									}
								}
							}
						}
					}
				}
				current.checkedByThread = id;
			}

			if(openSet.Count() > 0)
			{
				F = openSet.Peek().fScores[id];
			}
			else
			{
				finished = true;
			}
		}
	}
	
	public int Heuristic_cost_estimate (Node goal, Node current)
	{
		//Chebyshev distance
		int dx1 = (int)Math.Abs((current.xIndex) - (goal.xIndex));
		int dy1 = (int)Math.Abs((current.yIndex) - (goal.yIndex));
		
		if (dx1 > dy1)
			return 14*dy1 + 10*(dx1-dy1);
		else
			return 14*dx1 + 10*(dy1-dx1);
	}

	private void ControlLogic(Node current, int numSteps)
	{
		if(Map.map.ContinueToSelectedNode && current == Map.map.selectedNode){
			Map.map.ContinueToSelectedNode = false;
		}
		//Wait for user to hit step if in step through mode
		while(Map.map.TraceThroughOn && !Map.map.StepForward && !Map.map.exiting)
		{
			if(goBack)
			{
				if(numSteps == GoBackToStep)
				{
					goBack = false;
				}
				break;
			}
			else if(Map.map.StepBack)
			{
				Map.map.StepBack = false;
				goBack = true;
				GoBackToStep = numSteps-2;
				
				Map.map.InitMap();
				Astar();
				return;
			}
			
			if(Map.map.ContinueToSelectedNode && Map.map.selectedNode != null && Map.map.selectedNode.isWalkable && !Map.map.selectedNode.isInClosedSet)
				break;
		}
		Map.map.StepForward = false;
	}

	public List<Node> Reconstruct_path (Node start, Node goal)
	{
		List<Node> path = new List<Node> ();
		path.Add (goal);
		
		Node itr = goal;

		while (itr.parents[id] != start) {
			if(itr.parents[id] == null)
				break;

			path.Add (itr.parents[id]);
			itr = itr.parents[id];
		}
		path.Add (start);
		
		return path;
	}
}
