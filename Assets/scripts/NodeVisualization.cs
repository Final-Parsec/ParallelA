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
		InvokeRepeating("Visualize", 0, .25f);
	}


	void Visualize () {

		DisableEnableRenderers();
		if(Map.map.selectedNode == onNode){
			gScore.text = "";
			hScore.text = "";
			fScore.text = "";
			color.renderer.material.color = new Color(1,.5f,.25f); // orange
			return;
		}



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
			gScore.text = "G:"+onNode.gScore;
			hScore.text = "H:"+(onNode.fScore-onNode.gScore);
			fScore.text = "F:"+onNode.fScore;

			if (onNode.isInClosedSet)
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
			foreach(int key in PathfindingBidirectionalA.threadIds)
			{
				if(onNode.isInOpenSetOfThread[key] || (int)onNode.checkedByThread == key)
				{

					gScore.text = "G:"+onNode.gScores[key];
					hScore.text = "H:"+(onNode.fScores[key]-onNode.gScores[key]);
					fScore.text = "F:"+onNode.fScores[key];

					if(key == 1)
					{
						if (onNode.checkedByThread == key)
							color.renderer.material.color = Color.red;
						else if(onNode.isInOpenSetOfThread[key])
							color.renderer.material.color = Color.magenta;

					}
					else
					{
						if (onNode.checkedByThread == key)
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

		if(onNode.isCurrent){
			color.renderer.material.color = Color.yellow;
			return;
		}


	}

	public void DisableEnableRenderers()
	{
		bool isInThreadOpenSet = false;
		foreach(int key in PathfindingBidirectionalA.threadIds)
		{
			if(onNode.isInOpenSetOfThread[key])
			{
				isInThreadOpenSet = true;
			}
		}

		if(!onNode.isInOpenSet && !isInThreadOpenSet && !onNode.isInClosedSet && onNode.isWalkable && !onNode.isCurrent && !(Map.map.selectedNode == onNode))
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
