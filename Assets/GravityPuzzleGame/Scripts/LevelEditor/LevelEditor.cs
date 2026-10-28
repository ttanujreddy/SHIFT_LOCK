using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
	public void SelectTile (int selection)
	{
		selectedTile = selection;
		ghostTile.sprite = GetTile(selection).GetComponent<SpriteRenderer>().sprite;
	}

	void Update ()
	{
		mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mousePos = new Vector3((int)(mousePos.x / 9) * 9 + 4, (int)(mousePos.y / 9) * 9 + 4, 0);

		//Placing tiles.
		if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()){
			PlaceTile(mousePos);
		}

		//Deleting tiles.
		if(Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject()){
			DeleteTile(mousePos);
		}

		//Rotating tiles.
		if(Input.GetKeyDown(KeyCode.R)){
			if(rotation == 4){
				rotation = 0;
			}else{
				rotation += 1;
			}

			ghostTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90 * rotation));
		}

		//Escaping to menu.
		if(Input.GetKeyDown(KeyCode.Escape)){
			SceneManager.LoadScene(0);
		}

		ghostTile.transform.position = mousePos;

		//If the mouse is outside of the boundry, then the ghost tile will be red.
		if(mousePos.x < 0 || mousePos.y < 0){
			ghostTile.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		}else{
			ghostTile.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}
	}

	//Deletes the tile at the give position ("pos").
	void DeleteTile (Vector3 pos)
	{
		grid[(int)(pos.x / 9), (int)(pos.y / 9)].type = TileType.Empty;
		Destroy(grid[(int)(pos.x / 9), (int)(pos.y / 9)].tile);
	}

	//Places the selected tile at the given position ("pos").
	void PlaceTile (Vector3 pos)
	{
		if(grid[(int)(pos.x / 9), (int)(pos.y / 9)].type == TileType.Empty){
			GameObject tile = Instantiate(GetTile(selectedTile), new Vector3(pos.x, pos.y, 0), Quaternion.Euler(new Vector3(0, 0, -90 * rotation))) as GameObject;
			grid[(int)(pos.x / 9), (int)(pos.y / 9)].type = (TileType)(selectedTile + 1);
			grid[(int)(pos.x / 9), (int)(pos.y / 9)].tile = tile;
		}
	}

	//Retuns a tile prefab based on the given ID number.
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
		}

		return null;
	}

	//Saves the level to a .txt file and saves it in the resources folder.
	//It converts the tiles, their position and their rotation to an easy to read format for when they
	//are required to be loaded up.
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
