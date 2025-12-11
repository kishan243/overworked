using UnityEngine;

/// <summary>
/// Test script to validate core game functionality.
/// Attach to an empty GameObject and check the console for test results.
/// </summary>
public class GameTests : MonoBehaviour
{
    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("=== Starting Game Tests ===");
        
        TestToyCreation();
        TestToyProgress();
        TestWorkStation();
        TestToyCompletion();
        
        Debug.Log("=== All Tests Complete ===");
    }

    void TestToyCreation()
    {
        Debug.Log("Test: Toy Creation");
        
        var components = new System.Collections.Generic.List<string> { "Wood", "Wheels", "Paint" };
        Toy toy = new Toy("Test Car", components);
        
        Assert(toy.toyName == "Test Car", "Toy name should be 'Test Car'");
        Assert(toy.requiredComponents.Count == 3, "Should have 3 components");
        Assert(!toy.isCompleted, "Should not be completed initially");
        Assert(toy.currentComponentIndex == 0, "Should start at index 0");
        
        Debug.Log("✓ Toy Creation Test Passed");
    }

    void TestToyProgress()
    {
        Debug.Log("Test: Toy Progress");
        
        var components = new System.Collections.Generic.List<string> { "Wood", "Wheels", "Paint" };
        Toy toy = new Toy("Test Car", components);
        
        // Test getting next component
        Assert(toy.GetNextRequiredComponent() == "Wood", "First component should be Wood");
        
        // Test adding correct component
        bool result = toy.AddComponent("Wood");
        Assert(result, "Should accept correct component");
        Assert(toy.currentComponentIndex == 1, "Index should advance to 1");
        Assert(toy.GetProgress() == 1f/3f, "Progress should be 1/3");
        
        // Test adding wrong component
        result = toy.AddComponent("Paint");
        Assert(!result, "Should reject wrong component");
        Assert(toy.currentComponentIndex == 1, "Index should stay at 1");
        
        // Test adding correct component
        result = toy.AddComponent("Wheels");
        Assert(result, "Should accept correct component");
        Assert(toy.currentComponentIndex == 2, "Index should advance to 2");
        
        Debug.Log("✓ Toy Progress Test Passed");
    }

    void TestToyCompletion()
    {
        Debug.Log("Test: Toy Completion");
        
        var components = new System.Collections.Generic.List<string> { "Wood", "Paint" };
        Toy toy = new Toy("Simple Toy", components);
        
        toy.AddComponent("Wood");
        Assert(!toy.isCompleted, "Should not be completed yet");
        
        toy.AddComponent("Paint");
        Assert(toy.isCompleted, "Should be completed");
        Assert(toy.GetProgress() == 1f, "Progress should be 100%");
        
        // Test adding to completed toy
        bool result = toy.AddComponent("Extra");
        Assert(!result, "Should not accept components when completed");
        
        Debug.Log("✓ Toy Completion Test Passed");
    }

    void TestWorkStation()
    {
        Debug.Log("Test: WorkStation");
        
        GameObject stationObj = new GameObject("TestStation");
        WorkStation station = stationObj.AddComponent<WorkStation>();
        station.stationType = "Wood";
        station.componentProduced = "Wood";
        station.interactionTime = 1f;
        
        GameObject userObj = new GameObject("TestUser");
        
        // Test availability
        Assert(station.IsAvailable(), "Station should be available initially");
        
        // Test starting interaction
        bool started = station.StartInteraction(userObj.transform);
        Assert(started, "Should start interaction");
        Assert(!station.IsAvailable(), "Station should not be available during interaction");
        
        // Test cannot start second interaction
        GameObject user2Obj = new GameObject("TestUser2");
        started = station.StartInteraction(user2Obj.transform);
        Assert(!started, "Should not start second interaction");
        
        // Test interaction progress
        bool complete = station.UpdateInteraction(0.5f);
        Assert(!complete, "Should not be complete at 0.5s");
        Assert(station.GetProgress() == 0.5f, "Progress should be 50%");
        
        complete = station.UpdateInteraction(0.5f);
        Assert(complete, "Should be complete at 1.0s");
        
        // Test completion
        string result = station.CompleteInteraction();
        Assert(result == "Wood", "Should return Wood component");
        Assert(station.IsAvailable(), "Station should be available after completion");
        
        Destroy(stationObj);
        Destroy(userObj);
        Destroy(user2Obj);
        
        Debug.Log("✓ WorkStation Test Passed");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("ASSERTION FAILED: " + message);
        }
    }
}
