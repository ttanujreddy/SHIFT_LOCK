using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>Editor-only chamber authoring tool. Previews placement, validates bounds, prevents duplicate occupancy, and saves Resources/Levels/Level&lt;id&gt;.txt. Saving writes under Assets and refreshes AssetDatabase, so this is not a runtime editor.</summary>
public class LevelEditor : MonoBehaviour 
{
	[System.NonSerialized]
	public Tile[,] grid;				//Runtime-only tile grid.
	public Vector3 mousePos;			//The world position of the mouse.
	public int rotation;				//The current rotation of the placing tile.
	public SpriteRenderer ghostTile;	//The translucent tile to show you where you are going to place a tile.
	public int curLevel;				//The current level.

	//Tiles
	public int selectedTile;			//The id of the tile you currently have selected.

	public GameObject blockTile;		//Block tile prefab.
	public GameObject playerTile;		//Player prefab.
	public GameObject spikeTile;		//Spike tile prefab.
	public GameObject endFlagTile;		//End flag prefab.

	public bool placedPlayer;			//Has the player been placed?
	public bool placedFlag;				//Has the end flag been placed?

	//UI
	public TMP_Text saveText;			//The text on the save button, displaying what level it will be saved as.

	void Start ()
	{
		NewLevel();
	}

	//Sets up the grid for a new level.
	/// <summary>Creates an empty 20×20 grid and selects the first unused sequential level ID.</summary>
	public void NewLevel ()
	{
		grid = new Tile[20,20];

		//Setting up the grid
		for(int x = 0; x < 20; x++){
			for(int y = 0; y < 20; y++){
				Tile g = new Tile();
				g.x = x;
				g.y = y;
				g.type = TileType.Empty;
				grid[x, y] = g;
			}
		}

		curLevel = GetNextLevelId();
		saveText.text = "Save as: <b>LEVEL " + curLevel + "</b>";
	}

	// Find the first missing sequential Level<number>.txt ID. Counting every TextAsset
	// in Resources/Levels can inflate the number when unrelated text assets are present.
	/// <summary>Finds the lowest unused non-negative Level&lt;id&gt; by checking imported Resources assets and files already on disk.</summary>
	int GetNextLevelId()
	{
		HashSet<int> usedIds = new HashSet<int>();

		foreach (TextAsset asset in Resources.LoadAll<TextAsset>("Levels"))
		{
			int id;
			if (TryParseLevelId(asset.name, out id))
				usedIds.Add(id);
		}

		string levelsPath = Path.Combine(Application.dataPath, "GravityPuzzleGame/Resources/Levels");
		if (Directory.Exists(levelsPath))
		{
			foreach (string filePath in Directory.GetFiles(levelsPath, "Level*.txt"))
			{
				int id;
				if (TryParseLevelId(Path.GetFileNameWithoutExtension(filePath), out id))
					usedIds.Add(id);
			}
		}

		int nextId = 0;
		while (usedIds.Contains(nextId))
			nextId++;

		return nextId;
	}

	/// <summary>Parses names such as <c>Level12</c> into a non-negative internal ID.</summary>
	bool TryParseLevelId(string assetName, out int id)
	{
		id = -1;
		if (string.IsNullOrEmpty(assetName) ||
			!assetName.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase) ||
			assetName.Length <= 5)
			return false;

