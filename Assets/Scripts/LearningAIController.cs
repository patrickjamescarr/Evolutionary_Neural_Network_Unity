using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearningAIController : AICharacterController
{
    public NeuralNetwork network;

    // input values for the neural network
    private float[] input = new float[8];

    private void Awake()
    {
        OnAwake();   
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEnemyStatus();
        UpdateHealthAndMagicStatus();

        if(Health <= 0)
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
            input[0] = hasTarget ? 1f : 0f;

            // has target
            input[1] = hasPersuer ? 1f : 0f;

            // distance to target
            input[2] = distanceToTarget;

            // distance to persuer
            input[3] = distancePersuer;

            // current health
            input[4] = Health;

            // current magic
            input[5] = Magic;

            // health available
            input[6] = healthAvailable ? 1f : 0f;

            // magic available
            input[7] = magicAvailable ? 1f : 0f;

            // feed forward the input in the network
            float[] output = network.FeedForward(input);

            // find the strongest suggested output from the network
            float maxValue = 0;
            int maxValueIndex = 0;

            for (int i = 0; i < output.Length; i++)
            {
                var currentValue = output[i];

                if (i == 0)
                {
                    maxValue = currentValue;
                    maxValueIndex = i;
                    continue;
                }

                if (currentValue > maxValue)
                {
                    maxValue = currentValue;
                    maxValueIndex = i;
                }
            }

            state = (States)maxValueIndex;
        }

        // perform action
        Act();

        Animate();
    }

    public void UpdateFitness()
    {
        float fitness;

        if (state.Equals(States.Dead))
        {
            fitness = timeOfDeath - timeOfCreation - (enemies.Length == 0 ? -10 : 30);
        }
        else if (state.Equals(States.Victory))
        {
            fitness = timeOfVictory - timeOfCreation + 30;
        }
        else
        {
            fitness = Time.time - timeOfCreation;
        }

        network.fitness = fitness;//updates fitness of network for sorting
    }

}
