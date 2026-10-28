using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuUI : MonoBehaviour 
{
	// The original template occupies Level0-Level9. The custom SHIFT//LOCK campaign starts at Level10.
	const int firstCustomLevelId = 10;
	//Pages
	public GameObject menuPage;		//The page containing the menu.
	public GameObject levelsPage;	//The page containing the levels.

	//Levels
	public GameObject levelHolder;	//The gameobject holding all the level buttons.
	public GameObject levelButton;	//The level button prefab.

	void Start ()
	{
		// Apply the game's theme before creating level buttons.
		ShiftLockTheme.ApplyCamera(Camera.main);
		ShiftLockTheme.StylePage(menuPage, true);
		ShiftLockTheme.StylePage(levelsPage, false);
		ApplyDecorativeSprites();

		// Show only the original SHIFT//LOCK chambers. Template levels remain available in the project for reference.
		LoadLevels();
	}

	
    // Apply generated art to decorative objects placed around the menu scene.
    // Gameplay tiles are themed separately by GameManager when levels are loaded.
    void ApplyDecorativeSprites()
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || renderer.sprite == null) continue;

            string spriteName = renderer.sprite.name.ToLowerInvariant();
            int tileId = -1;
            if (spriteName.Contains("block")) tileId = 0;
            else if (spriteName.Contains("spike")) tileId = 2;
            else if (spriteName.Contains("endflag") || spriteName.Contains("flag")) tileId = 3;
            else if (spriteName.Contains("player")) tileId = 1;

            if (tileId >= 0)
                ShiftLockTheme.ApplyTile(renderer.gameObject, tileId);
        }
    }

	//Called when the menu buttons are pressed to change the page.
	//"page" is the page that the player wants to change to.
	public void SetPage (string page)
	{
		if(page == "menu"){
			menuPage.SetActive(true);
			levelsPage.SetActive(false);
		}
		if(page == "levels"){
			menuPage.SetActive(false);
			levelsPage.SetActive(true);
		}
	}

	//This loads the level buttons.
	//It loops through each level, creating a button on the "Levels" page, and making it so that when you click it,
	//it calls the PlayLevel function, which starts the level.
	public void LoadLevels ()
	{
		// Count consecutive custom levels, rather than displaying the ten template levels first.
		int customLevelCount = 0;
		while (Resources.Load<TextAsset>("Levels/Level" + (firstCustomLevelId + customLevelCount)) != null)
			customLevelCount++;

		for (int index = 0; index < customLevelCount; index++)
		{
			int levelId = firstCustomLevelId + index;
			GameObject level = Instantiate(levelButton, levelHolder.transform, false);
			level.transform.Find("Text").GetComponent<TMP_Text>().text = "LEVEL " + (index + 1);

			Button button = level.GetComponent<Button>();
			ShiftLockTheme.StyleButton(button);
			TMP_Text levelLabel = level.transform.Find("Text").GetComponent<TMP_Text>();
			ShiftLockTheme.StyleText(levelLabel, ShiftLockTheme.Accent, true);
			button.onClick.AddListener(new UnityEngine.Events.UnityAction(delegate { PlayLevel(levelId); }));
			button.interactable = true; // All custom chambers are directly selectable for evaluation.

			level.transform.localScale = Vector3.one;
			int completion = PlayerPrefs.GetInt("Level" + levelId);
			if (completion == 2)
				level.GetComponent<Image>().color = new Color(0.05f, 0.35f, 0.25f, 1f);
		}
	}

	//Called when a level button gets pressed.
	//"level" is the variable that the button sends over, telling the function which level to load.
	public void PlayLevel (int level)
	{
		//We set the playerpref "SelectedLevel" to  be the "level", then we load up the Game.unity scene.
		PlayerPrefs.SetInt("SelectedLevel", level);
		SceneManager.LoadScene(1);
	}

	//Called when the "QUIT" button is pressed.
	//It quits the application to the desktop.
	public void QuitGame ()
	{
		Application.Quit();
	}
}