		return int.TryParse(assetName.Substring(5), out id) && id >= 0;
}

	//Called when one of the tile buttons get pressed.
	//It sends over "selection" which will then be the selected tile to place down.
	/// <summary>Selects a tile ID and updates the translucent placement preview.</summary>
	/// <param name="selection">Serialized tile ID (0–5).</param>
	public void SelectTile (int selection)
	{
		GameObject prefab = GetTile(selection);
		if (prefab == null || ghostTile == null)
			return;

		selectedTile = selection;
		SpriteRenderer prefabRenderer = prefab.GetComponent<SpriteRenderer>();
		if (prefabRenderer != null)
			ghostTile.sprite = ShiftLockTheme.CreateTileSprite(selection, prefabRenderer.sprite);
		ghostTile.color = new Color(1f, 1f, 1f, 0.5f);
	}

	/// <summary>Converts mouse coordinates to grid cells, handles placement/deletion, rotates preview, and blocks edits outside the grid or over UI.</summary>
	void Update ()
	{
		if (Camera.main == null || ghostTile == null || grid == null)
			return;

		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		int cellX = Mathf.FloorToInt(worldMousePos.x / 9f);
		int cellY = Mathf.FloorToInt(worldMousePos.y / 9f);
		mousePos = new Vector3(cellX * 9 + 4, cellY * 9 + 4, 0);

		bool inBounds = IsValidGridIndex(cellX, cellY);
		bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

		// Only edit when the pointer maps to a real grid cell and is not over the UI.
		if (inBounds && !pointerOverUI)
		{
			if (Input.GetMouseButton(0))
				PlaceTile(cellX, cellY);

			if (Input.GetMouseButton(1))
				DeleteTile(cellX, cellY);
		}

		// Press 5 for a gravity switch and 6 for a locked door.
		if (Input.GetKeyDown(KeyCode.Alpha5))
			SelectTile(4);
		if (Input.GetKeyDown(KeyCode.Alpha6))
			SelectTile(5);

		if (Input.GetKeyDown(KeyCode.R))
		{
			rotation = (rotation + 1) % 5;
			ghostTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90 * rotation));
		}

		if (Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene(0);

		ghostTile.transform.position = mousePos;
		ghostTile.color = inBounds
			? new Color(1f, 1f, 1f, 0.5f)
			: new Color(1f, 0.12f, 0.10f, 0.5f);
	}

	/// <summary>Checks grid bounds before any tile array access or edit.</summary>
	bool IsValidGridIndex(int x, int y)
	{
		return grid != null &&
			x >= 0 && x < grid.GetLength(0) &&
			y >= 0 && y < grid.GetLength(1);
	}

	// Delete only a tile inside the grid. Clear its reference after destroying it.
	/// <summary>Destroys the cell's visual GameObject and clears its logical grid reference.</summary>
	void DeleteTile (int x, int y)
	{
		if (!IsValidGridIndex(x, y))
			return;

		Tile cell = grid[x, y];
		if (cell == null || cell.type == TileType.Empty)
			return;

		if (cell.tile != null)
			Destroy(cell.tile);

		cell.type = TileType.Empty;
		cell.tile = null;
	}

	// Place only inside the grid; invalid mouse positions never index the array.
	/// <summary>Places a selected tile in an empty in-bounds cell using the current 90-degree rotation step.</summary>
	void PlaceTile (int x, int y)
	{
		if (!IsValidGridIndex(x, y))
			return;

		GameObject prefab = GetTile(selectedTile);
		if (prefab == null || grid[x, y] == null || grid[x, y].type != TileType.Empty)
			return;

		Vector3 tilePosition = new Vector3(x * 9 + 4, y * 9 + 4, 0);
		GameObject placedTile = Instantiate(prefab, tilePosition, Quaternion.Euler(new Vector3(0, 0, -90 * rotation)));
		ShiftLockTheme.ApplyTile(placedTile, selectedTile);
		grid[x, y].type = (TileType)(selectedTile + 1);
		grid[x, y].tile = placedTile;
	}

	//Retuns a tile prefab based on the given ID number.
	/// <summary>Maps editor IDs to prefabs. Switches and doors reuse the block prefab as a base visual.</summary>
	GameObject GetTile (int selected)
	{
		switch(selected){
			case 0:
				return blockTile;
			case 1:
				return playerTile;
			case 2:
				return spikeTile;
			case 3:
				return endFlagTile;
			case 4: // Gravity switch placeholder
			case 5: // Locked door placeholder
				return blockTile;
		}

		return null;
	}

	//Saves the level to a .txt file and saves it in the resources folder.
	//It converts the tiles, their position and their rotation to an easy to read format for when they
	//are required to be loaded up.
	/// <summary>Serializes non-empty cells to comma-separated records and writes the next Level&lt;id&gt;.txt under Assets/GravityPuzzleGame/Resources/Levels.</summary>
	/// <remarks>Existing files are not overwritten. Spike records include a fourth field for rotation in 90-degree steps.</remarks>
	public void SaveLevel ()
	{
		string t = "";

		for(int x = 0; x < 20; x++){
			for(int y = 0; y < 20; y++){
				Tile g = grid[x,y];

				if(g.type != TileType.Empty){
					t += g.x + "," + g.y + "," + ((int)g.type - 1);

					if(g.type == TileType.Spike){
						float rotation = g.tile.transform.eulerAngles.z / 90;
						int rot = (int)rotation;

						t += "," + rot;
					}

					t += "\n";
				}
			}
		}


		string levelsPath = Path.Combine(Application.dataPath, "GravityPuzzleGame/Resources/Levels");
		Directory.CreateDirectory(levelsPath);
		string targetPath = Path.Combine(levelsPath, "Level" + curLevel + ".txt");

		if (File.Exists(targetPath))
		{
			Debug.LogWarning("File already exists; refusing to overwrite: " + targetPath);
			return;
		}

		File.WriteAllText(targetPath, t);
		Debug.Log("Saved new level: " + targetPath);
#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
#endif
		curLevel++;
		saveText.text = "Save as: <b>LEVEL " + curLevel + "</b>";
	}
}
