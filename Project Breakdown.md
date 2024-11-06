Bee’s Algorithm Breakdown
Scouting & Foraging
Drones are continually built being produced by the mothership (up to the maximum of 35), provided that there are a suitable number of resources
Twenty drones are produced on start
Of these twenty drones, four of them are assigned to the scout list using a fitness heuristic which allows for only the fastest drones to become scouts
These scouts engage in the scouting behaviour in which they will survey the vicinity of the mothership for asteroids (flower patches)
Upon locating an asteroid the drone will return to the mothership with the location of this asteroid before returning to scouting for additional asteroids
 The mothership stores these locations and relays them to forager drones which are chosen based on a similar heuristic to the scouts but with foragingSpeed in mind
The forager drones will navigate to the asteroid with the largest quantity of resources and being mining for resources
Upon reaching the maximum capacity that a drone can hold, they will return to the mothership and transfer their resources before once again heading to the asteroid with the largest quantity of resources




Flocking / Swarm Behaviours 

Drones employ boid-based flocking behaviours to move as a group whilst avoiding collisions with on another
Drones utilise a separation value which keeps them a minimum distance from one another to prevent collisions, accomplished by applying a repulsive force
Drones make use of a cohesion value which encourages them to move as a unit by moving towards the average position of the drones neighbours
Drones also employ a alignment property to align the direction of movement to ensure they move in approximately the same direction 



Predator / Prey Behaviours
The drones use a heuristic that accounts for their total remaining health and the number of nearby friendly drones to determine whether they should attack the player or retreat. When attacking the player, the drones will maintain formation and shoot lasers at the player, however if the heuristic determines a drone is in danger they will stray from the swarm and retreat before returning back to the mothership for repairs. Once repaired the drone will attempt to rejoin the swarm and continue attacking the player. This system aims to imitate a sense of self preservation within the drones which allows for more engaging fights.
