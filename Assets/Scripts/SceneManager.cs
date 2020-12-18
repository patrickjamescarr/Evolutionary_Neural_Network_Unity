using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SceneManager : MonoBehaviour
{
    public float timeframe;

    // population size
    [Range(10, 50)] public int populationSize = 50;

    // initialise the network to the rquired size
    public int[] layers = new int[3] { 8, 5, 5 };

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    public List<NeuralNetwork> networks;

    public GameObject InstancePrefab;

    private int rowSize = 10;

    private List<LearningAIController> learningAIs;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseNetworks();

        InvokeRepeating("CreateInstances", 0.1f, timeframe);//repeating function
    }

    private void CreateInstances()
    {
        // Set the game speed
        Time.timeScale = Gamespeed;

        if (learningAIs != null)
        {
            for (int i = 0; i < learningAIs.Count; i++)
            {
                // Clean up existing instances
                Destroy(learningAIs[i]);
            }

            SortNetworks();
        }

        int instanceCount = 0;

        learningAIs = new List<LearningAIController>();
        
        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < rowSize; j++)
            {
                var instance = Instantiate(InstancePrefab, new Vector3((12 * i) + (13.5f * j), (6 * i) - (6.75f * j), 0), new Quaternion(0, 0, 0, 0));

                var learningAi = instance.GetComponentInChildren<LearningAIController>();

                // Add network to each learning AI
                learningAi.network = networks[i];

                learningAIs.Add(learningAi);

                instanceCount++;

                if (instanceCount == populationSize) return;
            }
        }

        AstarPath.active.Scan();
    }

    public void InitialiseNetworks()
    {
        networks = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            var network = new NeuralNetwork(layers);
            //net.Load("Assets/Pre-trained.txt");//on start load the network save
            networks.Add(network);
        }
    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            // Set the fitness for each learning AI
            learningAIs[i].UpdateFitness();
        }

        networks.Sort();

        // networks[populationSize - 1].Save("Assets/Save.txt");//saves networks weights and biases to file, to preserve network performance

        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));

            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }
}
