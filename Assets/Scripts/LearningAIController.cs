using UnityEngine;

public class LearningAIController : AICharacterController
{
    public NeuralNetwork network;

    // input values for the neural network
    private float[] input = new float[7];

    public GameObject healthBar;
    public GameObject magicBar;

    public bool isControl = false;

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

    public bool TeamWon
    {
        get
        {
            return EnemyCount() == 0;
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

        UpdateHealthAndMagicBar();

        if(isControl)
        {
            SetState();
        }
        else
        {
            if (Health <= 0)
            {
                SetStateDead();
            }
            else if (EnemyCount() == 0)
            {
                SetStateVictory();
            }
            else
            {
                FeedForward();
            }
        }

        // perform action
        Act();

        Animate();
    }

    private void FeedForward()
    {
        // has target
        //input[0] = hasTarget ? 1f : 0f;
        input[0] = EnemyCount();

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
        int stateValue = CalculateOutputState(output);

        state = (States)stateValue;
    }

    private static int CalculateOutputState(float[] output)
    {
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

        return stateValue;
    }

    private void UpdateHealthAndMagicBar()
    {
        var healthScale = healthBar.transform.localScale;
        healthScale.x = (Health / 10);
        healthBar.transform.localScale = healthScale;

        var magicScale = magicBar.transform.localScale;
        magicScale.x = (Magic / 10);
        magicBar.transform.localScale = magicScale;
    }

    public float GetSurvivalTime(float currentTime)
    {
        return state.Equals(States.Dead) ? timeOfDeath - timeOfCreation : currentTime - timeOfCreation;
    }

    public float UpdateFitness(float currentTime)
    {
        float fitness = 0;

        // fitness based on decision making in certain circumstances
        var baseFitness = attackFitness + fleeFitness + healthFitness + magicFitness;

        fitness += baseFitness;

        // fitness bonus for every kill
        fitness += Kills * (baseFitness / 5);

        // fitness bonus for following through on magic and health decision making
        fitness += (goodHealthPickup + goodMagicPickup) * (baseFitness / 5);

        // large bonus if they win the battle
        fitness += EnemyCount() == 0 ? fitness : 0;

        network.fitness = fitness;

        return fitness;
    }
}
