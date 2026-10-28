using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the orthographic gameplay camera. It can follow the player or stay centered on the chamber, and clamps scroll-wheel zoom.
/// </summary>
public class CameraController : MonoBehaviour 
{
	public Vector3 centerTarget;				//The center of the level.
	public GameObject playerTarget;				//The game object of the player.
	public static bool followPlayer = true;		//Is the camera following the player?
	public Camera cam;							//The Camera class.

	public int minZoom;							//The minimum amount that the camera can zoom in.
	public int maxZoom;							//The maximum amount that the camera can zoom out.

	/// <summary>
	/// Position the camera after gameplay movement, then apply zoom input.
	/// </summary>
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
	/// <summary>
	/// Adjust orthographic size using the mouse wheel, clamped to configured limits.
	/// </summary>
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
	/// <summary>
	/// Calculate the center of all populated tiles after the level has loaded.
	/// </summary>
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
	/// <summary>
	/// Toggle between following the player and centering on the chamber.
	/// </summary>
	public void ToggleCameraMode ()
	{
		if(followPlayer){
			followPlayer = false;
		}else{
			followPlayer = true;
		}
	}
}
