using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Drone : Enemy
{

    GameManager gameManager;

    Rigidbody rb;

    //Movement & Rotation Variables
    public float foragingSpeed = 1f;
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;
    public float targetRadius = 200f;

    //Boid Steering/Flocking Variables
    public float separationDistance = 25.0f;
    public float cohesionDistance = 50.0f;
    public float separationStrength = 250.0f;
    public float cohesionStrength = 25.0f;
    private Vector3 cohesionPos = new Vector3(0f, 0f, 0f);
    private int boidIndex = 0;

    public DroneBehaviours droneBehaviour;
    public enum DroneBehaviours
    {
        Idle,
        Scouting,
        Foraging,
        Attacking,
        Fleeing,
        Return,
        Repair
    }
    //Enemy projectile variables

    public GameObject projectilePrefab; 
    public Transform shootPoint;        
    public float shootForce = 700f;     // How fast the projectile moves
    private bool isFiring = false;

    //Drone Behaviour Variables
    public GameObject motherShip;
    public Vector3 scoutPosition;

    //Scout variables
    private float scoutTimer;
    private float detectTimer;
    private float scoutTime = 10.0f;
    private float detectTime = 5.0f;
    private float detectionRadius = 400.0f;
    private int newResourceVal;
    public GameObject newResourceObject;


    private Vector3 tarVel;
    private Vector3 tarPrevPos;
    private Vector3 attackPos;
    private float distanceRatio = 0.05f;

    //Foraging values

    [SerializeField] private float attackOrFlee;
    public GameObject assignedResource;
    public int resourceCapacity = 0;
    public int maxResourceCapacity = 5;
    private bool isMining = false; // To track if the drone is already mining
    // Use this for initialization
    void Start()
    {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        rb = GetComponent<Rigidbody>();

        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Acquire player if spawned in
        if (gameManager.gameStarted)
            {
                target = gameManager.playerDreadnaught;
            //Heuristic function here
            attackOrFlee = health * Friends();
            if (attackOrFlee >= 250)
                droneBehaviour = DroneBehaviours.Attacking;
            else
            {
                droneBehaviour = DroneBehaviours.Fleeing;
            }
        };

        BoidBehaviour();

        switch (droneBehaviour)
        {
            case DroneBehaviours.Scouting:
                Scouting();
                break;
            case DroneBehaviours.Attacking:
                
                Attacking(); break;
            case DroneBehaviours.Fleeing:
                Fleeing();
                break;
            case DroneBehaviours.Foraging:
                Foraging(); 
                break;
            case DroneBehaviours.Return:
                ReturningToMothership(); 
                break;
            case DroneBehaviours.Repair:
                Repair();
                break;

               
        }

    }
    private void Repair()
    {
        // Check if the drone needs repair
        if (health < maxHealth)
        {
            // Move towards the mothership for repairs
            MoveTowardsTarget(motherShip.transform.position);
            Debug.DrawLine(transform.position, motherShip.transform.position, Color.cyan);

            // Get the healing range of the mothership
            Mothership motherShipScript = motherShip.GetComponent<Mothership>();

            // If the drone is within the mothership's healing range, start repairing
            if (Vector3.Distance(transform.position, motherShip.transform.position) <= motherShipScript.healRange)
            {
                // Repair the drone over time or instantly. For simplicity, let's set health to max instantly.
                health = maxHealth;
                Debug.Log("Drone repaired to full health.");
            }
        }

        // If the drone's health is fully restored, switch to attacking state
        if (health >= maxHealth)
        {
            droneBehaviour = DroneBehaviours.Attacking;
            Debug.Log("Drone is fully repaired and switching to Attacking mode.");
        }
    }
    private void Foraging()
    {
        if (assignedResource != null)
        {
            
            // Check the distance between the drone and the assigned resource
            float distanceToResource = Vector3.Distance(transform.position, assignedResource.transform.position);

            // If the drone is within the detection radius of the resource, start mining
            if (distanceToResource <= detectionRadius)
            {
                // Stop moving and start mining
                rb.velocity = Vector3.zero;
                Debug.Log("Drone is foraging at the resource.");

                // Begin mining if not already mining
                if (!isMining)
                {
                    StartCoroutine(Mining());
                }

                
            }
            else
            {
                // If not within detection radius, move towards the resource
                MoveTowardsTarget(assignedResource.transform.position);
                Debug.DrawLine(transform.position, assignedResource.transform.position, Color.blue);
            }
        }
        else
        {
            Debug.LogWarning("No resource assigned for foraging.");
        }
    }

    private void ReturningToMothership()
    {
        if (Vector3.Distance(transform.position, motherShip.transform.position) <= detectionRadius)
        {
            // Transfer resources to the mothership
            Mothership motherShipScript = motherShip.GetComponent<Mothership>();
            motherShipScript.resourceCount += resourceCapacity;

            Debug.Log("Resources transferred to Mothership.");

            // Reset drone resource capacity after transfer
            resourceCapacity = 0;
            // Assign the most suitable asteroid as mining target
            assignedResource = motherShip.GetComponent<Mothership>().resourceObjects[0];
            // Reassign the drone to foraging after resource transfer
            droneBehaviour = DroneBehaviours.Foraging;
            Debug.Log("Drone is returning to foraging.");
        }
        else
        {
            // If not yet in detection range of the mothership, keep moving towards it
            MoveTowardsTarget(motherShip.transform.position);
            
            Debug.DrawLine(transform.position, motherShip.transform.position, Color.green);
        }
    }


    // Mining coroutine that runs every 5 seconds to mine resources
    private IEnumerator Mining()
    {
        isMining = true;

        while (assignedResource != null && resourceCapacity < maxResourceCapacity)
        {
            // Get the Asteroid component from the assigned resource to access its resource count
            Asteroid asteroid = assignedResource.GetComponent<Asteroid>();

            // Check if the resource has at least 1 unit left
            if (asteroid.resource > 0)
            {
                // Mine one resource from the asteroid
                asteroid.resource--;

                // Add one resource to the drone's capacity
                resourceCapacity++;

                Debug.Log("Mining... Drone Resource Capacity: " + resourceCapacity + " / " + maxResourceCapacity);
                Debug.Log("Asteroid Resource Remaining: " + asteroid.resource);
            }
            else
            {
                Debug.Log("Asteroid depleted. Stopping mining.");
                // Stop mining if the asteroid is depleted
                break;
            }

            yield return new WaitForSeconds(10f - 2 * foragingSpeed);
        }

        // Reset mining flag after completing the mining process
        isMining = false;

        // If the drone's capacity is full, return to Mothership or perform other tasks
        if (resourceCapacity >= maxResourceCapacity)
        {
            Debug.Log("Drone resource capacity is full. Returning to Mothership...");
            // Implement logic to return to the mothership or switch behavior
            droneBehaviour = DroneBehaviours.Return;
        }
    }

    //Calculate number of Friendly Units in targetRadius
    private int Friends()
    {
        int clusterStrength = 0;
        for (int i = 0; i < gameManager.enemyList.Length; i++)
        {
            if (Vector3.Distance(transform.position,
            gameManager.enemyList[i].transform.position) < targetRadius)
            {
                clusterStrength++;
            }
        }
        return clusterStrength;
    }
    private void Fleeing()
{
    

    // Set a specific distance to flee away from the target
    float fleeDistance = 1000f; // Adjust this value to determine how far the drone should flee

    // Calculate the flee direction by moving away from the target (ignore Y axis)
    Vector3 fleeDirection = (target.transform.position + transform.position ).normalized;
    fleeDirection.y = 0; // Ignore the Y axis to keep the movement on the X and Z plane

    // Calculate the flee position a specific distance away from the target on the X and Z axis
    Vector3 fleePos = target.transform.position + (-fleeDirection * fleeDistance);

    // Keep the same Y position as the drone's current Y position
    fleePos.y = transform.position.y;

    // Debug to visualize the flee vector
    Debug.DrawLine(transform.position, fleePos, Color.blue);

    // If not at the flee position, move towards it using MoveTowardsTarget method
    if (Vector3.Distance(transform.position, fleePos) > targetRadius)
    {
        MoveTowardsTarget(fleePos);
        Debug.Log("Fleeing");
    }
    else
    {
            Debug.Log("out of range");
            droneBehaviour = DroneBehaviours.Repair;
    }
}
    private IEnumerator FireWeapons()
    {
        isFiring = true;
        yield return new WaitForSeconds(2.5f);
        // Create the projectile 
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        // Get the Rigidbody component of the projectile to apply force
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Apply force to the projectile to move it forward
            rb.AddForce(shootPoint.forward * shootForce);
        }
        isFiring = false;
    }


    private void Attacking()
    {
        //Calculate target's velocity (without using RB)
        tarVel = (target.transform.position - tarPrevPos) / Time.deltaTime;
        tarPrevPos = target.transform.position;
        //Calculate intercept attack position (p = t + r * d * v)
        attackPos = target.transform.position + distanceRatio *
        Vector3.Distance(transform.position, target.transform.position) *
        tarVel;
        attackPos.y = attackPos.y + 10;
        Debug.DrawLine(transform.position, attackPos, Color.red);
        // Not in range of intercept vector - move into position
        if (Vector3.Distance(transform.position, attackPos) > targetRadius)
            MoveTowardsTarget(attackPos);
        else
        {
            //Look at target - Lerp Towards target
            targetRotation = Quaternion.LookRotation(target.transform.position
            - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation,
            targetRotation, adjRotSpeed);
            //Fire Weapons at target
            if (!isFiring)
            {
                StartCoroutine(FireWeapons());
            }
        }
    }
    private GameObject DetectNewResources()
    {
        //Go through list of asteroids and ...
        for (int i = 0; i < gameManager.asteroids.Length; i++)
        {
            //... check if they are within detection distance
            if (Vector3.Distance(transform.position, gameManager.asteroids[i].transform.position) <=
            detectionRadius)
            {
                //Find the best one
                if (gameManager.asteroids[i].GetComponent<Asteroid>().resource >
                newResourceVal)
                {
                    newResourceObject = gameManager.asteroids[i];
                }
            }
        }
        //Double check to see if the Mothership already knows about it and return it if not
        if (motherShip.GetComponent<Mothership>().resourceObjects.Contains(newResourceObject))
        {
            return null;
        }
        else
            return newResourceObject;

    }



    private void Scouting()
    {
        if (!newResourceObject)
        {
            if (Vector3.Distance(transform.position, scoutPosition) < detectionRadius && Time.time > scoutTimer)
            {
                Vector3 position;
                position.x = motherShip.transform.position.x + Random.Range(-1500, 1500);
                position.y = motherShip.transform.position.y + Random.Range(-400, 400);
                position.z = motherShip.transform.position.z + Random.Range(-1500, 1500);
                scoutPosition = position;
                //Update scoutTimer
                scoutTimer = Time.time + scoutTime;
            }
            else
            {
                MoveTowardsTarget(scoutPosition);
                Debug.DrawLine(transform.position, scoutPosition, Color.yellow);
            }
            //Every few seconds, check for new resources
            if (Time.time > detectTimer)
            {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
            }
        }
        //Resource found, head back to Mothership
        else
        {
            target = motherShip;
            MoveTowardsTarget(target.transform.position);
            Debug.DrawLine(transform.position, target.transform.position, Color.green);

            //In range of mothership, relay information and reset to drone again
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
            {
                Mothership motherShipScript = motherShip.GetComponent<Mothership>();

                // Check if the resource is already in the resourceObjects list
                if (!motherShipScript.resourceObjects.Contains(newResourceObject))
                {
                    motherShipScript.resourceObjects.Add(newResourceObject);
                }

                motherShipScript.drones.Add(this.gameObject);
                motherShipScript.scouts.Remove(this.gameObject);

                newResourceVal = 0;
                newResourceObject = null;
                droneBehaviour = DroneBehaviours.Idle;
            }
        }
    }
        private void MoveTowardsTarget(Vector3 targetPos)
    {
        //Rotate and move towards target if out of range
        if (Vector3.Distance(targetPos, transform.position) > targetRadius)
        {

            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
        }
    }
    
    private void BoidBehaviour()
    {
        boidIndex++;
        if (boidIndex >= gameManager.enemyList.Length)
        {
            Vector3 cohesiveForce = (cohesionStrength / Vector3.Distance(cohesionPos,
            transform.position)) * (cohesionPos - transform.position);
            //Apply Force
            rb.AddForce(cohesiveForce);
            //Reset boidIndex
            boidIndex = 0;
            //Reset cohesion position
            cohesionPos.Set(0f, 0f, 0f);
        }
        //Currently analysed boid variables
        Vector3 pos = gameManager.enemyList[boidIndex].transform.position;
        Quaternion rot = gameManager.enemyList[boidIndex].transform.rotation;
        float dist = Vector3.Distance(transform.position, pos);
        if (dist > 0f)
        {
            //If within separation
            if (dist <= separationDistance)
            {
                //Compute scale of separation
                float scale = separationStrength / dist;
                //Apply force to ourselves
                rb.AddForce(scale * Vector3.Normalize(transform.position - pos));
            }
            //Otherwise if within cohesion distance of other boids
            else if (dist < cohesionDistance && dist > separationDistance)
            {
                //Calculate the current cohesionPos
                cohesionPos = cohesionPos + pos * (1f / (float)gameManager.enemyList.Length);
                //Rotate slightly towards current boid
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
            }
        }

    }

}
