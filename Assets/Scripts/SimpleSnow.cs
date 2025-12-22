using UnityEngine;

public class SimpleSnow : MonoBehaviour
{
    void Start()
    {
        // Add the Particle System
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();


        // 1. Point the object straight down
        transform.position = new Vector3(0, 10, -22);
        transform.rotation = Quaternion.Euler(90, 0, 0);

        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = 6f;        // Constant fall speed
        main.startSize = 0.1f;
        main.gravityModifier = 0f;   // Set to 0 so they don't speed up/drift
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 78f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        // The 20x20 area is now horizontal because of the 90-degree rotation
        shape.scale = new Vector3(40, 2, 1);

        // Apply a simple material
        var renderer = GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        ps.Play();
    }
}