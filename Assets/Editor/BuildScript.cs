using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Editor script for building WebGL with custom settings.
/// </summary>
public class BuildScript
{
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        string buildPath = "WebGLBuild";
        
        // Clean build directory
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }
        
        // Build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        
        // Build
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            
            // Copy index.html to build directory
            CopyIndexHTML(buildPath);
            
            // Copy StreamingAssets if needed
            CopyStreamingAssets(buildPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }
    }

    private static string[] GetScenes()
    {
        // Get all scenes in build settings
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }

    private static void CopyIndexHTML(string buildPath)
    {
        string sourceHTML = Path.Combine(Application.dataPath, "..", "index.html");
        string destHTML = Path.Combine(buildPath, "index.html");
        
        if (File.Exists(sourceHTML))
        {
            File.Copy(sourceHTML, destHTML, true);
            Debug.Log($"Copied index.html to {destHTML}");
        }
        else
        {
            Debug.LogWarning($"index.html not found at {sourceHTML}. Creating default.");
            CreateDefaultIndexHTML(destHTML);
        }
    }

    private static void CopyStreamingAssets(string buildPath)
    {
        string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
        string destPath = Path.Combine(buildPath, "StreamingAssets");
        
        if (Directory.Exists(streamingAssetsPath))
        {
            Directory.CreateDirectory(destPath);
            CopyDirectory(streamingAssetsPath, destPath);
            Debug.Log($"Copied StreamingAssets to {destPath}");
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }
        
        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }

    private static void CreateDefaultIndexHTML(string path)
    {
        string html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <title>FX Trader Challenge</title>
    <style>
        body { margin: 0; padding: 20px; font-family: Arial, sans-serif; background: #1a1a1a; color: #fff; }
        #unityContainer { margin: 20px auto; }
        #configPanel { background: #2a2a2a; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
        .config-item { margin: 10px 0; }
        label { display: inline-block; width: 150px; }
        input, select { padding: 5px; width: 200px; }
        button { padding: 8px 15px; background: #4CAF50; color: white; border: none; border-radius: 3px; cursor: pointer; }
        button:hover { background: #45a049; }
    </style>
</head>
<body>
    <div id=""configPanel"">
        <h3>Configuration</h3>
        <div class=""config-item"">
            <label>Mode:</label>
            <select id=""modeSelect"">
                <option value=""mock"">Mock Mode</option>
                <option value=""backend"">Backend Mode</option>
            </select>
        </div>
        <div class=""config-item"">
            <label>API URL:</label>
            <input type=""text"" id=""apiUrl"" value=""http://localhost:3000"" />
        </div>
        <div class=""config-item"">
            <label>Mock Seed:</label>
            <input type=""number"" id=""mockSeed"" value=""12345"" />
        </div>
        <button onclick=""applyConfig()"">Apply Config</button>
    </div>
    <div id=""unityContainer""></div>
    <script>
        var buildUrl = ""Build"";
        var loaderUrl = buildUrl + ""/Build.loader.js"";
        var config = {
            dataUrl: buildUrl + ""/Build.data"",
            frameworkUrl: buildUrl + ""/Build.framework.js"",
            codeUrl: buildUrl + ""/Build.wasm"",
            streamingAssetsUrl: ""StreamingAssets"",
            companyName: ""FX Trader"",
            productName: ""FX Trader Challenge"",
            productVersion: ""1.0""
        };

        function applyConfig() {
            var mode = document.getElementById('modeSelect').value;
            var apiUrl = document.getElementById('apiUrl').value;
            var mockSeed = parseInt(document.getElementById('mockSeed').value);
            
            // Store config in localStorage for Unity to read
            localStorage.setItem('gameConfig', JSON.stringify({
                useBackend: mode === 'backend',
                apiBaseUrl: apiUrl,
                mockSeed: mockSeed
            }));
            
            alert('Config applied! Reload page to apply changes.');
        }

        var container = document.querySelector(""#unityContainer"");
        var canvas = document.createElement(""canvas"");
        var loadingBar = document.createElement(""div"");
        loadingBar.style.cssText = ""width: 100%; height: 100%; background: #2a2a2a; display: flex; align-items: center; justify-content: center;"";
        loadingBar.innerHTML = ""<div>Loading...</div>"";
        container.appendChild(loadingBar);

        var script = document.createElement(""script"");
        script.src = loaderUrl;
        script.onload = () => {
            createUnityInstance(canvas, config, (progress) => {
                loadingBar.style.display = ""none"";
            }).then((instance) => {
                container.appendChild(canvas);
                loadingBar.remove();
            }).catch((message) => {
                alert(message);
            });
        };
        document.body.appendChild(script);
    </script>
</body>
</html>";
        
        File.WriteAllText(path, html);
    }
}

