using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public int resource = 50;
    public int maxResource = 100; // Max limit for resource regeneration
    private float regenerateTimer = 0f;
    public float regenerateInterval = 10f; // Time interval for regenerating resources (10 seconds)

    // Start is called before the first frame update
    void Start()
    {
        // Initialize resource to a random value between 10 and 100
        resource = Random.Range(10, 100);
    }

    // Update is called once per frame
    void Update()
    {
        RegenerateResource();
    }

    void RegenerateResource()
    {
        // Only regenerate if the current resource is less than the maxResource
        if (resource < maxResource)
        {
            // Increase the timer by the time passed since the last frame
            regenerateTimer += Time.deltaTime;

            // If the timer exceeds the regenerateInterval (10 seconds), regenerate 1 resource
            if (regenerateTimer >= regenerateInterval)
            {
                resource += 1;
                // Reset the timer after resource is regenerated
                regenerateTimer = 0f;
            }
        }
    }
}
