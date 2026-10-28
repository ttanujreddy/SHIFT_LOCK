using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	public Vector3 centerTarget;				//The center of the level.
	public GameObject playerTarget;				//The game object of the player.
	public static bool followPlayer = true;		//Is the camera following the player?
	public Camera cam;							//The Camera class.

	public int minZoom;							//The minimum amount that the camera can zoom in.
	public int maxZoom;							//The maximum amount that the camera can zoom out.

	void LateUpdate ()
	{
		//If we are following the player, then do so. Otherwise stay stationary in the center of the level.
		if(followPlayer && playerTarget != null){
			transform.position = new Vector3(playerTarget.transform.position.x, playerTarget.transform.position.y, -10);
		}else{
			transform.position = centerTarget;
		}

		Zoom();
	}

	//Manages the zooming of the camera.
	void Zoom ()
	{
		if(Input.GetAxis("Mouse ScrollWheel") != 0){
			cam.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * -20;

			if(cam.orthographicSize > maxZoom)
				cam.orthographicSize = maxZoom;
			if(cam.orthographicSize < minZoom)
				cam.orthographicSize = minZoom;
		}
	}

	//Called by the game manager class.
	//This function finds the center of the level by finding the average position of all the tiles.
	public void SetTarget (GameManager manager)
	{
		int x = 0;
		int y = 0;
		int total = 0;

		foreach(Tile tile in manager.grid){
			if(tile.type != TileType.Empty){
				x += (int)tile.tile.transform.position.x;
				y += (int)tile.tile.transform.position.y;
				total++;
			}
		}

		centerTarget = new Vector3(x / total, y / total, -10);
		transform.position = centerTarget;
	}

	//Called when the "CAM" button is pressed. Toggles between following the player and remaining stationary
	//in the center of the level.
	public void ToggleCameraMode ()
	{
		if(followPlayer){
			followPlayer = false;
		}else{
			followPlayer = true;
		}
	}
}
