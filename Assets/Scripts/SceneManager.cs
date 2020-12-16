using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public float timeframe;

    // population size
    public int populationSize = 50;

    // the learning AI prefab
    public GameObject learingAI;

    // initialise the network to the rquired size
    public int[] layers = new int[3] { 5, 3, 2 };

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    public List<NeuralNetwork> networks;

    public GameObject InstancePrefab;

    private List<Instance> instances;

    // Start is called before the first frame update
    void Start()
    {
        populationSize = 50;
        
    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            var network = gameObject.AddComponent<NeuralNetwork>();
            //net.Load("Assets/Pre-trained.txt");//on start load the network save
            networks.Add(network);
        }
    }

    public void CreateInstances()
    {
        // Set the game speed
        Time.timeScale = Gamespeed;

        if (instances != null)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                // Clean up existing instances
                Destroy(instances[i]);
            }

           SortNetworks();
        }

        instances = new List<Instance>();

        for (int i = 0; i < populationSize; i++)
        {
            var instance = (Instantiate(InstancePrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0))).GetComponent<Instance>();

            // Add network to each learning AI
            instance.learningAI.network = networks[i];

            instances.Add(instance);
        }

    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            // Set the fitness for each learning AI
            instances[i].learningAI.UpdateFitness();
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
