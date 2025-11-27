# FX Trader Challenge - Unity WebGL Game

A gamified FX learning simulation game built with Unity for WebGL deployment. Players use virtual credits to predict FX moves across currency corridors.

## Features

- **Virtual Trading**: Predict FX movements with virtual credits
- **Multiple Currency Pairs**: Trade USD→SGD, USD→INR, EUR→USD
- **Flexible Time Horizons**: 30s, 60s, or 5-minute predictions
- **Dual Mode Operation**: 
  - Mock mode with deterministic seeded RNG (works offline)
  - Backend API mode for real FX rates and persistence
- **Leaderboards**: Track top players and your rank
- **Badges & Rewards**: Earn XP and unlock achievements
- **Responsive UI**: Works across common browser sizes

## Project Structure

```
game_fx_trader_challenge/
├── Assets/
│   ├── Scripts/
│   │   ├── Managers/          # Core game managers
│   │   ├── Game/               # Game logic and mechanics
│   │   ├── UI/                 # UI controllers
│   │   ├── API/                # Backend integration
│   │   └── Utils/              # Utilities and helpers
│   ├── Scenes/                 # Unity scenes
│   ├── Prefabs/                # UI prefabs
│   ├── Resources/              # Config and resources
│   └── StreamingAssets/        # Config files
├── WebGLBuild/                 # WebGL build output
├── mock_server/                # Mock backend server
├── index.html                  # WebGL wrapper
└── README.md                   # This file
```

## Prerequisites

- Unity LTS 2021.3.x or 2022.3.x
- TextMeshPro package (included in Unity)
- Git

## Setup Instructions

### 1. Open in Unity

1. Open Unity Hub
2. Click "Add" and select this project folder
3. Select Unity LTS version (2021.3+ or 2022.3+)
4. Open the project

### 2. Configure TextMeshPro

1. Unity will prompt to import TextMeshPro essentials - click "Import"
2. Import TextMeshPro Examples & Extras if needed

### 3. Build WebGL

1. File → Build Settings
2. Select "WebGL" platform
3. Click "Switch Platform" if needed
4. Click "Build" and select `WebGLBuild/` folder

Alternatively, use the provided build script:
- Open `Assets/Editor/BuildScript.cs`
- Right-click in Project window → "Build WebGL"

### 4. Host the Build

#### Option A: Simple HTTP Server

```bash
cd WebGLBuild
python3 -m http.server 8000
# Or: npx http-server -p 8000
```

Open `http://localhost:8000` in browser

#### Option B: Nginx

```nginx
server {
    listen 80;
    server_name localhost;
    root /path/to/WebGLBuild;
    index index.html;
    
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # CORS headers for API calls
    add_header Access-Control-Allow-Origin *;
}
```

## Configuration

### Mock Mode (Default)

The game works out-of-the-box in mock mode. No configuration needed.

### Backend API Mode

1. Edit `Assets/StreamingAssets/config.json`:
```json
{
  "useBackend": true,
  "apiBaseUrl": "https://your-api.com",
  "mockSeed": 12345,
  "initialCredits": 10000,
  "availableCorridors": ["USD_SGD", "USD_INR", "EUR_USD"]
}
```

2. Or configure via `index.html` config panel (opens automatically)

## Mock Server (Development)

A simple Node.js mock server is included for testing backend integration:

```bash
cd mock_server
npm install
npm start
```

Server runs on `http://localhost:3000` with endpoints:
- `POST /auth/login` - Returns JWT token
- `GET /api/fx_rates` - Returns mock FX rates
- `GET /api/fx_rates/history?pair=USD_SGD` - Historical data
- `POST /api/game/score` - Persist score
- `GET /api/leaderboard` - Leaderboard data

## Game Mechanics

### Payout Formula

**Correct Prediction:**
```
payout = bet_amount * (1 + min(5, abs(percent_move) * 10))
```

**Incorrect Prediction:**
```
loss = bet_amount
```

**XP Reward:**
```
xp = floor(10 * sqrt(abs(percent_move) * 100))
```

Where `percent_move = (rate_end - rate_start) / rate_start`

## API Endpoints

### Authentication
```
POST /auth/login
Body: { "username": "guest_123", "password": "optional" }
Response: { "token": "jwt_token_here" }
```

### FX Rates
```
GET /api/fx_rates
Response: {
  "timestamp": "2025-11-27T12:00:00Z",
  "rates": {
    "USD_SGD": 1.3567,
    "USD_INR": 83.45,
    "EUR_USD": 1.088
  }
}
```

### Leaderboard
```
GET /api/leaderboard
Response: [
  { "rank": 1, "username": "alice", "score": 12500 },
  { "rank": 2, "username": "bob", "score": 11000 }
]
```

### Score Persistence
```
POST /api/game/score
Headers: { "Authorization": "Bearer <token>" }
Body: {
  "username": "guest_123",
  "score": 15200,
  "credits": 124500,
  "stats": { "wins": 12, "losses": 3 }
}
```

## Testing

Run unit tests:
1. Window → General → Test Runner
2. Select "EditMode" tests
3. Click "Run All"

## Development

### Key Scripts

- `FXFeedManager.cs` - Handles FX rate feeds (Mock/Backend)
- `GameManager.cs` - Core game logic and state
- `ConfigManager.cs` - Configuration management
- `APIClient.cs` - REST API integration
- `ChartRenderer.cs` - Simple line chart visualization

### Adding New Currency Pairs

Edit `Assets/StreamingAssets/config.json`:
```json
{
  "availableCorridors": ["USD_SGD", "USD_INR", "EUR_USD", "GBP_USD"]
}
```

## Troubleshooting

### WebGL Build Issues

- Ensure WebGL module is installed in Unity Hub
- Check browser console for errors
- Verify CORS headers if using backend API

### Mock Mode Not Deterministic

- Ensure `mockSeed` is set in config
- Mock generator uses seeded RNG for reproducibility

### API Connection Issues

- Check `apiBaseUrl` in config
- Verify CORS is enabled on backend
- Check browser network tab for failed requests

## License

MIT License - See LICENSE file for details

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes
4. Add tests
5. Submit a pull request

