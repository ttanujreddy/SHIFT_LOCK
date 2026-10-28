using UnityEngine;
using System.Collections;

public class Pool : MonoBehaviour 
{
	//This class loads in the prefabs used when loading levels.
	//It stores them here for later use.

	//Tiles
	public static GameObject blockTile;
	public static GameObject playerTile;
	public static GameObject spikeTile;
	public static GameObject endFlagTile;

	void Awake ()
	{
		//Loading tiles in from the resources folder.
		blockTile = Resources.Load<GameObject>("Prefabs/Block") as GameObject;
		playerTile = Resources.Load<GameObject>("Prefabs/Player") as GameObject;
		spikeTile = Resources.Load<GameObject>("Prefabs/Spike") as GameObject;
		endFlagTile = Resources.Load<GameObject>("Prefabs/EndFlag") as GameObject;
	}

	//Returns a tile prefab relevant to its ID ("type").
	public static GameObject GetTile (int type)
	{
		switch(type){
			case 0:
				return blockTile;
			case 1:
				return playerTile;
			case 2:
				return spikeTile;
			case 3:
				return endFlagTile;
		}

		return null;
	}
}
