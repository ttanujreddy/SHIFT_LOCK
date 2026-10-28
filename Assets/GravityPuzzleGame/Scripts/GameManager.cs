using UnityEngine;
using System.Collections;
using System.IO;

public class GameManager : MonoBehaviour 
{
	public const int FirstCustomLevelId = 10;
	//Level
	public int levelId;									//The current level.
	public int levels;									//The total amount of levels there are.

	[Header("Gravity Shift Budget")]
	[Min(0)] public int maxGravityShifts = 5; // Fallback for levels without an explicit budget.
	[Tooltip("Element 0 is Level0, element 1 is Level1, and so on. Set each level's maximum gravity shifts here.")]
	public int[] gravityShiftsPerLevel = { 5, 5, 5, 5, 5, 5, 6, 7, 8, 9, 2, 3, 4 };
	// New levels missing from an older scene's serialized array default to four shifts.
	private const int DefaultShiftBudgetForNewLevels = 4;
	[System.NonSerialized] public int gravityShiftsRemaining;
	[System.NonSerialized]
	public Tile[,] grid;								//Runtime-only tile grid.
	[System.NonSerialized] private bool doorsOpened;
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

		// Set the UI reference before loading so the level can update its HUD.
		ui.manager = this;

		//Then we load the level.
		LoadLevel();
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

		// Use the configured budget for this level; fall back to maxGravityShifts if no entry exists.
		if (gravityShiftsPerLevel != null &&
			levelId >= 0 && levelId < gravityShiftsPerLevel.Length)
		{
			maxGravityShifts = Mathf.Max(0, gravityShiftsPerLevel[levelId]);
		}
		else
		{
			// Do not use a serialized fallback field here: older scenes may retain its stale value.
			maxGravityShifts = DefaultShiftBudgetForNewLevels;
		}

		ResetGravityShiftBudget();
		doorsOpened = false;
		grid = new Tile[20, 20];
		string[] lines = levelAsset.text.Split('\n');
		int customLevelCount = 0;
		while (Resources.Load<TextAsset>("Levels/Level" + (FirstCustomLevelId + customLevelCount)) != null)
			customLevelCount++;

		// Keep internal IDs 10-12 for budget/progression data, but show the custom campaign as Levels 1-3.
		ui.levelText.text = levelId >= FirstCustomLevelId
			? "LEVEL " + (levelId - FirstCustomLevelId + 1) + " / " + customLevelCount
			: "LEVEL " + levelId;
		levels = FirstCustomLevelId + customLevelCount;

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
			if (tileX < 0 || tileX >= 19 || tileY < 0 || tileY >= 19 || tileId < 0 || tileId > 5)
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

			// Temporary switch/door visuals; the planned art pass will replace these.
			if (tileId == 4 || tileId == 5)
			{
				SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
				if (renderer != null)
					renderer.color = tileId == 4 ? new Color(0.15f, 0.85f, 1f, 1f) : new Color(1f, 0.28f, 0.18f, 1f);
			}

			grid[tileX + 1, tileY + 1] = gridTile;
		}

		cameraController.SetTarget(this);
		ui.SetTutorial();
	}

	// Starts each chamber/retry with its full gravity-shift allowance.
	public void ResetGravityShiftBudget()
	{
		gravityShiftsRemaining = Mathf.Max(0, maxGravityShifts);
		if (ui != null)
			ui.UpdateShiftCounter(gravityShiftsRemaining, maxGravityShifts);
	}

	// Player-initiated gravity changes consume a shift. System resets use FlipGravity directly.
	public bool TryFlipGravity(GravityDirection dir)
	{
		if (dir == gravityDirection || gravityShiftsRemaining <= 0)
		{
			if (ui != null)
				ui.UpdateShiftCounter(gravityShiftsRemaining, maxGravityShifts);
			return false;
		}

		gravityShiftsRemaining--;
		FlipGravity(dir);
		if (ui != null)
			ui.UpdateShiftCounter(gravityShiftsRemaining, maxGravityShifts);
		return true;
	}

	// A gravity switch opens every locked door in the current chamber.
	public void ActivateDoors()
	{
		if (doorsOpened || grid == null)
			return;

		doorsOpened = true;
		for (int x = 0; x < grid.GetLength(0); x++)
		{
			for (int y = 0; y < grid.GetLength(1); y++)
			{
				Tile cell = grid[x, y];
				if (cell == null)
					continue;

				if (cell.type == TileType.LockedDoor)
				{
					if (cell.tile != null)
						Destroy(cell.tile);
					cell.tile = null;
					cell.type = TileType.Empty;
				}
				else if (cell.type == TileType.GravitySwitch && cell.tile != null)
				{
					SpriteRenderer renderer = cell.tile.GetComponent<SpriteRenderer>();
					if (renderer != null)
						renderer.color = new Color(0.25f, 1f, 0.45f, 1f);
				}
			}
		}
	}

	bool IsSolidTile(TileType type)
	{
		return type == TileType.Block || type == TileType.LockedDoor;
	}

	//Can the player move in the direction they want to?
	public bool PlayerCanMove (Vector3 playerPos, Vector3 dir)
	{
		int posX = (int)(playerPos.x / 9);
		int posY = (int)(playerPos.y / 9);
		int dirX = (int)dir.x;
		int dirY = (int)dir.y;

		Tile g = grid[(posX + dirX), (posY + dirY)];

		if (g.type == TileType.Block || g.type == TileType.Spike || g.type == TileType.LockedDoor){
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
				if(IsSolidTile(grid[(posX + (dirX * x)), posY].type)){
					return grid[(posX + (dirX * x) - dirX), posY];
				}
				if(grid[(posX + (dirX * x)), posY].type == TileType.Spike){
					player.fallingInSpikes = true;
				}
			}
		}

		if(dirX == 0){
			for(int y = 0; y < 20; y++){
				if(IsSolidTile(grid[posX, (posY + (dirY * y))].type)){
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
					if(IsSolidTile(grid[(posX + (dirX * x)), posY].type)){
						return true;
					}
				}
			}
		}

		if(dirX == 0){
			for(int y = 0; y < 20; y++){
				if(posY + (dirY * y) > 0 && posY + (dirY * y) < 20){
					if(IsSolidTile(grid[posX, (posY + (dirY * y))].type)){
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
public enum TileType {Empty, Block, Player, Spike, EndFlag, GravitySwitch, LockedDoor}