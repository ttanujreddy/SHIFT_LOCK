using UnityEngine;

/// <summary>Logical grid-cell data shared by runtime/editor. This serializable plain C# object is constructed with <c>new Tile()</c>, not AddComponent. The player's grid cell uses <see cref="TileType.Empty"/> for collision while retaining the player's GameObject reference.</summary>
[System.Serializable]
public class Tile
{
	/// <summary>X coordinate in the logical grid, not world-space units.</summary>
	public int x;
	/// <summary>Y coordinate in the logical grid, not world-space units.</summary>
	public int y;
	/// <summary>Logical tile type used by collision and gameplay rules.</summary>
	public TileType type;
	/// <summary>Visual GameObject for the cell; may be null for empty or player cells.</summary>
	public GameObject tile;
}


