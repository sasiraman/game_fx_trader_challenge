# Unity Scene Setup Guide

This guide explains how to set up the Unity scenes for FX Trader Challenge.

## Required Scenes

Create the following scenes in `Assets/Scenes/`:

1. **Landing** - Login/landing screen
2. **MainMenu** - Main menu with corridor selection
3. **GamePlay** - Gameplay screen with chart and betting
4. **Results** - Results/rewards screen
5. **Leaderboard** - Leaderboard display

## Scene Setup Steps

### 1. Landing Scene

1. Create new scene: `File → New Scene → Basic (Built-in)`
2. Save as `Assets/Scenes/Landing.unity`
3. Add Canvas (UI → Canvas)
4. Add UI elements:
   - Title text (TextMeshPro)
   - Username input field (InputField - TextMeshPro)
   - Password input field (InputField - TextMeshPro)
   - "Guest Login" button
   - "Backend Login" button
   - Tutorial panel (initially hidden)
5. Add `LandingScreenController` script to an empty GameObject
6. Assign UI references in the inspector

### 2. MainMenu Scene

1. Create new scene: `File → New Scene → Basic (Built-in)`
2. Save as `Assets/Scenes/MainMenu.unity`
3. Add Canvas
4. Add UI elements:
   - Credits display (TextMeshPro)
   - Username display (TextMeshPro)
   - Stats panel (wins, losses, streak, XP)
   - Corridor selection area (Vertical Layout Group)
   - Corridor button prefab (create one, duplicate for each corridor)
   - "Play" button
   - "Leaderboard" button
   - Badges panel
5. Add `MainMenuController` script to an empty GameObject
6. Assign UI references

### 3. GamePlay Scene

1. Create new scene: `File → New Scene → Basic (Built-in)`
2. Save as `Assets/Scenes/GamePlay.unity`
3. Add Canvas
4. Add UI elements:
   - Currency pair label (TextMeshPro)
   - Current rate display (TextMeshPro)
   - Credits display (TextMeshPro)
   - Chart container (RectTransform with Image background)
   - "BUY" button
   - "SELL" button
   - Bet amount slider
   - Bet amount text display
   - Time horizon dropdown
   - Timer text
   - Prediction status panel (initially hidden)
   - "Back" button
5. Add `GamePlayController` script to an empty GameObject
6. Add `ChartRenderer` script to chart container
7. Assign UI references

### 4. Results Scene

1. Create new scene: `File → New Scene → Basic (Built-in)`
2. Save as `Assets/Scenes/Results.unity`
3. Add Canvas
4. Add UI elements:
   - Result text (WIN/LOSS) (TextMeshPro)
   - Credits change display (TextMeshPro)
   - XP earned display (TextMeshPro)
   - Current credits display (TextMeshPro)
   - Badge unlocked panel (initially hidden)
   - "Continue" button
   - "Leaderboard" button
5. Add `ResultsScreenController` script to an empty GameObject
6. Assign UI references

### 5. Leaderboard Scene

1. Create new scene: `File → New Scene → Basic (Built-in)`
2. Save as `Assets/Scenes/Leaderboard.unity`
3. Add Canvas
4. Add UI elements:
   - Leaderboard container (Vertical Layout Group)
   - Leaderboard entry prefab (rank, username, score)
   - Player rank display (TextMeshPro)
   - "Refresh" button
   - "Back" button
5. Add `LeaderboardController` script to an empty GameObject
6. Assign UI references

## Manager Setup

In each scene, add these manager GameObjects (or use a persistent scene):

1. **ConfigManager** GameObject with `ConfigManager` script
2. **FXFeedManager** GameObject with `FXFeedManager` script
3. **GameManager** GameObject with `GameManager` script
4. **APIClient** GameObject with `APIClient` script (if using backend)
5. **Analytics** GameObject with `Analytics` script

**Note:** These managers use `DontDestroyOnLoad`, so you can add them to the first scene (Landing) and they'll persist across scenes.

## Build Settings

1. Open `File → Build Settings`
2. Add all scenes in order:
   - Landing
   - MainMenu
   - GamePlay
   - Results
   - Leaderboard
3. Select WebGL platform
4. Click "Switch Platform" if needed

## TextMeshPro Setup

1. When you first add a TextMeshPro component, Unity will prompt to import TMP Essentials
2. Click "Import TMP Essentials"
3. Optionally import "TMP Examples & Extras" for more fonts

## Chart Renderer Setup

The chart uses Unity's LineRenderer component:

1. Create an empty GameObject as child of chart container
2. Add LineRenderer component
3. Set material: `Sprites/Default` shader
4. Set color and width
5. Assign to `ChartRenderer.linePrefab` or let it create automatically

## Prefabs

Create prefabs for reusable UI elements:

1. **CorridorButton** - Button with TextMeshPro label
2. **LeaderboardEntry** - Panel with rank, username, score text

## Testing

1. Set Landing scene as the first scene in Build Settings
2. Press Play in Editor
3. Test each scene transition
4. Verify UI updates correctly

## Tips

- Use Canvas Scaler set to "Scale With Screen Size" for responsive UI
- Use Layout Groups for automatic positioning
- Set Canvas Render Mode to "Screen Space - Overlay" for UI
- Use EventSystem (automatically added with Canvas) for button interactions

