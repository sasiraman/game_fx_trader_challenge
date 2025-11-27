# Project Structure

```
game_fx_trader_challenge/
│
├── Assets/
│   ├── Scripts/
│   │   ├── Managers/
│   │   │   ├── ConfigManager.cs          # Configuration management
│   │   │   ├── FXFeedManager.cs          # FX rate feed (Mock/Backend)
│   │   │   └── GameManager.cs             # Core game logic
│   │   │
│   │   ├── API/
│   │   │   └── APIClient.cs               # REST API client
│   │   │
│   │   ├── UI/
│   │   │   ├── LandingScreenController.cs
│   │   │   ├── MainMenuController.cs
│   │   │   ├── GamePlayController.cs
│   │   │   ├── ResultsScreenController.cs
│   │   │   ├── LeaderboardController.cs
│   │   │   └── ChartRenderer.cs           # FX rate chart visualization
│   │   │
│   │   └── Utils/
│   │       ├── Analytics.cs               # Event tracking
│   │       └── SceneLoader.cs             # Scene management utilities
│   │
│   ├── Scenes/                             # Unity scene files
│   │   ├── Landing.unity
│   │   ├── MainMenu.unity
│   │   ├── GamePlay.unity
│   │   ├── Results.unity
│   │   └── Leaderboard.unity
│   │
│   ├── Prefabs/                            # UI prefabs
│   │   ├── CorridorButton.prefab
│   │   └── LeaderboardEntry.prefab
│   │
│   ├── Resources/                          # Runtime resources
│   │
│   ├── StreamingAssets/                   # Config files
│   │   └── config.json
│   │
│   ├── Tests/                              # Unit tests
│   │   ├── GameManagerTests.cs
│   │   └── FXFeedManagerTests.cs
│   │
│   └── Editor/                             # Editor scripts
│       └── BuildScript.cs                  # WebGL build automation
│
├── WebGLBuild/                             # WebGL build output (generated)
│   ├── Build/
│   ├── StreamingAssets/
│   └── index.html
│
├── mock_server/                            # Mock backend server
│   ├── server.js
│   ├── package.json
│   └── README.md
│
├── index.html                              # WebGL wrapper with config panel
│
├── .gitignore                              # Git ignore rules
├── README.md                               # Main documentation
├── QUICK_START.md                          # Quick start guide
├── SCENE_SETUP_GUIDE.md                    # Scene setup instructions
├── API_EXAMPLES.md                         # API usage examples
└── PROJECT_STRUCTURE.md                    # This file
```

## Key Components

### Managers

- **ConfigManager**: Loads and manages game configuration from JSON
- **FXFeedManager**: Handles FX rate feeds (mock or backend API)
- **GameManager**: Core game logic (bets, predictions, credits, XP, badges)

### API Integration

- **APIClient**: REST client for backend communication
  - Authentication
  - FX rates fetching
  - Score persistence
  - Leaderboard retrieval

### UI Controllers

Each scene has a controller script that manages UI interactions:
- Landing → Login/guest flow
- MainMenu → Corridor selection, stats display
- GamePlay → Betting interface, chart, timer
- Results → Outcome display, rewards
- Leaderboard → Top players display

### Utilities

- **Analytics**: Event tracking system
- **ChartRenderer**: Simple line chart for FX rates
- **SceneLoader**: Scene navigation helpers

## Data Flow

1. **Configuration**: `config.json` → `ConfigManager` → Game systems
2. **FX Rates**: `FXFeedManager` (Mock/Backend) → `GameManager` → UI
3. **Game State**: `GameManager` → `PlayerPrefs` (local) / `APIClient` (backend)
4. **UI Updates**: Events → Controllers → UI elements

## Build Process

1. Unity Editor → Build Script → WebGL Build
2. Build output → `WebGLBuild/`
3. `index.html` copied to build directory
4. Serve via HTTP server

## Testing

- Unit tests in `Assets/Tests/`
- Run via Unity Test Runner (Window → General → Test Runner)
- Mock server for backend integration testing

