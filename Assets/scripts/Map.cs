using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;


public class Map : MonoBehaviour
{

	public static Map map;

	// Prefabs, GameObjects, and Textures
	public RectTransform UiPanel;
	public GameObject nodeVisual;
	public Transform left;
	public Transform right;
	public Transform enemySpawnTransform;
	public Transform destinationTransform;
	public Node destinationNode;
	public Node spawnNode;
	private bool traceThroughOn = false;
	public bool TraceThroughOn
	{ get
		{
			return traceThroughOn;
		}
		set
		{
			if(value)
				traceThroughOn = !traceThroughOn;
		}
	}
	public bool StepForward{ get; set; }
	public bool StepBack{ get; set; }
	public bool ContinueToSelectedNode{ get; set; }
	public bool Repeat{ get; set; }
	public bool VisualsOn{ get; set; }
	private bool runSequentialAStar = true;
	public bool RunSequentialAStar
	{ get
		{
			return runSequentialAStar;
		}
		set
		{
			if(value)
				runSequentialAStar = !runSequentialAStar;
			else
				runSequentialAStar = false;
		}
	}
	private bool runBidirectionalAStar = false;
	public bool RunBidirectionalAStar
	{ get
		{
			return runBidirectionalAStar;
		}
		set
		{
			if(value)
				runBidirectionalAStar = !runBidirectionalAStar;
			else
				runBidirectionalAStar = false;
		}
	}
	public bool DisableMaze{ get; set; }
	public bool GenerateNewMap{ get; set; }
	public int datGap;
	public int numDoors;

	private bool moveBack;
	public Node selectedNode;

	// A*
	public List<Node> sequentialAPath;
	private LineRenderer lineRenderer;
	PathfindingSequentialA sequentialA;

	// BI A*
	PathfindingBidirectionalA bidirectionalStart;
	PathfindingBidirectionalA bidirectionalGoal;
	
//	private Stopwatch stopWatch = new Stopwatch();

	// Grid/Node
	public int size_x;
	public int size_z;
	private Vector2 nodeSize;
	public Node[,] nodes;

	public bool exiting = false;

	void Awake()
	{
		map = this;
		
		lineRenderer = GetComponent<LineRenderer>();
		nodes = new Node[size_x, size_z];
		
		BuildNodes ();
		
		if(!DisableMaze)
			MakeWalls (0, size_x-1, 0, size_z-1);
		
		destinationNode = nodes[size_x - 1, 0];
		spawnNode = nodes[0, size_z - 1];
		
		destinationNode.isWalkable = true;
		spawnNode.isWalkable = true;
	}

	// Use this for initialization
	void Start ()
	{

		// Initial state
		StepForward = false;
		StepBack = false;
		ContinueToSelectedNode = false;
		Repeat = false;
		VisualsOn = true;
		DisableMaze = false;
		GenerateNewMap = false;
		sequentialA = new PathfindingSequentialA(spawnNode, destinationNode);

		bidirectionalStart = new PathfindingBidirectionalA(spawnNode, destinationNode);
		bidirectionalGoal = new PathfindingBidirectionalA(destinationNode, spawnNode);
		bidirectionalStart.brotherThread = bidirectionalGoal;
		bidirectionalGoal.brotherThread = bidirectionalStart;

		InitMap();
		
		StartCoroutine (makeVisuals());

		if (RunSequentialAStar)
			StartCoroutine (RunSequentialA ());
		else if (RunBidirectionalAStar)
			StartCoroutine (RunBidirectionalA ());

	}

