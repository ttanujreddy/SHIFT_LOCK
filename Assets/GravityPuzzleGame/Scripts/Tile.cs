using UnityEngine;

// Tile is grid data, not a Unity component. These objects are created with new Tile().
[System.Serializable]
/// <summary>
/// Plain serializable grid-cell data: grid coordinates, tile type, and optional instantiated GameObject. This class is not a MonoBehaviour.
/// </summary>
public class Tile
{
	public int x;			//The x position of the tile.
	public int y;			//The y position of the tile.
	public TileType type;	//The type of tile that it is.
	public GameObject tile;	//The gameobject relevant to this tile.
}


