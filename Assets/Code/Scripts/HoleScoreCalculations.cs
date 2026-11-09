using NUnit.Framework;
using UnityEngine;


public class HoleScoreCalculationTest
{
    
    private const float Tolerance = 0.5f;

    private Hole _hole;

    
    [SetUp]
    public void SetUp() // creates a hole with the known values before every test
    {
        _hole = ScriptableObject.CreateInstance<Hole>();
        _hole.BaseScore = 100000f;
        _hole.ShotCountScoreFactor = 0.9f;
        _hole.TimeSpentScoreFactor = 0.99f;
        _hole.TimeScoreInterval = 1.0f;
    }

    
    // removes the hole after each test
   
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_hole);
    }

    
    // no shots and no time means no decrease
    
    [Test]
    public void CalculateScore_NoShotsNoTime_ReturnsBaseScore()
    {
        Assert.AreEqual(100000f, _hole.CalculateScore(0, 0f), Tolerance);
    }
 
    [Test]
    public void CalculateScore_OneShot_AppliesShotFactorOnce()
    {
        Assert.AreEqual(90000f, _hole.CalculateScore(1, 0f), Tolerance);
    }

    [Test]
    public void CalculateScore_DefaultInterval_AppliesTimeFactorPerSecond() // each second the score is multiplied by the time factor
    {
        Assert.AreEqual(99000f, _hole.CalculateScore(0, 1f), Tolerance);
        Assert.AreEqual(97029.9f, _hole.CalculateScore(0, 3f), Tolerance);
    }

    [Test]
    public void CalculateScore_ShotsAndTime_AppliesBothFactors() // shots and time lower the score 
    {
        
        Assert.AreEqual(79388.1f, _hole.CalculateScore(2, 2f), Tolerance);
    }

    [Test]
    public void CalculateScore_LongerInterval_DecreasesLessOften() // a longer interval means the score will drop less often
    {
        _hole.TimeScoreInterval = 5.0f;

        Assert.AreEqual(99000f, _hole.CalculateScore(0, 5f), Tolerance);
        Assert.AreEqual(98010f, _hole.CalculateScore(0, 10f), Tolerance);
    }

    
    [Test]
    public void CalculateScore_MoreTime_GivesLowerScore() // more time taken always gives a lower score
    {
        float quick = _hole.CalculateScore(1, 5f);
        float slow = _hole.CalculateScore(1, 30f);

        Assert.Less(slow, quick);
    }


    [Test]
    public void CalculateScore_InvalidInterval_FallsBackToOneSecond() // a invalid interval (0 or -) falls back to 1 second instead of breaking the score
    {
        _hole.TimeScoreInterval = 0f;

        float score = _hole.CalculateScore(0, 2f);

        Assert.IsFalse(float.IsNaN(score));
        Assert.IsFalse(float.IsInfinity(score));
        Assert.AreEqual(98010f, score, Tolerance);
    }
}
