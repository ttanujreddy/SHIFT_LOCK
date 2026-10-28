Gravity Puzzle Game
<><><><><><><><><><><><><>

------------------------------
Contents
------------------------------

- What is Gravity Puzzle Game?
- About the Project
- Project Folders Layout
- Level Editor
- PlayerPrefs
- Contact



------------------------------
What is Gravity Puzzle Game?
------------------------------

Gravity Puzzle Game, is a small puzzle game where you play as a green cube and the
aim of each level is to get to the red flag. How you do this requires you to move
the cube using the ARROW KEYS, as well as the WASD keys to change the direction of
gravity. By changing the direction of gravity, it can allow you to get to areas that
were once unaccessable and challenges your mind to think out a puzzle from different
perspectives.



------------------------------
About the Project
------------------------------

This project contains the full game and the features listed below:

- Fully documented scripts.
- A level editor (can only be used in the editor).



------------------------------
Project Folders Layout
------------------------------

> GravityPuzzleGame
	> Animations
		> MenuPlayerAnim.anim
		> Player.controller
	> Resources
		> LevelEditorPrefabs
			> Block.prefab
			> EndFlag.prefab
			> Player.prefab
			> Spike.prefab
		> Levels
			(Levels go here)
		> MenuPrefabs
			> LevelButton.prefab
		> Prefabs
			> Block.prefab
			> EndFlag.prefab
			> LevelCompleteParticleEffect.prefab
			> Player.prefab
			> Spike.prefab
		> Sprites
			(Sprites)
	> Scenes
		> Game.unity
		> LevelEditor.unity
		> Menu.unity
	> Scripts
		> LevelEditor
			> LevelEditor.cs
			> LevelEditorCamera.cs
		> CameraController.cs
		> GameManager.cs
		> GameUI.cs
		> MenuUI.cs
		> Player.cs
		> Pool.cs
		> Tile.cs
	> readme.txt



------------------------------
Level Editor
------------------------------

This project comes with a Level Editor (LevelEditor.unity). It is a scene you can 
load up in the editor and use to create your levels for the game. 



To create a level you first must be in the Unity Editor, then:

1)	Open up the "LevelEditor.unity" scene and begin to create your level using the
	tools provided.

2) 	Then once your level is complete, click on the "Save as LEVEL ..." button. This
	will save the level to the "GravityPuzzleGame/Resources/Levels" folder.    

DO NOT try and use the level editor in a built version of the game. It only works in
the editor since it saves files to the Resources folder.  



The controls in the Level Editor:

> Place Tile: Left Mouse Button
> Delete Tile: Right Mouse Button
> Rotate Tile: R

> Move Camera: WASD or Arrow Keys
> Zoom In/Out: Mouse Scroll Wheel

> Exit to Menu: Escape



------------------------------
PlayerPrefs
------------------------------

PlayerPrefs are used in this project. Each level has an int playerpref. E.g. "Level2".
The number that each level has is dependent on it state. 

0 = the level is locked to the player.
1 = the level is unlocked and can be played.
2 = the level is completed.

So in the code you will see these prefs being changed to like 1 or 2. Now you know
that this just means the state of the level is being changed.



------------------------------
Contact
------------------------------

Thank you for purchasing this asset! If there are any questions or recommendations for
future updates you have, please feel free to contact me at the outlets below:

E-mail: buckleydaniel101@gmail.com
Steam: Sothern
Discord: Sothern#4256



Thank you, and have fun with the asset.

- Daniel



