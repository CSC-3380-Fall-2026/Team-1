using NUnit.Framework;

public class PersistentStrokeCountTests // test persistent stroke tracking across multiple holes
{
    [SetUp]
    public void SetUp()
    {
        CourseStrokeTracker.StartNewCourse();
    }

    [TearDown]
    public void TearDown()
    {
        CourseStrokeTracker.StartNewCourse();
    }

    
    [Test]
    public void RecordStroke_IncreasesStrokeCount() // verifies that recording a valid stroke increases the count
    {
        CourseStrokeTracker.RecordStroke(0, 0);

        Assert.AreEqual(
            1,
            CourseStrokeTracker.GetHoleStrokeCount(0, 0)
        );
    }

    [Test]
    public void RecordStroke_TracksCorrectHole() // verifies that strokes are assigned to the correct hole
    {
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(1, 0);

        Assert.AreEqual(2, CourseStrokeTracker.GetHoleStrokeCount(0, 0));
        Assert.AreEqual(1, CourseStrokeTracker.GetHoleStrokeCount(1, 0));
        Assert.AreEqual(0, CourseStrokeTracker.GetHoleStrokeCount(2, 0));
    }

    [Test]
    public void RecordStroke_PersistsBetweenHoles() // verifies that stroke records records remain available when progressing to another hole
    {
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(0, 0);

        CourseStrokeTracker.SetCurrentHoleIndex(1);

        CourseStrokeTracker.RecordStroke(1, 0);

        Assert.AreEqual(2, CourseStrokeTracker.GetHoleStrokeCount(0, 0));
        Assert.AreEqual(1, CourseStrokeTracker.GetHoleStrokeCount(1, 0));
    }

    
    [Test]
    public void GetTotalStrokes_CalculatesCourseTotal() // verifies that strokes from multiple holes are added together
    {
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(0, 0);

        CourseStrokeTracker.RecordStroke(1, 0);
        CourseStrokeTracker.RecordStroke(1, 0);
        CourseStrokeTracker.RecordStroke(1, 0);
        CourseStrokeTracker.RecordStroke(1, 0);

        CourseStrokeTracker.RecordStroke(2, 0);
        CourseStrokeTracker.RecordStroke(2, 0);

        Assert.AreEqual(9, CourseStrokeTracker.GetPlayerTotalStrokes(0));
        Assert.AreEqual(9, CourseStrokeTracker.GetTotalStrokes());
    }

    
    [Test]
    public void StartNewCourse_ResetsStrokeData() // verifies that starting a new course clears previous stroke records
    {
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(1, 0);
        CourseStrokeTracker.RecordStroke(1, 1);

        CourseStrokeTracker.StartNewCourse();

        Assert.AreEqual(0, CourseStrokeTracker.GetHoleStrokeCount(0, 0));
        Assert.AreEqual(0, CourseStrokeTracker.GetHoleStrokeCount(1, 0));
        Assert.AreEqual(0, CourseStrokeTracker.GetHoleStrokeCount(1, 1));
        Assert.AreEqual(0, CourseStrokeTracker.GetTotalStrokes());
        Assert.AreEqual(0, CourseStrokeTracker.CurrentHoleIndex);
    }

    
    [Test]
    public void RecordStroke_TracksPlayersSeparately() // verifies that different players have separate stroke totals
    {
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(0, 0);
        CourseStrokeTracker.RecordStroke(0, 1);

        Assert.AreEqual(2, CourseStrokeTracker.GetPlayerTotalStrokes(0));
        Assert.AreEqual(1, CourseStrokeTracker.GetPlayerTotalStrokes(1));
        Assert.AreEqual(3, CourseStrokeTracker.GetTotalStrokes());
    }
}
