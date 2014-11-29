using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;


public class Map : MonoBehaviour
{

	// Prefabs, GameObjects, and Texturess
	public GameObject obstacle;
	public Transform left;
	public Transform right;
	public Transform enemySpawnTransform;
	public Transform destinationTransform;
	public Node destinationNode;
	public Node spawnNode;
	public bool visualsOn;
	public bool onlySequentialA;
	public bool disableMaze;
	public int datGap;
	public int numDoors;

	// A*
	public List<Node> sequentialAPath;
	private LineRenderer biALineRenderer;
	private LineRenderer seqALineRenderer;

	// BI A*
	PathfindingThread pathThreadStart;
	PathfindingThread pathThreadGoal;

	private Stopwatch stopWatch = new Stopwatch();

	// Grid/Node
	public int size_x;
	public int size_z;
	private Vector2 nodeSize;
	public Node[,] nodes;

	void Awake()
	{

		biALineRenderer = GetComponent<LineRenderer>();
		seqALineRenderer = GameObject.Find ("SeqALR").GetComponent<LineRenderer>();
		nodes = new Node[size_x, size_z];

		BuildNodes ();
		ConnectNodes ();

		if(!disableMaze)
			MakeWalls (0, size_x-1, 0, size_z-1);

		destinationNode = nodes[size_x - 1, 0];
		//destinationNode = nodes[4, size_z - 1];
		spawnNode = nodes[0, size_z - 1];

		destinationNode.isWalkable = true;
		spawnNode.isWalkable = true;

	}

	// Use this for initialization
	void Start ()
	{
		stopWatch.Start();

		sequentialAPath = Pathfinding.Astar (spawnNode, destinationNode);

		stopWatch.Stop();
		TimeSpan ts = stopWatch.Elapsed;
		string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
		                                   ts.Hours, ts.Minutes, ts.Seconds,
		                                   ts.Milliseconds / 10);
		stopWatch.Reset();
		if(sequentialAPath != null){
			UnityEngine.Debug.Log ("Done Sequential A*: "+ elapsedTime + " Len: "+sequentialAPath.Count());
			DrawPath(sequentialAPath, seqALineRenderer);
		}

		if(!onlySequentialA){
			//clear from last run
			InitMap();
				

			pathThreadStart = new PathfindingThread(1);
			pathThreadGoal = new PathfindingThread(2);
			pathThreadStart.brotherThread = pathThreadGoal;
			pathThreadGoal.brotherThread = pathThreadStart;

			stopWatch.Start();
			pathThreadGoal.MakeThread(destinationNode, spawnNode);
			pathThreadStart.MakeThread(spawnNode, destinationNode);
			while(!pathThreadStart.done || !pathThreadGoal.done){

			}

			stopWatch.Stop();
			ts = stopWatch.Elapsed;
			elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
			                                   ts.Hours, ts.Minutes, ts.Seconds,
			                                   ts.Milliseconds / 10);
			stopWatch.Reset();

