# Quick Start Guide

Get FX Trader Challenge running in 5 minutes!

## Prerequisites

- Unity LTS 2021.3.x or 2022.3.x
- Node.js (for mock server, optional)

## Step 1: Open in Unity

1. Open Unity Hub
2. Click "Add" → Select this project folder
3. Select Unity LTS version
4. Click "Open"

## Step 2: Import TextMeshPro

1. When prompted, click "Import TMP Essentials"
2. Wait for import to complete

## Step 3: Set Up Scenes

Follow `SCENE_SETUP_GUIDE.md` to create the 5 required scenes, OR:

1. Create empty scenes: Landing, MainMenu, GamePlay, Results, Leaderboard
2. Add Canvas to each scene
3. Add manager GameObjects (ConfigManager, FXFeedManager, GameManager) to Landing scene
4. Add UI controllers to respective scenes

**Note:** For a quick test, you can create minimal UI - the scripts will work with basic setups.

## Step 4: Build Settings

1. File → Build Settings
2. Add all 5 scenes
3. Select WebGL platform
4. Click "Switch Platform" if needed

## Step 5: Build WebGL

**Option A: Using Build Script**
1. Right-click in Project window
2. Select "Build WebGL"
3. Wait for build to complete

**Option B: Manual Build**
1. File → Build Settings
2. Click "Build"
3. Select/create `WebGLBuild` folder

## Step 6: Test Locally

```bash
cd WebGLBuild
python3 -m http.server 8000
# Or: npx http-server -p 8000
```

Open `http://localhost:8000` in browser.

## Step 7: (Optional) Start Mock Server

```bash
cd mock_server
npm install
npm start
```

Then configure game to use backend mode in `index.html` config panel.

## Troubleshooting

### Build Fails
- Ensure WebGL module is installed in Unity Hub
- Check for compilation errors in Console

### Game Doesn't Load
- Check browser console for errors
- Verify `Build.loader.js` exists in `WebGLBuild/Build/`
- Ensure CORS is enabled if using backend API

### UI Not Showing
- Verify Canvas is set up correctly
- Check UI references are assigned in inspector
- Ensure TextMeshPro is imported

### Mock Mode Not Working
- Check `StreamingAssets/config.json` exists
- Verify `mockSeed` is set
- Check Console for FXFeedManager errors

## Next Steps

- Read `README.md` for full documentation
- Check `API_EXAMPLES.md` for backend integration
- Review `SCENE_SETUP_GUIDE.md` for detailed UI setup

