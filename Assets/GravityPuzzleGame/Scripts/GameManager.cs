using UnityEngine;
using System.Collections;
using System.IO;

public class GameManager : MonoBehaviour 
{
	//Level
	public int levelId;									//The current level.
	public int levels;									//The total amount of levels there are.
	[System.NonSerialized]
	public Tile[,] grid;								//Runtime-only tile grid.
	public Vector2 playerSpawn;							//The position where the player will spawn.
	public GravityDirection gravityDirection;			//The current direction of gravity.
	public ParticleSystem levelCompleteParticleEffect;	//The particle effect that will be played once the player completes a level.

	public Player player;								//The player class.
	public CameraController cameraController;			//The camera controller class.
	public GameUI ui;									//The game ui class.

	void Start ()
	{
		//When we load in, we set levelId to be the level that the player selected in the menu.
		levelId = PlayerPrefs.GetInt("SelectedLevel");

		//Then we load the level.
		LoadLevel();
		ui.manager = this;
	}

	//This function loads the level. The level it loads is based on what the levelId is.
	//It reads the level text file and builds the level based on that.
	public void LoadLevel ()
	{
		TextAsset levelAsset = Resources.Load<TextAsset>("Levels/Level" + levelId);
		if (levelAsset == null)
		{
			Debug.LogError("Could not load level " + levelId + ". Expected Resources/Levels/Level" + levelId + ".txt.");
			return;
		}

		grid = new Tile[20, 20];
		string[] lines = levelAsset.text.Split('\n');
		ui.levelText.text = "LEVEL " + levelId;
		levels = Resources.LoadAll<TextAsset>("Levels").Length;

		// Initialize every cell as empty before applying the level records.
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				grid[x, y] = new Tile
				{
					x = x,
					y = y,
					type = TileType.Empty
				};
			}
		}

		// Blank lines and malformed records should not crash level loading.
		foreach (string rawLine in lines)
		{
			if (string.IsNullOrWhiteSpace(rawLine))
				continue;

			string[] fields = rawLine.Trim().Split(',');
			int tileX;
			int tileY;
			int tileId;
			if (fields.Length < 3 ||
				!int.TryParse(fields[0].Trim(), out tileX) ||
				!int.TryParse(fields[1].Trim(), out tileY) ||
				!int.TryParse(fields[2].Trim(), out tileId))
			{
				Debug.LogWarning("Skipping invalid tile record in level " + levelId + ": " + rawLine);
				continue;
			}

			// Coordinates are shifted by one when mapped into the 20x20 runtime grid.
			if (tileX < 0 || tileX >= 19 || tileY < 0 || tileY >= 19 || tileId < 0 || tileId > 3)
			{
				Debug.LogWarning("Skipping out-of-range tile record in level " + levelId + ": " + rawLine);
				continue;
			}

			int spikeRotation = 0;
			if (tileId == 2 &&
				(fields.Length < 4 || !int.TryParse(fields[3].Trim(), out spikeRotation)))
			{
				Debug.LogWarning("Skipping spike with missing or invalid rotation in level " + levelId + ": " + rawLine);
				continue;
			}

			GameObject prefab = Pool.GetTile(tileId);
			if (prefab == null)
			{
				Debug.LogWarning("No prefab exists for tile type " + tileId + ". Skipping: " + rawLine);
				continue;
			}

			Vector3 pos = new Vector3((tileX + 1) * 9, (tileY + 1) * 9, 0);
			GameObject tileObject = Instantiate(prefab, pos, Quaternion.identity);
			Tile gridTile = new Tile
			{
				x = tileX + 1,
				y = tileY + 1,
				type = (TileType)(tileId + 1),
				tile = tileObject
			};

			if (tileId == 1)
			{
				player = tileObject.GetComponent<Player>();
				if (player == null)
				{
					Debug.LogError("The player prefab has no Player component.");
					Destroy(tileObject);
					continue;
				}

				player.manager = this;
				cameraController.playerTarget = player.gameObject;
				gridTile.type = TileType.Empty;
				playerSpawn = pos;
			}

			if (tileId == 2)
				tileObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, spikeRotation * -90));

			if (tileId == 3)
				levelCompleteParticleEffect.transform.position = tileObject.transform.position;

			grid[tileX + 1, tileY + 1] = gridTile;
		}

		cameraController.SetTarget(this);
		ui.SetTutorial();
	}

	//Can the player move in the direction they want to?
	public bool PlayerCanMove (Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		Tile g = grid[(posX + dirX), (posY + dirY)];

		if(g.type == TileType.Block || g.type == TileType.Spike){
			return false;
		}else{
			return true;
		}
	}

	//Returns the tile type in the direction sent from the player's position.
	public TileType GetTileType (Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		Tile g = grid[(posX + dirX), (posY + dirY)];

		return g.type;
	}

	//Returns the tile that the player is ontop of.
	public Tile GetFloor (Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		if(dirY == 0){
			for(int x = 0; x < 20; x++){
				if(grid[(posX + (dirX * x)), posY].type == TileType.Block){
					return grid[(posX + (dirX * x) - dirX), posY];
				}
				if(grid[(posX + (dirX * x)), posY].type == TileType.Spike){
					player.fallingInSpikes = true;
				}
			}
		}

		if(dirX == 0){
			for(int y = 0; y < 20; y++){
				if(grid[posX, (posY + (dirY * y))].type == TileType.Block){
					return grid[posX, (posY + (dirY * y) - dirY)];
				}
				if(grid[posX, (posY + (dirY * y))].type == TileType.Spike){
					player.fallingInSpikes = true;
				}
			}
		}
			
		return null;
	}

	//Returns true if there is a floor for the player to fall onto.
	public bool HasFloor (Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		if(dirY == 0){
			for(int x = 0; x < 20; x++){
				if(posX + (dirX * x) > 0 && posX + (dirX * x) < 20){
					if(grid[(posX + (dirX * x)), posY].type == TileType.Block){
						return true;
					}
				}
			}
		}

		if(dirX == 0){
			for(int y = 0; y < 20; y++){
				if(posY + (dirY * y) > 0 && posY + (dirY * y) < 20){
					if(grid[posX, (posY + (dirY * y))].type == TileType.Block){
						return true;
					}
				}
			}
		}
			
		return false;
	}

	//Flips the gravity to the requested direction.
	public void FlipGravity (GravityDirection dir)
	{
		switch(dir){
			case GravityDirection.North:{
				cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
				levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 0);
				gravityDirection = GravityDirection.North;
				break;
			}
			case GravityDirection.South:{
				cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -180));
				levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 180);
				gravityDirection = GravityDirection.South;
				break;
			}
			case GravityDirection.East:{
				cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
				levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 90);
				gravityDirection = GravityDirection.East;
				break;
			}
			case GravityDirection.West:{
				cameraController.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -270));
				levelCompleteParticleEffect.transform.eulerAngles = new Vector3(-90, 0, 270);
				gravityDirection = GravityDirection.West;
				break;
			}
		}

		player.CheckGravity();
		ui.StartCoroutine("RotateCompass", dir);
	}
}

public enum GravityDirection {North, South, East, West}
public enum TileType {Empty, Block, Player, Spike, EndFlag}