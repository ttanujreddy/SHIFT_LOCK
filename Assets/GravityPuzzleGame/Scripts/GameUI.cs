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

	void Start()
	{
		ShiftLockAudio.EnsureExists();
		ShiftLockAudio.AddClickSounds(winScreen);
		ShiftLockTheme.StyleText(levelText, ShiftLockTheme.Accent, true);
		if (levelText != null) levelText.fontSize = 28f;
		ShiftLockTheme.StyleText(tutorialText, ShiftLockTheme.Text, false);
		if (compass != null)
		{
			foreach (Graphic graphic in compass.GetComponentsInChildren<Graphic>(true))
				graphic.color = ShiftLockTheme.Accent;
		}
		ShiftLockTheme.StylePanel(winScreen);
		ShiftLockTheme.ApplyCamera(Camera.main);
	}

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
		// Anchor the counter to the same canvas as the existing level label.
		// FindAnyObjectByType can select a different canvas with a different scale.
		Canvas canvas = levelText != null ? levelText.GetComponentInParent<Canvas>() : null;
		if (canvas == null)
		{
			Debug.LogWarning("Cannot display the gravity-shift counter because levelText is not under a Canvas.");
			return;
		}

		// Draw the counter on a dark panel below the camera toggle.
		GameObject panelObject = new GameObject("ShiftCounterPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
		RectTransform panelRect = panelObject.GetComponent<RectTransform>();
		panelRect.SetParent(canvas.transform, false);
		panelRect.anchorMin = new Vector2(0, 1);
		panelRect.anchorMax = new Vector2(0, 1);
		panelRect.pivot = new Vector2(0, 1);
		panelRect.anchoredPosition = new Vector2(8, -92);
		panelRect.sizeDelta = new Vector2(250, 42);
		panelObject.GetComponent<Image>().color = ShiftLockTheme.Panel;
		Outline outline = panelObject.GetComponent<Outline>();
		outline.effectColor = new Color(ShiftLockTheme.Accent.r, ShiftLockTheme.Accent.g, ShiftLockTheme.Accent.b, 0.55f);
		outline.effectDistance = new Vector2(1f, -1f);

		GameObject counterObject = new GameObject("ShiftCounter", typeof(RectTransform), typeof(TextMeshProUGUI));
		RectTransform rect = counterObject.GetComponent<RectTransform>();
		rect.SetParent(canvas.transform, false);
		rect.anchorMin = new Vector2(0, 1);
		rect.anchorMax = new Vector2(0, 1);
		rect.pivot = new Vector2(0, 1);
		rect.anchoredPosition = new Vector2(18, -97);
		rect.sizeDelta = new Vector2(270, 36);

		shiftCounterText = counterObject.GetComponent<TextMeshProUGUI>();
		if (TMP_Settings.defaultFontAsset != null)
			shiftCounterText.font = TMP_Settings.defaultFontAsset;
		shiftCounterText.fontSize = 22;
		shiftCounterText.fontStyle = FontStyles.Bold;
		shiftCounterText.color = ShiftLockTheme.Accent;
		shiftCounterText.alignment = TextAlignmentOptions.TopLeft;
		shiftCounterText.textWrappingMode = TextWrappingModes.NoWrap;
		shiftCounterText.raycastTarget = false;
	}

	//Called when the player wins the game.
	public void SetWinScreen ()
	{
		winScreen.SetActive(true);

		ShiftLockTheme.StylePanel(winScreen);
		if (manager.levelId >= GameManager.FirstCustomLevelId + 2)
			nextLevelButton.SetActive(false);
		else
			nextLevelButton.SetActive(true);
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
		if (manager.levelId >= GameManager.FirstCustomLevelId)
		{
			switch (manager.levelId)
			{
				case 10: tutorialText.text = "<b>POWER CYCLE</b>  // Touch the cyan switch to unlock the door."; break;
				case 11: tutorialText.text = "<b>THE DANGEROUS SHORTCUT</b>  // Avoid the hazard. Reach the switch, then escape."; break;
				case 12: tutorialText.text = "<b>FAIL-SAFE</b>  // One switch opens both security doors. Plan your route."; break;
				default: tutorialText.text = "Use arrow keys to move. Use W/A/D to change gravity. Press R to restart."; break;
			}
		}
		else if (manager.levelId < 3)
		{
			switch (manager.levelId)
			{
				case 0: tutorialText.text = "Use the <b>ARROW KEYS</b> to move the player."; break;
				case 1: tutorialText.text = "Change gravity with <b>WASD</b>."; break;
				case 2: tutorialText.text = "Watch out for the <b>SPIKES</b>!"; break;
			}
		}
		ShiftLockTheme.StyleText(tutorialText, ShiftLockTheme.Text, false);
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
