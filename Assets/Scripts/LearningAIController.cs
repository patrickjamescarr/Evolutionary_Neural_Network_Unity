using UnityEngine;

public class LearningAIController : AICharacterController
{
    public NeuralNetwork network;

    // input values for the neural network
    private float[] input = new float[7];

    public GameObject healthBar;
    public GameObject magicBar;

    public States State
    {
        get
        {
            return state;
        }
    }

    public int Kills
    {
        get
        {
            return enemiesKilled;
        }
    }

    public int DamageGiven
    {
        get
        {
            return hitsLanded;
        }
    }

    public int DamageTaken
    {
        get
        {
            return hitsTaken;
        }
    }

    private void Awake()
    {
        OnAwake(); 
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEnemyStatus();
        UpdateHealthAndMagicStatus();

        var healthScale = healthBar.transform.localScale;
        healthScale.x = (Health / 10);
        healthBar.transform.localScale = healthScale;

        var magicScale = magicBar.transform.localScale;
        magicScale.x = (Magic / 10);
        magicBar.transform.localScale = magicScale;

        if (Health <= 0)
        {
            SetStateDead();
        }
        else if(enemies.Length == 0)
        {
            SetStateVictory();
        }
        else
        {
            // has target
            //input[0] = hasTarget ? 1f : 0f;
            input[0] = enemies.Length;

            // has target
            input[1] = hasPersuer ? 1f : 0f;

            // distance to persuer
            input[2] = distanceToPersuer;

            // current health
            input[3] = Health;

            // current magic
            input[4] = Magic;

            // health available
            input[5] = healthAvailable ? 1f : 0f;

            // magic available
            input[6] = magicAvailable ? 1f : 0f;

            // feed forward the input in the network
            float[] output = network.FeedForward(input);

            // find the strongest suggested output from the network
            float maxValue = 0;
            int stateValue = 0;

            for (int i = 0; i < output.Length; i++)
            {
                var currentValue = output[i];

                if (i == 0)
                {
                    maxValue = currentValue;
                    stateValue = i + 1;
                    continue;
                }

                if (currentValue > maxValue)
                {
                    maxValue = currentValue;
                    stateValue = i + 1;
                }
            }

            state = (States)stateValue;
        }

        // perform action
        Act();

        Animate();
    }

    public float GetSurvivalTime(float currentTime)
    {
        return state.Equals(States.Dead) ? timeOfDeath - timeOfCreation : currentTime - timeOfCreation;
    }

    public float UpdateFitness(float currentTime)
    {
        //fitness = CaclulateTimeFitness();

        //float fitness = KillsDamageGivenAndTake();

        float fitness = 0;

        //fitness += KillsDamageGivenAndTake();

        //fitness += GetSurvivalTime(currentTime);

        fitness += hitsLanded;


        fitness += (enemies.Length == 0 ? 50 : 0);

        //fitness += Kills * 10;

        //fitness += (attackFitness + hitsLanded + fleeFitness + healthFitness + magicFitness);

        network.fitness = fitness; //updates fitness of network for sorting

        return fitness;
    }

    private float KillsDamageGivenAndTake()
    {
        return enemiesKilled + hitsLanded - hitsTaken;
    }

    private float CaclulateTimeFitness()
    {
        float fitness;
        if (state.Equals(States.Dead))
        {
            fitness = timeOfDeath - timeOfCreation;
        }
        else
        {
            fitness = Time.time - timeOfCreation;
        }

        return fitness;
    }
}