	void Update()
	{
		if(Input.GetMouseButtonUp(0) && (Input.mousePosition.x > UiPanel.rect.size.x || Screen.height - Input.mousePosition.y > UiPanel.rect.size.y))
		{
			selectedNode = GetNodeFromLocation(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}

		if(GenerateNewMap)
		{
			GenerateNewMap=false;

			foreach(Node node in nodes)
				node.isWalkable = true;

			if(!DisableMaze)
				MakeWalls (0, size_x-1, 0, size_z-1);

			destinationNode = nodes[size_x - 1, 0];
			spawnNode = nodes[0, size_z - 1];
			
			destinationNode.isWalkable = true;
			spawnNode.isWalkable = true;

			sequentialA = new PathfindingSequentialA(spawnNode, destinationNode);
			bidirectionalStart = new PathfindingBidirectionalA(spawnNode, destinationNode);
			bidirectionalGoal = new PathfindingBidirectionalA(destinationNode, spawnNode);
			bidirectionalStart.brotherThread = bidirectionalGoal;
			bidirectionalGoal.brotherThread = bidirectionalStart;

			Repeat = true;
		}

		if(Repeat)
		{
			Repeat = false;

			JoinThreads();
			InitMap();

			if (RunSequentialAStar)
				StartCoroutine (RunSequentialA ());
			else if (RunBidirectionalAStar)
				StartCoroutine (RunBidirectionalA ());
		}
	}

	IEnumerator RunSequentialA()
	{

		sequentialA.MakeThread();
		while(!sequentialA.done)
		{
			yield return new WaitForSeconds(.00001f);
		}
		DrawPath( sequentialA.pathToDestination, lineRenderer);

	}

	IEnumerator RunBidirectionalA()
	{

		bidirectionalStart.MakeThread(PathfindingBidirectionalA.threadIds[0]);
		bidirectionalGoal.MakeThread(PathfindingBidirectionalA.threadIds[1]);
		while(!PathfindingBidirectionalA.finished)
		{
			yield return new WaitForSeconds(.00001f);
		}

		if(bidirectionalStart.endNode != null && bidirectionalGoal.endNode != null)
		{
			List<Node> fistHalf = bidirectionalStart.Reconstruct_path (spawnNode, bidirectionalStart.endNode);
			List<Node> secondHalf = bidirectionalGoal.Reconstruct_path (destinationNode, bidirectionalGoal.endNode);
			secondHalf.Reverse();
			List<Node> finalPath = secondHalf.Concat(fistHalf).ToList();
			DrawPath( finalPath, lineRenderer);
		}
		
	}

	IEnumerator makeVisuals()
	{
		int count = 0;
		foreach (Node node in nodes) {
			count++;
			Instantiate (nodeVisual, new Vector3(node.unityPosition.x, 1, node.unityPosition.z), nodeVisual.transform.rotation);
			if(count> 100){
				yield return new WaitForSeconds(.00001f);
				count = 0;
			}
		}
	}

	public void MakeWalls(int xsmall, int xbig, int zsmall, int zbig){

		if (xsmall > xbig-1-datGap || zsmall > zbig-1-datGap)
			return;

		//x wall
		int randX = UnityEngine.Random.Range(xsmall, xbig);
		for(int a = zsmall; a <= zbig; a++){
		
			nodes[randX,a].isWalkable = false;
		}

		//z wall
		int randZ = UnityEngine.Random.Range(zsmall, zbig);
		for(int a = xsmall; a <= xbig; a++){
			
			nodes[a,randZ].isWalkable = false;
		}

		// make openings in the walls. one opening in each section off wall for every loop
		for (int a = 0; a<numDoors ; a++){
			nodes[randX, UnityEngine.Random.Range(zsmall, randZ)].isWalkable = true;
			nodes[randX, UnityEngine.Random.Range(randZ+1, zbig)].isWalkable = true;
			nodes[UnityEngine.Random.Range(xsmall, randX), randZ].isWalkable = true;
			nodes[UnityEngine.Random.Range(randX + 1, xbig), randZ].isWalkable = true;
		}


		MakeWalls(xsmall, randX - 1, zsmall, randZ - 1);
		MakeWalls(xsmall, randX - 1, randZ, zbig);

		MakeWalls(randX + 1, xbig, randZ, zbig);
		MakeWalls(randX + 1, xbig, zsmall, randZ - 1);



	}

	public void ClearMap(){
		foreach (Node node in nodes) {
			node.isWalkable = true;
		}
	}

	private void JoinThreads()
	{
		exiting = true;
		if (sequentialA != null && sequentialA.thread != null)
			sequentialA.thread.Join();
		
		if(bidirectionalStart != null && bidirectionalStart.thread != null)
			bidirectionalStart.thread.Join();
		if(bidirectionalGoal != null && bidirectionalGoal.thread != null)
			bidirectionalGoal.thread.Join();
		
		exiting = false;
	}

	void OnApplicationQuit()
	{
		
		JoinThreads();
		map = null;
	}

	public void DrawPath(List<Node> path, LineRenderer lineRenderer){

		if(path == null)
		{
			lineRenderer.SetVertexCount(0);
			return;
		}

		lineRenderer.SetVertexCount(path.Count);
		for (int x=path.Count-1; x>=0; x--)
		{
			if(path[x] != null)
				lineRenderer.SetPosition(x, new Vector3(path[x].unityPosition.x, path[x].unityPosition.y + 5, path[x].unityPosition.z));
		}
	}

	/// <summary>
	/// Gets the tile from location.
	/// </summary>
	public Node GetNodeFromLocation (Vector3 location)
	{
		
		int xIndex = (int)Mathf.Floor ((location.x - left.position.x) / nodeSize.x);
		int zIndex = size_z + ((int)Mathf.Floor ((location.z - left.position.z) / nodeSize.y));

		// out of bounds check
		if (zIndex >= size_z || zIndex < 0 || xIndex >= size_x || xIndex < 0)
			return null;

		
		return nodes [xIndex, zIndex];
	}
	
	/// <summary>
	/// Initializes nodes that make up the map.
	/// </summary>
	private void BuildNodes ()
	{
		
		float mapSizeX = (right.position.x - left.position.x);
		float mapSizwZ = (left.position.z - right.position.z);
		
		nodeSize = new Vector2 (mapSizeX / size_x, mapSizwZ / size_z);
		float xPos;
		float zPos;
		for (int x=0; x<size_x; x++) {
			for (int z=0; z<size_z; z++) {
				xPos = left.position.x + (x * nodeSize.x);
				zPos = right.position.z + ((z + 1) * nodeSize.y);
				Vector3 position = new Vector3 (xPos + nodeSize.x / 2, 0, zPos - nodeSize.y / 2);
				nodes [x, z] = new Node (true, position, x, z);
			}
		}
	}

	public void InitMap()
	{
		// initialize pathfinding variables
		foreach (Node node in nodes) {
			node.gScore = int.MaxValue;
			node.fScore = int.MaxValue;
			node.parent = null;
			node.isInOpenSet = false;
			node.isInClosedSet = false;
			node.checkedByThread = 0;
			node.isCurrent = false;

			foreach(int id in PathfindingBidirectionalA.threadIds)
			{
				if(!node.gScores.ContainsKey(id))
					node.gScores.Add(id,int.MaxValue);
				if(!node.fScores.ContainsKey(id))
					node.fScores.Add(id,int.MaxValue);
				if(!node.isInOpenSetOfThread.ContainsKey(id))
					node.isInOpenSetOfThread.Add(id, false);
				if(!node.parents.ContainsKey(id))
					node.parents.Add(id, null);

				node.gScores[id] = int.MaxValue;
				node.fScores[id] = int.MaxValue;
				node.isInOpenSetOfThread[id] = false;
				node.parents[id] = null;
			}

			PathfindingBidirectionalA.finished = false;
			PathfindingBidirectionalA.L = int.MaxValue;

		}
	}
}
