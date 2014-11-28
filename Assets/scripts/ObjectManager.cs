using UnityEngine;
using System.Collections.Generic;

public class ObjectManager
{


	public static ObjectManager _ObjectManager{get;set;}



	private Map map;
	public Map Map
	{
		get{
			if(map == null)
				map = GameObject.Find ("Map").GetComponent<Map> ();
			return map;
		}
	}


	private Pathfinding pathfinding;
	public Pathfinding Pathfinding
	{
		get{
			if(pathfinding == null)
				pathfinding = GameObject.Find ("Map").GetComponent<Pathfinding> ();
			return pathfinding;
		}
	}

	public ObjectManager ()
	{
		_ObjectManager = this;
	}
	
	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <returns>The instance.</returns>
	public static ObjectManager GetInstance ()
	{
		if (_ObjectManager == null)
			return new ObjectManager ();
		else
			return _ObjectManager;
	}

}