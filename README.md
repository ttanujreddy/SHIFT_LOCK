# SHIFT//LOCK

**SHIFT//LOCK** is a single-player 2D gravity puzzle game developed in **Unity 6000.5.9f1** for **CS 583: 3D Game Programming**. The goal is to reach the exit by changing gravity, navigating hazards, and activating switches to open locked doors—all within a limited gravity-shift budget.

The playable MVP features **three original chambers**, each directly selectable from the main menu.

## Getting started

1. Clone the repository and open the project in Unity Hub using **Unity 6000.5.9f1**.
2. Open `Assets/GravityPuzzleGame/Scenes/Menu.unity`.
3. Press **Play**, select **PLAY**, and choose a chamber.

## Controls

| Input | Action |
| --- | --- |
| Left / Right Arrow | Move along the current surface, relative to gravity |
| W | Reverse gravity |
| A / D | Rotate gravity left/right relative to the player |
| R | Restart the current chamber |
| Escape | Return to the main menu |
| CAM button | Toggle player-follow and centered camera |
| Mouse wheel | Zoom the camera |

**S** is not mapped to a gravity change. Each successful gravity change consumes one shift; the **SHIFTS LEFT** counter shows the remaining budget. Attempts to change gravity without enough shifts do nothing.

## Mechanics

- **Gravity:** Movement is relative to gravity. When unsupported, the player falls in the current gravity direction.
- **Switches and doors:** Entering a switch activates it once, changes its appearance, and removes every locked door in the chamber.
- **Hazards:** Spikes or falling into the void cause death. The scene reloads, resetting player position, gravity, shift budget, switches, and doors.
- **Completion:** Reaching the exit flag completes a chamber. Completion state is saved with Unity PlayerPrefs.

### Original chambers

| Displayed level | Internal asset | Chamber | Shift budget |
| --- | --- | --- | ---: |
| Level 1 | `Level10.txt` | Power Cycle | 2 |
| Level 2 | `Level11.txt` | The Dangerous Shortcut | 3 |
| Level 3 | `Level12.txt` | Fail-Safe | 4 |

The ten original template levels (`Level0.txt` through `Level9.txt`) remain in the repository for reference but are not displayed in normal level selection. Internal IDs 10–12 were preserved to retain level settings. Level 12 was playtested with two four-shift routes; the budgets are not presented as mathematically proven minima.

## Project structure

```text
Assets/GravityPuzzleGame/
├── Scenes/                   # Menu, Game, LevelEditor
├── Resources/
│   ├── Levels/               # Level<number>.txt layouts
│   └── Prefabs/              # Reusable gameplay tile prefabs
└── Scripts/
    ├── GameManager.cs        # Level loading, grid, shift budgets, switches, doors
    ├── Player.cs             # Movement, gravity, hazards, victory
    ├── GameUI.cs             # HUD, compass, instructions, win screen
    ├── MenuUI.cs             # Menu and chamber selection
    ├── CameraController.cs   # Camera tracking and zoom
    ├── Pool.cs               # Prefab lookup
    ├── Tile.cs               # Grid-cell data
    ├── ShiftLockTheme.cs     # Procedural pixel art and UI theme
    ├── ShiftLockAudio.cs     # Synthesized event sounds
    └── LevelEditor/
        ├── LevelEditor.cs
        └── LevelEditorCamera.cs
```

### Level format

Levels are text assets in `Assets/GravityPuzzleGame/Resources/Levels/`. Each nonblank line uses `x,y,tileId`; spikes require a fourth rotation field: `x,y,2,rotation`.

| ID | Tile |
| ---: | --- |
| 0 | Block |
| 1 | Player spawn |
| 2 | Spike |
| 3 | Exit flag |
| 4 | Gravity switch |
| 5 | Locked door |

The runtime uses a 20×20 grid with 9-unit tile spacing. Switches and doors reuse the block prefab but receive distinct generated sprites.

### Shift budgets

In the Game scene, select **GameManager** and edit **Gravity Shifts Per Level**. Array indices use *internal* level IDs: elements **10**, **11**, and **12** control the three displayed chambers. Inspector values saved in the scene override C# field initializers. Levels without a serialized array entry use a four-shift fallback.

### Level Editor

Open `LevelEditor.unity` to place or remove tiles. Keys **5** and **6** select switches and doors, and **R** rotates the placement preview. Saving writes the next unused `Level<number>.txt` without overwriting an existing level. The editor is a development utility rather than part of the normal player flow.

## Visuals and audio

`ShiftLockTheme.cs` generates and caches pixel-art sprites at runtime using Unity `Texture2D`, and applies a dark navy, metallic blue-gray, cyan, red, and green UI palette. `ShiftLockAudio.cs` synthesizes short one-shot sounds for movement, gravity changes, switch activation, death, victory, and menu clicks. **No background music** is used.

## Testing and MVP scope

The three original chambers, menu progression, gravity-shift limits, switches and doors, full-chamber death reset, victory, and audio cues were playtested in Unity. This is a course MVP rather than a production release. Minor decorative overlap remains in the imported menu scene.

## Attribution

SHIFT//LOCK extends the imported **Gravity Puzzle Game** Unity template. The original scenes, movement foundation, camera, prefabs, and level editor provided the starting point. This project adds limited gravity shifts, switch/door mechanics, three original chambers, direct chamber selection, procedural sci-fi visuals, synthesized audio, and related fixes. The original template assets and levels remain in the repository; the game is not presented as built entirely from scratch.

## Course submission

The assignment requires a **gameplay video**, the game available in **Git**, and a **separate text file describing changes from the original one-pager**. This README is technical documentation, not a substitute for that assignment-specific change note.
