using UnityEngine;
using System.Collections;

/// <summary>
/// Caches reusable tile prefabs from Resources. Switches and doors reuse the block prefab and receive different runtime-generated sprites.
/// </summary>
public class Pool : MonoBehaviour 
{
	//This class loads in the prefabs used when loading levels.
	//It stores them here for later use.

	//Tiles
	public static GameObject blockTile;
	public static GameObject playerTile;
	public static GameObject spikeTile;
	public static GameObject endFlagTile;

	/// <summary>
	/// Initialize cached prefabs or the persistent audio source and generated sound clips.
	/// </summary>
	void Awake ()
	{
		//Loading tiles in from the resources folder.
		blockTile = Resources.Load<GameObject>("Prefabs/Block") as GameObject;
		playerTile = Resources.Load<GameObject>("Prefabs/Player") as GameObject;
		spikeTile = Resources.Load<GameObject>("Prefabs/Spike") as GameObject;
		endFlagTile = Resources.Load<GameObject>("Prefabs/EndFlag") as GameObject;
	}

	//Returns a tile prefab relevant to its ID ("type").
	/// <summary>
	/// Return the prefab matching the numeric tile ID, or null if unsupported.
	/// </summary>
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
			// Temporary art: switch/door tiles reuse the block prefab with distinct colors.
			case 4:
			case 5:
				return blockTile;
		}

		return null;
	}
}
