using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Toy
{
    public string toyName;
    public List<string> requiredComponents;
    public bool isCompleted;
    public int currentComponentIndex;

    public Toy(string name, List<string> components)
    {
        toyName = name;
        requiredComponents = components;
        isCompleted = false;
        currentComponentIndex = 0;
    }

    public bool AddComponent(string componentType)
    {
        if (isCompleted || currentComponentIndex >= requiredComponents.Count)
            return false;

        if (requiredComponents[currentComponentIndex] == componentType)
        {
            currentComponentIndex++;
            if (currentComponentIndex >= requiredComponents.Count)
            {
                isCompleted = true;
            }
            return true;
        }
        return false;
    }

    public string GetNextRequiredComponent()
    {
        if (currentComponentIndex < requiredComponents.Count)
            return requiredComponents[currentComponentIndex];
        return "";
    }

    public float GetProgress()
    {
        return (float)currentComponentIndex / requiredComponents.Count;
    }
}
