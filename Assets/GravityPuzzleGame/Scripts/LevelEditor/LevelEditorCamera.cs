using UnityEngine;
using System.Collections;

public class LevelEditorCamera : MonoBehaviour 
{
	public Camera cam;		//The camera class.
	public float moveSpeed;	//The move speed of the camera.
	public int minZoom;		//The minimum amount that the camera can zoom in.
	public int maxZoom;		//The maximum amount that the camera can zoom out.

	void Update ()
	{
		//Moves the camera based on horizontal and vertical inputs.
		cam.transform.Translate(new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed, Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed, 0));

		//Zooms the camera in/out based on the scroll wheel.
		if(Input.GetAxis("Mouse ScrollWheel") != 0){
			cam.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * -20;

			if(cam.orthographicSize > maxZoom)
				cam.orthographicSize = maxZoom;
			if(cam.orthographicSize < minZoom)
				cam.orthographicSize = minZoom;
		}
	}
}
