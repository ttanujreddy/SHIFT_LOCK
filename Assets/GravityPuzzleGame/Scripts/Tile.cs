using UnityEngine;

// Tile is grid data, not a Unity component. These objects are created with new Tile().
[System.Serializable]
public class Tile
{
	public int x;			//The x position of the tile.
	public int y;			//The y position of the tile.
	public TileType type;	//The type of tile that it is.
	public GameObject tile;	//The gameobject relevant to this tile.
}


