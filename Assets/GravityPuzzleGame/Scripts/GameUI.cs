using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour 
{
	public TMP_Text levelText;				//The text at the top of the screen displaying the current level.
	public TMP_Text tutorialText;			//The text at the bottom of the screen, displaying a tutorial.
	public GameObject winScreen;		//The win screen that pops up after the player lands on the flag.
	public GameObject nextLevelButton;	//The button that sets up the next level.
	public GameManager manager;			//The game manager class.

	public GameObject compass;			//The game object that is used as a compass to tell the player what direction they are.

	private TMP_Text shiftCounterText;

	// Creates a HUD label at runtime so the existing template scene needs no manual re-wiring.
	public void UpdateShiftCounter(int remaining, int total)
	{
		if (shiftCounterText == null)
			CreateShiftCounter();

		if (shiftCounterText != null)
			shiftCounterText.text = "SHIFTS LEFT: " + remaining + " / " + total;
	}

	void CreateShiftCounter()
	{
		Canvas canvas = FindAnyObjectByType<Canvas>();
		if (canvas == null)
		{
			Debug.LogWarning("Cannot display the gravity-shift counter because the Game scene has no Canvas.");
			return;
		}

		GameObject counterObject = new GameObject("ShiftCounter", typeof(RectTransform), typeof(TextMeshProUGUI));
		RectTransform rect = counterObject.GetComponent<RectTransform>();
		rect.SetParent(canvas.transform, false);
		rect.anchorMin = new Vector2(0, 1);
		rect.anchorMax = new Vector2(0, 1);
		rect.pivot = new Vector2(0, 1);
		rect.anchoredPosition = new Vector2(18, -88);
		rect.sizeDelta = new Vector2(300, 42);

		shiftCounterText = counterObject.GetComponent<TextMeshProUGUI>();
		if (TMP_Settings.defaultFontAsset != null)
			shiftCounterText.font = TMP_Settings.defaultFontAsset;
		shiftCounterText.fontSize = 24;
		shiftCounterText.fontStyle = FontStyles.Bold;
		shiftCounterText.color = new Color(0.08f, 0.08f, 0.08f, 1f);
		shiftCounterText.alignment = TextAlignmentOptions.TopLeft;
		shiftCounterText.textWrappingMode = TextWrappingModes.NoWrap;
		shiftCounterText.raycastTarget = false;
	}

	//Called when the player wins the game.
	public void SetWinScreen ()
	{
		winScreen.SetActive(true);

		if(manager.levels == manager.levelId + 1)
			nextLevelButton.SetActive(false);
	}

	//Called when the player presses the "Next Level" button on the win screen.
	//It reloads the level, and loads up the next level.
	public void NextLevelButton ()
	{
		PlayerPrefs.SetInt("SelectedLevel", manager.levelId + 1);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	//Called when the "Menu" button gets pressed on the win screen.
	public void MenuButton ()
	{
		SceneManager.LoadScene(0);
	}

	//This sets the tutorial text based on what level it is.
	//If it is level 1, then it will talk about moving, etc.
	//You can change or remove this to match your levels.
	public void SetTutorial ()
	{
		if(manager.levelId < 3){
			switch(manager.levelId){
				case 0:{
					tutorialText.text = "Use the <b>ARROW KEYS</b> to move the player.";
					break;
				}
				case 1:{
					tutorialText.text = "Change the direction of gravity with <b>WASD</b>.";
					break;
				}
				case 2:{
					tutorialText.text = "Watch out for the <b>SPIKES</b>!";
					break;
				}
			}
		}
	}

	//Called whenever the player changes the gravity. 
	//It rotates the compass (top right) to face the original north direction.
	IEnumerator RotateCompass (GravityDirection dir)
	{
		int rot = 0;

		if(dir == GravityDirection.North)
			rot = 0;
		if(dir == GravityDirection.South)
			rot = 180;
		if(dir == GravityDirection.East)
			rot = 90;
		if(dir == GravityDirection.West)
			rot = 270;

		RectTransform t = compass.GetComponent<RectTransform>();

		while((int)t.localEulerAngles.z != rot){
			t.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(t.localEulerAngles.z, rot, 1000 * Time.deltaTime));
			yield return null;
		}
	}
}
