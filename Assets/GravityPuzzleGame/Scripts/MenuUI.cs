using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuUI : MonoBehaviour 
{
	//Pages
	public GameObject menuPage;		//The page containing the menu.
	public GameObject levelsPage;	//The page containing the levels.

	//Levels
	public GameObject levelHolder;	//The gameobject holding all the level buttons.
	public GameObject levelButton;	//The level button prefab.

	void Start ()
	{
		//If level 0 is locked, set it to be unlocked.
		if(PlayerPrefs.GetInt("Level0") == 0)
			PlayerPrefs.SetInt("Level0", 1);

		//Load the level buttons.
		LoadLevels();
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
		for(int x = 0; x < Resources.LoadAll<TextAsset>("Levels").Length; x++){
			GameObject level = Instantiate(levelButton, levelHolder.transform, false);
			level.transform.Find("Text").GetComponent<TMP_Text>().text = "LEVEL " + x;
			int e = x;
			level.transform.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(delegate {PlayLevel(e);}));
			level.transform.localScale = Vector3.one;

			int l = PlayerPrefs.GetInt("Level" + x);

			if(l == 0){
				level.transform.GetComponent<Image>().color = new Color(0.5f, 0.0f, 0.0f, 1.0f);
				level.transform.GetComponent<Button>().interactable = false;
			}
			if(l == 2){
				level.transform.GetComponent<Image>().color = new Color(0.0f, 0.5f, 0.0f, 1.0f);
			}
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
