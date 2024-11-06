using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mothership : MonoBehaviour
{
    private float forageTimer;
    private float forageTime = 5.0f;

    public List<GameObject> resourceObjects = new List<GameObject>();
    public int resourceCount = 0; // Resources available for the mothership
    public GameObject enemy; // Prefab for the drone/enemy
    public int numberOfEnemies = 20;

    public GameObject spawnLocation;

    public List<GameObject> drones = new List<GameObject>();
    public List<GameObject> scouts = new List<GameObject>();
    public List<GameObject> foragers = new List<GameObject>();
    public int maxScouts = 4;
    public int maxForagers = 4;
    public int maxDrones = 30; // Limit for the number of drones that can be created

    public float healRange = 50f; // The range within which the mothership can heal drones

    void Start()
    {
        // Start coroutine to create a drone every 10 seconds if resources are available
        StartCoroutine(GenerateDrones());

        for (int i = 0; i < numberOfEnemies; i++)
        {
            CreateDrone();
        }
    }

    void Update()
    {
        SelectScouts();
        SelectForagers();
        SortResources();
        HealDronesInRange(); // Check and heal drones within range
    }

    // Coroutine to generate a drone every 10 seconds
    private IEnumerator GenerateDrones()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (resourceCount >= 10 && drones.Count < maxDrones)
            {
                // Deduct 10 resources
                resourceCount -= 10;

                // Create the drone
                CreateDrone();

                Debug.Log("Created a new drone. Resources remaining: " + resourceCount);
            }
            else if (resourceCount < 10)
            {
                Debug.Log("Not enough resources to create a new drone.");
            }
            else if (drones.Count >= maxDrones)
            {
                Debug.Log("Reached maximum drone capacity.");
            }
        }
    }

    // Helper method to create a new drone
    private void CreateDrone()
    {
        Vector3 spawnPosition = spawnLocation.transform.position;

        spawnPosition.x = spawnPosition.x + Random.Range(-50, 50);
        spawnPosition.y = spawnPosition.y + Random.Range(-50, 50);
        spawnPosition.z = spawnPosition.z + Random.Range(-50, 50);

        GameObject newDrone = Instantiate(enemy, spawnPosition, spawnLocation.transform.rotation) as GameObject;

        // Randomize drone foraging speed and movement speed
        newDrone.GetComponent<Drone>().speed = Random.Range(50, 100);
        newDrone.GetComponent<Drone>().foragingSpeed = Random.Range(1, 2);

        drones.Add(newDrone);
    }

    private void SelectScouts()
    {
        //(Re)Initialise Scouts Continuously
        if (scouts.Count < maxScouts)
        {
            //Sort drones by their speed value
            drones.Sort((a, b) => b.GetComponent<Drone>().speed.CompareTo(a.GetComponent<Drone>().speed));
            scouts.Add(drones[0]);
            drones.Remove(drones[0]);
            scouts[scouts.Count - 1].GetComponent<Drone>().droneBehaviour =
            Drone.DroneBehaviours.Scouting;
        }
    }

    private void SelectForagers()
    {
        if (resourceObjects.Count > 0)
        {
            //Assign foragers if not at maximum capacity
            if (foragers.Count < maxForagers)
            {
                //Sort drones by their foraging speed value
                drones.Sort((a, b) => b.GetComponent<Drone>().foragingSpeed.CompareTo(a.GetComponent<Drone>().foragingSpeed));
                foragers.Add(drones[0]);
                drones.Remove(drones[0]);
                foragers[foragers.Count - 1].GetComponent<Drone>().droneBehaviour =
                    Drone.DroneBehaviours.Foraging;
                //Assign the object with the highest amount of resources to the foragers
                foragers[foragers.Count - 1].GetComponent<Drone>().assignedResource = resourceObjects[0];
            }
        }
        else
        {
            return;
        }
    }

    private void SortResources()
    {
        //(Re)Determine best resource objects periodically
        if (resourceObjects.Count > 0 && Time.time > forageTimer)
        {
            //Sort resource objects delegated by their resource amount in decreasing order
            resourceObjects.Sort(delegate (GameObject a, GameObject b) {
                return
                (b.GetComponent<Asteroid>().resource).CompareTo(a.GetComponent<Asteroid>().resource);
            });
            forageTimer = Time.time + forageTime;
        }
    }

    // New method to check and heal drones within range
    private void HealDronesInRange()
    {
        foreach (GameObject drone in drones)
        {
            float distance = Vector3.Distance(transform.position, drone.transform.position);

            // Check if the drone is within the healing range and is not at full health
            Enemy droneComponent = drone.GetComponent<Enemy>();
            if (distance <= healRange && droneComponent.health < droneComponent.maxHealth)
            {
                if (resourceCount >= 1)
                {
                    // Restore drone health and deduct 1 resource
                    droneComponent.health = droneComponent.maxHealth;
                    resourceCount -= 1;
                    Debug.Log("Healed drone. Resources remaining: " + resourceCount);
                }
                else
                {
                    Debug.Log("Not enough resources to heal drones.");
                }
            }
        }
    }
    // Draw healing range as a wireframe sphere in the scene view
    private void OnDrawGizmosSelected()
    {
        // Set Gizmos color for healing range
        Gizmos.color = Color.green;

        // Draw a wire sphere at the mothership's position representing the healing range
        Gizmos.DrawWireSphere(transform.position, healRange);
    }
}