			if(pathThreadStart.finalPath != null){
				UnityEngine.Debug.Log ("Done Bidirectional A*: "+ elapsedTime + " Len: "+pathThreadStart.finalPath.Count());
				DrawPath (pathThreadStart.finalPath, biALineRenderer);
			}
		}



		if(visualsOn)
			StartCoroutine (makeVisuals());
	}

	IEnumerator makeVisuals()
	{
		int count = 0;
		foreach (Node node in nodes) {
			count++;


			if (!node.isWalkable){
				GameObject wall = Instantiate (obstacle, new Vector3(node.unityPosition.x, -1, node.unityPosition.z), obstacle.transform.rotation) as GameObject;
				wall.renderer.material.color = Color.black;
			}
			else if(node.gScore < float.MaxValue){

				if(onlySequentialA){
					GameObject wall = Instantiate (obstacle, new Vector3(node.unityPosition.x, -1, node.unityPosition.z), obstacle.transform.rotation) as GameObject;
					float ratio = ((float)(node.gScore)/((float)destinationNode.gScore));
					wall.renderer.material.color = new Color(1-ratio,1,0,.5f);
				}else{
					if(node.checkedByThread == 1 && pathThreadGoal.endNode != null){
						GameObject wall = Instantiate (obstacle, new Vector3(node.unityPosition.x, -1, node.unityPosition.z), obstacle.transform.rotation) as GameObject;
						float ratio = ((float)(node.gScore)/((float)pathThreadGoal.endNode.gScore));
						wall.renderer.material.color = new Color(1-ratio,1,0,.5f);
					}
					else if(node.checkedByThread == 2 && pathThreadStart.endNode != null){
						GameObject wall = Instantiate (obstacle, new Vector3(node.unityPosition.x, -1, node.unityPosition.z), obstacle.transform.rotation) as GameObject;
						float ratio = ((float)(node.gScore)/((float)pathThreadStart.endNode.gScore));
						wall.renderer.material.color = new Color(.5f,.2f,1-ratio,.5f);
					}
				}
			}
			

			if(count> 2000){
				yield return new WaitForSeconds(.00001f);
				count = 0;
			}
		}
	}

	public void MakeWalls(int xsmall, int xbig, int zsmall, int zbig){

		if (xsmall < xbig-10 && zsmall < zbig-10){

			//x wall
			int randX = UnityEngine.Random.Range(xsmall, xbig);
			for(int a = zsmall; a <= zbig; a++){
			
				nodes[randX,a].isWalkable = false;
			}

			//z 
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


			MakeWalls(xsmall, randX - datGap, zsmall, randZ - datGap);
			MakeWalls(xsmall, randX - datGap, randZ + datGap, zbig);

			MakeWalls(randX+datGap, xbig, randZ + datGap, zbig);
			MakeWalls(randX+datGap, xbig, zsmall, randZ - datGap);

		}


	}

	public void ClearMap(){
		foreach (Node node in nodes) {
			node.isWalkable = true;
		}
	}

	public void DrawPath(List<Node> path, LineRenderer lineRenderer){

		if(path == null)
			return;

		lineRenderer.SetVertexCount(path.Count);
		for (int x=path.Count-1; x>=0; x--)
			lineRenderer.SetPosition(x, path[x].unityPosition);
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
	/// Blocks the node at the given position.
	/// returns true if the object can be built 
	/// else false	
	/// </summary>
	public bool BlockNode (Vector3 position)
	{
		Node node = GetNodeFromLocation (position);


		node.isWalkable = false;

		return true;
	}

	public void UnBlockNode (Vector3 position)
	{
		Node node = GetNodeFromLocation (position);

		node.isWalkable = true;
	}
	
	public void Update ()
	{
	}

	private void SetPositions ()
	{
		Vector3 midLeft = new Vector3 (0, Screen.height / 2);
		Vector3 midRight = new Vector3 (Screen.width, Screen.height / 2);
		
		midLeft = Camera.main.ScreenToWorldPoint (midLeft);
		midRight = Camera.main.ScreenToWorldPoint (midRight);
		
		midLeft.y = 0;
		midRight.y = 0;
		
		left.transform.position = midLeft;
		right.transform.position = midRight;
		
		enemySpawnTransform.transform.position = midLeft;
		destinationTransform.transform.position = midRight;
		
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
				Vector3 listIndex = new Vector3 (x, 0, z);
				nodes [x, z] = new Node (true, position, listIndex);

			}
		}
	}
	
	/// <summary>
	/// Connects the nodes.
	/// </summary>
	private void ConnectNodes ()
	{
		for (int z=0; z<size_z; z++) {
			for (int x=0; x<size_x; x++) {
				//Debug.Log(x+", "+ y);
				
				if (x - 1 >= 0) {
					nodes [x, z].borderTiles [(int)Border.downRight] = nodes [x - 1, z];
					if (z - 1 >= 0)
						nodes [x, z].borderTiles [(int)Border.Down] = nodes [x - 1, z - 1];
					
					if (z + 1 < nodes.GetLength (1))
						nodes [x, z].borderTiles [(int)Border.Right] = nodes [x - 1, z + 1];
				}
				
				if (x + 1 < nodes.GetLength (0)) {
					nodes [x, z].borderTiles [(int)Border.upLeft] = nodes [x + 1, z];
					if (z - 1 >= 0)
						nodes [x, z].borderTiles [(int)Border.Left] = nodes [x + 1, z - 1];
					
					if (z + 1 < nodes.GetLength (1))
						nodes [x, z].borderTiles [(int)Border.Up] = nodes [x + 1, z + 1];
				}
				
				if (z - 1 >= 0)
					nodes [x, z].borderTiles [(int)Border.downLeft] = nodes [x, z - 1];
				
				if (z + 1 < nodes.GetLength (1))
					nodes [x, z].borderTiles [(int)Border.upRight] = nodes [x, z + 1];
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
		}
	}
}
