# Training AI Agents Using An Evolutionary Neural Network

The application demonstrates how
an evolutionary neural network can be used to train an AI to make decisions
based on the state of the game world. In order to achieve this, the game engine
Unity has been used to create a basic deathmatch style game scene in which the
AI can be trained.


## The Deathmatch Instance



Two teams of AI characters fight
in a deathmatch; the Green team and the Purple team. The winning team is the
first to kill all the enemies on the opposing team. The AIs have health and
magic levels. When they attack an enemy, their magic level is decreased by one.
When they are attacked, their health level is decreased by one. Additional
health and magic items are available to collect around the edges of the
battlefield.  All but one of the NPC’s
actions are controlled using a basic state machine. If they have good health
and magic levels, they will seek out an enemy opponent and attack. If their
magic has ran out and their health is ok, they will find some magic. If their
health is low, they will find health. If their health is low and they have an
attacker in firing range, they will flee.



## The learning AI



One of the NPCs on the green team
is a special learning AI. This is the AI that will be trained using an evolutionary
neural network to make decisions that will allow it to rival, or better the AIs
controlled by the state machine. The learning AI has the same abilities as the
state machine AIs, where it can attack a near-by enemy, find health, find magic
and flee. The output from the neural network will determine which of those states
it should enter at each frame update during the game. In order to assess the
learning AIs performance vs a standard AI, the learning AI can be run in
control mode. When running in control mode, the learning AI behaves like the
rest of the AIs on the map, where its actions are determined by a state
machine. This allows metrics to be recorded in both learning and control mode
for comparison.



## Application Workflow



1. Parameters for the network and genetic algorithm, along with the game settings are first
set through a Scene Manager interface in the Unity scene. For the network, the
hidden layer configuration can be adjusted. For the genetic algorithm
parameters that can be adjusted are population size, mutation chance and
mutation strength. Game settings that can be adjusted are battle duration and
game speed. 



2. neural network instances are created with a user-specified layer configuration, where  refers to the population size. Their
weights and biases are assigned a small random value between -0.5 and 0.5.



3. deathmatch instances are created in a grid formation in the scene. Each deathmatch instance comprises of a basic square battlefield
with some health and magic collectable items scattered around the edges. Six AI
characters are placed in the battlefield, three on each team. One of the AIs on
the Green team is the learning AI.



4. The learning AI within each deathmatch instance is assigned a neural network



5. The battles in each instance play out for a given period. During gameplay, the state
of the game world from the perspective of the learning AI is fed into its
network as inputs on each frame update. The data fed into the network is the AIs
health and magic levels, if there is health and magic available on the map, how
many enemies are present, if they have an enemy pursuing them and the distance
to their pursuer. These values are fed into the network’s feedforward algorithm
and the output is used to set the AI state. The state will be one of Attack,
Flee, Find Health or Find Magic.



6. After the time has elapsed, the fitness of each learning AI is calculated. The
networks are then sorted by their fitness level.



7. Once the networks have been sorted, the poorest performing half of the population
are removed from the gene pool. Random crossover is then performed on the
remaining networks with parents picked at random to make up the missing
numbers. During crossover, weights and biases are picked at random from both
parent networks to create a new child network to add into the population.



8. Finally, after a new network has been added into the population, it is subject to random
mutation of some of its weights and biases. The chance that mutation will occur
on a weight or bias is user configurable, along with the upper and lower limit
of the possible strength of the mutation. 



9. Once all the discarded networks have been replaced, the existing deathmatch
instances are destroyed, a new set is created, and the cycle starts again. 

## Fitness Function 

The fitness of the AI is
calculated in order to reward good decision making with positive fitness and
punish poor decision making with negative fitness. The AI’s decision-making
ability is determined by comparing the state that has been output by the
network with the input data at each update. A fitness score is accumulated for
each possible output throughout the duration of each generation, and then added
together when the AI fitness function is called to form the base fitness level.
The four output fitness levels are calculated as follows:

    
### Attack fitness
- When the attack state is output by the network, attack
fitness is incremented when the AI’s magic level is greater than zero and, if
health is available, when its health level is above a certain level. Otherwise,
attack fitness is decremented.
    
### Flee fitness
- When the flee state is output by the network,
flee fitness is incremented when the AI has a pursuer, its health is below a
certain level and its pursuer is within attacking range. Otherwise, flee
fitness is decremented.

### Health fitness
- When the find health state is output by the
network, health fitness is incremented when the AIs health is below a certain
level. Otherwise, health fitness is decremented.

### Magic fitness
- When the find magic state is output by the
network, magic fitness is incremented when the AIs magic is below a certain
level. Otherwise, magic fitness is decremented.

Along with the base fitness
level, fitness bonuses are also awarded for each enemy AI that the AI kills,
when the AI follows through on a correct decision to collect health or magic and
if all enemy AIs are killed. 

## Stopping point

In order to determine a stopping
point for GA, the average fitness over each generation was recorded and
observed. As can been seen in the results, the average fitness fluctuates heavily,
while still showing an increasing trend in the fitness level. Refinement of the
network and genetic variables did not show any noticeable stabilisation in the
fluctuations. When comparing the average fitness per generation data from several
runs, it can be observed that by 180 generations the trend in the average
fitness level has either flattened out, or started to dip, making 180 generations
a reasonable stopping point.
