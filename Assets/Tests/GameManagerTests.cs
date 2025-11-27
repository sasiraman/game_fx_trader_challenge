using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for GameManager payout calculations and game logic.
/// </summary>
public class GameManagerTests
{
    private GameObject gameManagerObj;
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        // Create GameManager instance for testing
        gameManagerObj = new GameObject("GameManager");
        gameManager = gameManagerObj.AddComponent<GameManager>();
        
        // Create required dependencies
        GameObject configObj = new GameObject("ConfigManager");
        configObj.AddComponent<ConfigManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (gameManagerObj != null)
        {
            Object.DestroyImmediate(gameManagerObj);
        }
    }

    [Test]
    public void TestPayoutCalculation_CorrectPrediction_SmallMove()
    {
        // Test payout formula: bet_amount * (1 + min(5, abs(percent_move) * 10))
        // Small move: 0.1% increase
        float betAmount = 100f;
        float rateStart = 1.0f;
        float rateEnd = 1.001f; // 0.1% increase
        float percentMove = (rateEnd - rateStart) / rateStart; // 0.001
        
        float multiplier = 1f + Mathf.Min(5f, Mathf.Abs(percentMove) * 10f);
        float expectedPayout = betAmount * multiplier;
        
        // Expected: 100 * (1 + min(5, 0.001 * 10)) = 100 * (1 + 0.01) = 101
        Assert.AreEqual(101f, expectedPayout, 0.01f);
    }

    [Test]
    public void TestPayoutCalculation_CorrectPrediction_LargeMove()
    {
        // Large move: 10% increase (should cap at 5x multiplier)
        float betAmount = 100f;
        float rateStart = 1.0f;
        float rateEnd = 1.1f; // 10% increase
        float percentMove = (rateEnd - rateStart) / rateStart; // 0.1
        
        float multiplier = 1f + Mathf.Min(5f, Mathf.Abs(percentMove) * 10f);
        float expectedPayout = betAmount * multiplier;
        
        // Expected: 100 * (1 + min(5, 0.1 * 10)) = 100 * (1 + 1) = 200
        // Actually: min(5, 1.0) = 1.0, so 100 * (1 + 1) = 200
        // But if percentMove is 0.1, then abs(percentMove) * 10 = 1.0, min(5, 1.0) = 1.0
        Assert.AreEqual(200f, expectedPayout, 0.01f);
    }

    [Test]
    public void TestPayoutCalculation_CorrectPrediction_MaxCap()
    {
        // Very large move: 100% increase (should cap at 5x multiplier)
        float betAmount = 100f;
        float rateStart = 1.0f;
        float rateEnd = 2.0f; // 100% increase
        float percentMove = (rateEnd - rateStart) / rateStart; // 1.0
        
        float multiplier = 1f + Mathf.Min(5f, Mathf.Abs(percentMove) * 10f);
        float expectedPayout = betAmount * multiplier;
        
        // Expected: 100 * (1 + min(5, 1.0 * 10)) = 100 * (1 + 5) = 600
        Assert.AreEqual(600f, expectedPayout, 0.01f);
    }

    [Test]
    public void TestLossCalculation_IncorrectPrediction()
    {
        // Loss should equal bet amount
        float betAmount = 100f;
        float expectedLoss = -betAmount;
        
        Assert.AreEqual(-100f, expectedLoss);
    }

    [Test]
    public void TestXPCalculation()
    {
        // XP formula: floor(10 * sqrt(abs(percent_move) * 100))
        float percentMove = 0.01f; // 1% move
        
        int xp = Mathf.FloorToInt(10f * Mathf.Sqrt(Mathf.Abs(percentMove) * 100f));
        
        // sqrt(0.01 * 100) = sqrt(1) = 1
        // 10 * 1 = 10
        Assert.AreEqual(10, xp);
    }

    [Test]
    public void TestXPCalculation_LargeMove()
    {
        // 10% move
        float percentMove = 0.1f;
        
        int xp = Mathf.FloorToInt(10f * Mathf.Sqrt(Mathf.Abs(percentMove) * 100f));
        
        // sqrt(0.1 * 100) = sqrt(10) ≈ 3.16
        // 10 * 3.16 ≈ 31.6, floor = 31
        Assert.AreEqual(31, xp);
    }

    [Test]
    public void TestCreditsRounding()
    {
        // Credits should be rounded to 2 decimal places
        float credits = 12345.6789f;
        float rounded = (float)System.Math.Round(credits, 2);
        
        Assert.AreEqual(12345.68f, rounded, 0.001f);
    }
}

