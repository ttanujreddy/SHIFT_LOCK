using UnityEngine;
using System.Collections;

/// <summary>Loads base tile prefabs from Resources and maps IDs to prefabs. Switches/doors reuse the block prefab; ShiftLockTheme supplies their distinct runtime artwork.</summary>
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
	/// <summary>Returns the base prefab for a level tile ID, or <see langword="null"/> for an unknown ID.</summary>
	/// <param name="type">0 block, 1 player, 2 spike, 3 exit, 4 gravity switch, 5 locked door.</param>
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
			// Switches and doors share the block prefab; ShiftLockTheme replaces their rendered sprite.
			case 4:
			case 5:
				return blockTile;
		}

		return null;
	}
}
