using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple line chart renderer for FX rate visualization using Unity UI.
/// Draws a sparkline-style chart of recent rate history.
/// </summary>
public class ChartRenderer : MonoBehaviour
{
    [Header("Chart Settings")]
    public RectTransform chartContainer;
    public GameObject linePrefab; // Simple line renderer prefab
    public Color lineColor = Color.cyan;
    public float lineWidth = 2f;
    public int maxDataPoints = 50;
    
    private LineRenderer lineRenderer;
    private List<float> rateData = new List<float>();
    private string currentPair = "";
    private float minRate = float.MaxValue;
    private float maxRate = float.MinValue;

    private void Start()
    {
        InitializeChart();
        
        if (FXFeedManager.Instance != null)
        {
            FXFeedManager.Instance.OnRateChanged += OnRateUpdated;
        }
    }

    private void OnDestroy()
    {
        if (FXFeedManager.Instance != null)
        {
            FXFeedManager.Instance.OnRateChanged -= OnRateUpdated;
        }
    }

    /// <summary>
    /// Initializes the chart with a LineRenderer component.
    /// </summary>
    private void InitializeChart()
    {
        // Create LineRenderer if prefab not provided
        if (linePrefab == null)
        {
            GameObject lineObj = new GameObject("ChartLine");
            lineObj.transform.SetParent(chartContainer, false);
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.color = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = false;
            lineRenderer.sortingOrder = 1;
        }
        else
        {
            GameObject lineObj = Instantiate(linePrefab, chartContainer);
            lineRenderer = lineObj.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = lineObj.AddComponent<LineRenderer>();
            }
        }
    }

    /// <summary>
    /// Sets the currency pair to display and loads initial history.
    /// </summary>
    public void SetCurrencyPair(string pair)
    {
        currentPair = pair;
        rateData.Clear();
        
        if (FXFeedManager.Instance != null)
        {
            var history = FXFeedManager.Instance.GetRateHistory(pair, maxDataPoints);
            foreach (var data in history)
            {
                rateData.Add(data.rate);
            }
        }
        
        UpdateChart();
    }

    /// <summary>
    /// Called when FX rate updates.
    /// </summary>
    private void OnRateUpdated(string pair, float rate)
    {
        if (pair == currentPair)
        {
            rateData.Add(rate);
            
            // Keep only last maxDataPoints
            if (rateData.Count > maxDataPoints)
            {
                rateData.RemoveAt(0);
            }
            
            UpdateChart();
        }
    }

    /// <summary>
    /// Updates the chart visualization.
    /// </summary>
    private void UpdateChart()
    {
        if (rateData.Count < 2 || lineRenderer == null || chartContainer == null)
        {
            return;
        }

        // Calculate min/max for scaling
        minRate = float.MaxValue;
        maxRate = float.MinValue;
        foreach (float rate in rateData)
        {
            if (rate < minRate) minRate = rate;
            if (rate > maxRate) maxRate = rate;
        }
        
        // Add padding
        float range = maxRate - minRate;
        if (range < 0.001f) range = 0.001f; // Avoid division by zero
        minRate -= range * 0.1f;
        maxRate += range * 0.1f;
        range = maxRate - minRate;

        // Get container bounds
        Rect rect = chartContainer.rect;
        float width = rect.width;
        float height = rect.height;

        // Set line renderer positions
        lineRenderer.positionCount = rateData.Count;
        
        for (int i = 0; i < rateData.Count; i++)
        {
            float normalizedX = (float)i / (rateData.Count - 1);
            float normalizedY = (rateData[i] - minRate) / range;
            
            // Convert to local space coordinates
            float x = normalizedX * width - width / 2f;
            float y = normalizedY * height - height / 2f;
            
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    /// <summary>
    /// Clears the chart.
    /// </summary>
    public void ClearChart()
    {
        rateData.Clear();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }
}

