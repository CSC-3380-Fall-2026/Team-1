using NUnit.Framework; // our testing tools
using UnityEngine; // the unity features

public class CourseStrokeTrackerTests
{
    private GameObject testObject; 
    private CourseStrokeTracker tracker; // the stroke tracker

    [SetUp]
    public void SetUp()
    {
        testObject = new GameObject("CourseStrokeTrackerTest"); // this will create new test objects
        tracker = testObject.AddComponent<CourseStrokeTracker>(); // tracker added
        tracker.StartCourse(3); // a course will be added with 3 holes 
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject); // this will clean up any test object
    }

    [Test]
    public void ValidShot_IncreasesStrokeCount()
    {
        tracker.RegisterStroke(); // one shot recorded 

        Assert.AreEqual(1, tracker.CurrentHoleStrokes); // this will check the hole strokes. 
        Assert.AreEqual(1, tracker.TotalStrokes); // checks for total stroke counts. 
    }

    [Test]
    public void Stroke_IsRecordedForCorrectHole()
    {
        tracker.SetCurrentHole(2); // Selects Hole 2
        tracker.RegisterStroke(); // a shot is recorded 

        Assert.AreEqual(0, tracker.GetStrokesForHole(1)); // checking for hole 1
        Assert.AreEqual(1, tracker.GetStrokesForHole(2)); // checking for hole 1
        Assert.AreEqual(0, tracker.GetStrokesForHole(3)); // checking for hole 3
    }

    [Test]
    public void Strokes_PersistWhenChangingHoles()
    {
        tracker.RegisterStroke(); // adding a shot to hole 1 
        tracker.RegisterStroke(); // thsi will add another shot to hole 1 

        tracker.SetCurrentHole(2); // switched to hole 2
        tracker.RegisterStroke(); // added a shot to hole 2 

        Assert.AreEqual(2, tracker.GetStrokesForHole(1)); // this will keep the scores for hole 1
        Assert.AreEqual(1, tracker.GetStrokesForHole(2)); // this will be the scored for hole 2 
    }

    [Test]
    public void TotalStrokes_AddsMultipleHoleScores()
    {
        
        tracker.RegisterStroke(); // 1st shot 
        tracker.RegisterStroke(); // 2nd shot
        tracker.RegisterStroke(); // 3rd shot

        
        tracker.SetCurrentHole(2); // hole 2 is selected 
        tracker.RegisterStroke(); // 1st shot
        tracker.RegisterStroke(); // 2nd shot 
        tracker.RegisterStroke(); // 3rd shot
        tracker.RegisterStroke(); // 4th shot

        
        tracker.SetCurrentHole(3); // hole 3 is selected 
        tracker.RegisterStroke(); 
        tracker.RegisterStroke(); 

        Assert.AreEqual(3, tracker.GetStrokesForHole(1)); // checking hole 1
        Assert.AreEqual(4, tracker.GetStrokesForHole(2)); // checking hole 2
        Assert.AreEqual(2, tracker.GetStrokesForHole(3)); // checking hole 3
        Assert.AreEqual(9, tracker.TotalStrokes); 
    }

    [Test]
    public void StartingNewCourse_ResetsAllStrokeData()
    {
        tracker.RegisterStroke(); // adding a shot to hole 1 
        tracker.SetCurrentHole(2); // hole 2 selected 
        tracker.RegisterStroke(); // adds a shot to hole 2

        tracker.StartCourse(3); // a new course will start 

        Assert.AreEqual(1, tracker.CurrentHole); // this will reset to hole 1 
        Assert.AreEqual(0, tracker.CurrentHoleStrokes); 
        Assert.AreEqual(0, tracker.TotalStrokes); // this will resert every hole 
        Assert.AreEqual(0, tracker.GetStrokesForHole(1)); // hole 1 resets 
        Assert.AreEqual(0, tracker.GetStrokesForHole(2)); // hole 2 resets 
    }
}
