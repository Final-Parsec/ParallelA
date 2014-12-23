using UnityEngine;
using System.Collections;

public class NodeVisualization : MonoBehaviour {

	public TextMesh gScore;
	public TextMesh hScore;
	public TextMesh fScore;
	public MeshRenderer color;
	public Transform parent;
	private Node onNode;

	void Start () {
		onNode = Map.map.GetNodeFromLocation(transform.position);
	}


	void Update () {

		DisableEnableRenderers();

		if(!onNode.isWalkable)
		{
			color.renderer.material.color = Color.black;
			gScore.text = "";
			hScore.text = "";
			fScore.text = "";
			parent.position = new Vector3(0,-22,0);

			return;
		}

		if(Map.map.RunSequentialAStar)
		{
			gScore.text = ""+onNode.gScore;
			hScore.text = ""+(onNode.fScore-onNode.gScore);
			fScore.text = ""+onNode.fScore;

			if(onNode.isCurrent)
				color.renderer.material.color = Color.yellow;
			else if (onNode.isInClosedSet)
				color.renderer.material.color = Color.red;
			else if(onNode.isInOpenSet)
				color.renderer.material.color = Color.magenta;

			if(onNode.parent != null){
				parent.LookAt(onNode.parent.unityPosition);
				parent.rotation = Quaternion.Euler( new Vector3(90,parent.rotation.eulerAngles.y,parent.rotation.eulerAngles.z));
				parent.position = new Vector3 (onNode.unityPosition.x, onNode.unityPosition.y+2, onNode.unityPosition.z);
			}

		}
		else if(Map.map.RunBidirectionalAStar)
		{
			foreach(int key in PathfindingThread.threadIds)
			{
				if(onNode.isInOpenSetOfThread[key] || (int)onNode.checkedByThread == key)
				{

					gScore.text = ""+onNode.gScores[key];
					hScore.text = ""+(onNode.fScores[key]-onNode.gScores[key]);
					fScore.text = ""+onNode.fScores[key];

					if(key == 1)
					{
						if(onNode.isCurrent)
							color.renderer.material.color = Color.yellow;
						else if (onNode.checkedByThread == key)
							color.renderer.material.color = Color.red;
						else if(onNode.isInOpenSetOfThread[key])
							color.renderer.material.color = Color.magenta;

					}
					else
					{
						if(onNode.isCurrent)
							color.renderer.material.color = Color.yellow;
						else if (onNode.checkedByThread == key)
							color.renderer.material.color = Color.green;
						else if(onNode.isInOpenSetOfThread[key])
							color.renderer.material.color = Color.cyan;
					}

					if(onNode.parents[key] != null)
					{
						parent.LookAt(onNode.parents[key].unityPosition);
						parent.rotation = Quaternion.Euler( new Vector3(90,parent.rotation.eulerAngles.y,parent.rotation.eulerAngles.z));
						parent.position = new Vector3 (onNode.unityPosition.x, onNode.unityPosition.y+2, onNode.unityPosition.z);
					}

				}

			}

		}


	}

	public void DisableEnableRenderers()
	{
		bool isInThreadOpenSet = false;
		foreach(int key in PathfindingThread.threadIds)
		{
			if(onNode.isInOpenSetOfThread[key])
			{
				isInThreadOpenSet = true;
			}
		}


		if(!onNode.isInOpenSet && !isInThreadOpenSet && !onNode.isInClosedSet && onNode.isWalkable && !onNode.isCurrent)
		{
			gScore.renderer.enabled = false;
			hScore.renderer.enabled = false;
			fScore.renderer.enabled = false;
			color.renderer.enabled = false;
			parent.position = new Vector3(0,-22,0);
		}
		else{
			gScore.renderer.enabled = true;
			hScore.renderer.enabled = true;
			fScore.renderer.enabled = true;
			color.renderer.enabled = true;
		}
	}

}