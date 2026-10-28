using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour 
{
	public GameManager manager;		//The game manager class.
	public SpriteRenderer sr;		//The player's sprite renderer.

	public Vector3 targetPos;		//The position that the player intends to move to.

	//Bools
	public bool falling;			//Is the player currently falling?
	public bool moving;				//Is the player currently moving?
	public bool fallingInSpikes;	//Is the player currently falling into a spike?

	void Update ()
	{
		//If the player is not moving and not falling, they are allowed to either move or change the gravity.
		if(!moving && !falling){
			Inputs();
		}

		// R retries the current chamber; Escape returns to the menu.
		if (Input.GetKeyDown(KeyCode.R))
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);

		if (Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene(0);
	}

	//Manages the key press inputs. 
	//Manages moving the player and the changing of the gravity.
	void Inputs ()
	{
		//Move Left
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			MoveInput("left");
		}

		//Move Right
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			MoveInput("right");
		}

		//Change Gravity
		if(Input.GetKeyDown(KeyCode.W)){
			ChangeGravityInput("up");
		}

		//Pressing "S" doesn't change the gravity since the gravity is always downwards for the player.

		if(Input.GetKeyDown(KeyCode.A)){
			ChangeGravityInput("left");
		}

		if(Input.GetKeyDown(KeyCode.D)){
			ChangeGravityInput("right");
		}
	}

	//Called when the player presses either the left or right arrow key to move.
	//"d" is the direction that the player wants to move ("left" or "right").
	public void MoveInput (string d)
	{
		//If the target direction is "left", then we set the "dir" to be the left of the player depending
		//on what the direction of gravity is. This code is then repeated for the "right".
		if(d == "left"){
			Vector3 dir = Vector3.left;

			if(manager.gravityDirection == GravityDirection.North)
				dir = Vector3.left;
			if(manager.gravityDirection == GravityDirection.South)
				dir = Vector3.right;
			if(manager.gravityDirection == GravityDirection.East)
				dir = Vector3.up;
			if(manager.gravityDirection == GravityDirection.West)
				dir = Vector3.down;

			//From this we can then set the target pos to be one block to the left of the player.
			targetPos = transform.position + (dir * 9);

			//Then we need to check if the player can move in that direction. If there is a block or spike
			//in the way, then they cannot.
			if(manager.PlayerCanMove(transform.position, dir)){
				moving = true;
				StartCoroutine("Move");
			}
		}

		if(d == "right"){
			Vector3 dir = Vector3.right;

			if(manager.gravityDirection == GravityDirection.North)
				dir = Vector3.right;
			if(manager.gravityDirection == GravityDirection.South)
				dir = Vector3.left;
			if(manager.gravityDirection == GravityDirection.East)
				dir = Vector3.down;
			if(manager.gravityDirection == GravityDirection.West)
				dir = Vector3.up;

			targetPos = transform.position + (dir * 9);
			if(manager.PlayerCanMove(transform.position, dir)){
				moving = true;
				StartCoroutine("Move");
			}
		}
	}

	//Called when the player presses either the W, A, S or D keys to change the gravity direction.
	//"dir" is the direction that the gravity will change to.
	public void ChangeGravityInput (string dir)
	{
		//If the target gravity direction is "up", then we flip the gravity, depending on what the
		//current gravity direction is. This code is repeated for the other 2 directions.
		if(dir == "up"){
			switch(manager.gravityDirection){
				case GravityDirection.North:{
					manager.TryFlipGravity(GravityDirection.South);
					break;
				}
				case GravityDirection.South:{
					manager.TryFlipGravity(GravityDirection.North);
					break;
				}
				case GravityDirection.East:{
					manager.TryFlipGravity(GravityDirection.West);
					break;
				}
				case GravityDirection.West:{
					manager.TryFlipGravity(GravityDirection.East);
					break;
				}
			}
		}

		//Pressing "S" doesn't change the gravity since the gravity is always downwards for the player.

		if(dir == "left"){
			switch(manager.gravityDirection){
				case GravityDirection.North:{
					manager.TryFlipGravity(GravityDirection.East);
					break;
				}
				case GravityDirection.South:{
					manager.TryFlipGravity(GravityDirection.West);
					break;
				}
				case GravityDirection.East:{
					manager.TryFlipGravity(GravityDirection.South);
					break;
				}
				case GravityDirection.West:{
					manager.TryFlipGravity(GravityDirection.North);
					break;
				}
			}
		}

		if(dir == "right"){
			switch(manager.gravityDirection){
				case GravityDirection.North:{
					manager.TryFlipGravity(GravityDirection.West);
					break;
				}
				case GravityDirection.South:{
					manager.TryFlipGravity(GravityDirection.East);
					break;
				}
				case GravityDirection.East:{
					manager.TryFlipGravity(GravityDirection.North);
					break;
				}
				case GravityDirection.West:{
					manager.TryFlipGravity(GravityDirection.South);
					break;
				}
			}
		}
	}

	//This is called after the player flips the gravity.
	//It is also called after the player moves to a new position.
	//It checks to see if there is a block under the player. If there isn't then the player will fall.
	public void CheckGravity ()
	{
		//We first create a temp variable "dir", this is the direction of gravity in a Vector3 format.
		Vector3 dir = Vector3.down;

		if(manager.gravityDirection == GravityDirection.North)
			dir = Vector3.down;
		if(manager.gravityDirection == GravityDirection.South)
			dir = Vector3.up;
		if(manager.gravityDirection == GravityDirection.East)
			dir = Vector3.left;
		if(manager.gravityDirection == GravityDirection.West)
			dir = Vector3.right;

		//Then we get the tile that the player is currently sitting ontop of.
		TileType t = manager.GetTileType(transform.position, dir);

		//If that tile is not a block (meaning it's air) then the player will fall.
		if(t != TileType.Block && t != TileType.LockedDoor){
			
			//We then check to see if the player does fall, will they land on a floor?
			if(manager.HasFloor(transform.position, dir)){
				Tile floor = manager.GetFloor(transform.position, dir);

				falling = true;

				//Is the player going to fall on a spike?
				if(floor.type == TileType.Spike)
					fallingInSpikes = true;

				//Finally, we call the coroutine "Fall" and the player will fall down to the floor or on a spike.
				StartCoroutine("Fall", new Vector3(floor.x * 9, floor.y * 9, 0));

			//If there is no floor for the player to land on, then they will fall to their death.
			}else{
				falling = true;
				StartCoroutine("DeathFall", new Vector3(transform.position.x + (dir.x * 9 * 7), transform.position.y + (dir.y * 9 * 7), 0));
			}
		}
	}

	//This is called one the player falls and lands on a tile, or if the player moves.
	//It checks to see if the current tile that the player is on, is a flag.
	//If it is a flag, then the player has won the level and the Win() function gets called.
	public void CheckCurTile ()
	{
		TileType t = manager.grid[(int)(transform.position.x / 9), (int)(transform.position.y / 9)].type;

		if (t == TileType.GravitySwitch)
			manager.ActivateDoors();

		if (t == TileType.EndFlag && !fallingInSpikes)
			Win();
	}

	//This is called whenever the player falls off the map or falls into a spike.
	public void Die ()
	{
		ShiftLockAudio.PlayDeath();
		//We reset the player's position, the gravity direction and other variables.

		transform.position = manager.playerSpawn;
		manager.FlipGravity(GravityDirection.North);
		manager.ResetGravityShiftBudget();

		fallingInSpikes = false;
		falling = false;
		moving = false;

		sr.color = Color.white;
	}

	//This is called when the player lands on the flag.
	public void Win ()
	{
		ShiftLockAudio.PlayVictory();
		//We tell the player prefs that we have won this level.
		PlayerPrefs.SetInt("Level" + manager.levelId, 2);

		//Then we make it so that the next level is unlocked.
		//0 = locked, 1 = unlocked, 2 = completed.
		int nextLevel = PlayerPrefs.GetInt("Level" + (manager.levelId + 1));

		if(nextLevel == 0 || nextLevel == 1){
			PlayerPrefs.SetInt("Level" + (manager.levelId + 1), 1);
		}

		//Finally, we shoot off a particle effect and call WinTimer, so that the next level
		//loads after a few seconds.
		moving = true;
		manager.levelCompleteParticleEffect.Play();
		StartCoroutine("WinTimer");
	}

	//This is called once the player completes a level.
	//It waits a few seconds before displaying the win screen - allowing the player to either
	//go to the next level or go to the menu.
	IEnumerator WinTimer ()
	{
		yield return new WaitForSeconds(2);
		manager.ui.SetWinScreen();
	}

	//This is called once the player moves. This is the movement animation.
	IEnumerator Move ()
	{
		ShiftLockAudio.PlayMove();
		while(transform.position != targetPos){
			transform.position = Vector3.MoveTowards(transform.position, targetPos, 54 * Time.deltaTime);
			yield return null;
		}

		moving = false;

		//After moving we need to check to see if we are ontop of a block and check to see if we are in a flag tile.
		CheckGravity();
		CheckCurTile();
	}

	//This is called when the player falls.
	//"pos" is the position where the player will to.
	IEnumerator Fall (Vector3 pos)
	{
		while(transform.position != pos){
			transform.position = Vector3.MoveTowards(transform.position, pos, 72 * Time.deltaTime);
			yield return null;
		}

		falling = false;

		CheckCurTile();

		//If the player is falling into spikes they will die.
		if(fallingInSpikes)
			Die();
	}

	//This is called when the player falls off the level.
	//"pos" is the position where the player will fall to.
	IEnumerator DeathFall (Vector3 pos)
	{
		//Since the player is falling to their death, they will fade away as they fall, just before dying.
		//This gives off the impression as if they are falling into the void.

		float elapsed = 0;

		while(transform.position != pos){
			transform.position = Vector3.MoveTowards(transform.position, pos, 72 * Time.deltaTime);

			sr.color = Color.Lerp(Color.white, new Color(1.0f, 1.0f, 1.0f, 0.0f), (elapsed / 2.0f));
			elapsed += Time.deltaTime;

			yield return null;
		}

		Die();
	}
}
