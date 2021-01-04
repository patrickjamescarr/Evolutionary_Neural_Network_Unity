using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    #region Public scene manager inputs

    [Space(5)]
    [Header("Game Settings")]

    // amount of game speed dependant time that each generation shall run for
    public float battleDuration;

    // game speed
    [Range(0.1f, 10f)] public float gameSpeed = 5f;

    // Set the AI to control mode. When set, AI is controlled by a state machine instead of the neural network
    public bool controlMode = false;

    [Space(5)]
    [Header("Network Settings")]
    [Space(20)]

    // population size
    [Range(10, 60)] public int populationSize = 60;

    // hidden layer configuration
    public int[] hiddenLayers = { 8 };

    // mutation variables
    [Range(0.0001f, 1f)] public float mutationChance = 0.03f;
    [Range(0f, 1f)] public float mutationStrength = 0.01f;

    [Space(5)]
    [Header("Battle Instance")]
    [Space(20)]

    // reference to the instance prefab
    public GameObject instancePrefab;

    [Space(5)]
    [Header("UI Labels")]
    [Space(20)]

    // Output panel labels
    public Text generationsLabel;
    public Text fitnessLabel;
    public Text changeInFitnessLabel;
    public Text greenTeamWinsLabel;
    public Text survivedLabel;
    public Text avgFitnessLabel;
    public Text avgKillsLabel;
    public Text avgDmgGivenLabel;
    public Text avgDmgTakenLabel;
    public Text avgSurvivalTimeLabel;
    public Text fittestFitnessLabel;
    public Text fittestKillsLabel;
    public Text fittestDmgGivenLabel;
    public Text fittestDmgTakenLabel;
    public Text fittestSurvivalTimeLabel;
    public Text timeLabel;
    #endregion

    // the networks belonging to each AI
    public List<NeuralNetwork> networks;

    // row size for rendering in the instances in the scene
    private readonly int rowSize = 10;

    // the learning AIs from each instance 
    private List<LearningAIController> learningAIs;

    // how many generations have passed
    private int generationCount = 0;

    // network layer configuration
    private readonly int inputLayers = 7;
    private readonly int outputLayers = 4;

    // variables for recording some in-game stats
    private float generationStartTime = 0f;
    private float elapsedTime = 0f;
    private float previousAvgFitness = 0f;
    private StringBuilder avgFitnessCsv; 

    private void Update()
    {
        elapsedTime = Time.time - generationStartTime;
        timeLabel.text = "Time: " + elapsedTime.ToString("0.##");
    }

    // Start is called before the first frame update
    void Start()
    {
        avgFitnessCsv = new StringBuilder();
        avgFitnessCsv.AppendLine("Generations, Max fitness, Avg. fitness, Min fitness, % Won, % Survived, Avg. Survival Time");



        InitialiseNetworks();

        // Call the NewGeneration function to repeat in the given time frame
        InvokeRepeating(nameof(NewGeneration), 0.1f, battleDuration);
    }

    /// <summary>
    /// 
    /// </summary>
    private void InitialiseNetworks()
    {
        networks = new List<NeuralNetwork>();

        var layers = InitialiseNetworkLayers();

        for (int i = 0; i < populationSize; i++)
        {
            var network = new NeuralNetwork(layers);
            //network.Load("Assets/Results/Run_2.txt"); // load a previously saved network configuration
            networks.Add(network);
        }
    }

    /// <summary>
    /// Initialises the training network instance with the given layer configuration.
    /// </summary>
    private int[] InitialiseNetworkLayers()
    {
        int[] layers = new int[hiddenLayers.Length + 2];

        layers[0] = inputLayers;

        for (int i = 1; i <= hiddenLayers.Length; i++)
        {
            layers[i] = hiddenLayers[i - 1];
        }

        layers[hiddenLayers.Length + 1] = outputLayers;

        return layers;
    }

    /// <summary>
    /// Spawns a new generation
    /// </summary>
    private void NewGeneration()
    {
        // capture the current scaled game time 
        var currentTime = Time.time;

        generationStartTime = currentTime;

        elapsedTime = 0.0f;

        // Set the game speed
        Time.timeScale = gameSpeed;

        // record metrics and sort networks from the previous generation
        if (learningAIs != null)
        {
            RecordMetrics(currentTime);
            if(!controlMode)
            {
                SortNetworks();
            }
        }

        // create the new instances
        CreateInstances();
    }

    /// <summary>
    /// creates the battle instances and assigns a network 
    /// to the learning AI within each instance
    /// </summary>
    private void CreateInstances()
    {
        int instanceCount = 0;

        learningAIs = new List<LearningAIController>();

        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < rowSize; j++)
            {
                // create a new instance
                var battleInstance = Instantiate(instancePrefab, new Vector3((12 * i) + (13.5f * j), (6 * i) - (6.75f * j), 0), new Quaternion(0, 0, 0, 0));

                // get the learning AI from the instance so we can keep a reference to it 
                var learningAi = battleInstance.GetComponentInChildren<LearningAIController>();

                learningAi.isControl = controlMode;

                // add a network to each learning AI
                learningAi.network = networks[instanceCount];

                learningAIs.Add(learningAi);

                instanceCount++;

                // check if all instances have been created and return if so
                if (instanceCount == populationSize)
                {
                    // perform a pathfinding graph scan of the new instances
                    AstarPath.active.Scan();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RecordMetrics(float currentTime)
    {
        generationCount++;
        generationsLabel.text = "Generations: " + generationCount;

        int greenTeamWins = 0;
        float avgFitness = 0f;
        float avgKills = 0f;
        float avgDamageGiven = 0f;
        float avgDamageTaken = 0f;
        float avgSurvivalTime = 0f;
        int survivalCount = 0;

        int aiCount = learningAIs.Count;

        float maxFitness = 0;
        float minFitness = 0;
        int fittestIndex = 0;

        for (int i = 0; i < aiCount; i++)
        {
            var learningAi = learningAIs[i];

            if (learningAi.TeamWon)
            {
                greenTeamWins++;
            }

            if (learningAi.State != States.Dead)
            {
                survivalCount++;
            }

            avgKills += learningAi.Kills;
            avgDamageGiven += learningAi.DamageGiven;
            avgDamageTaken += learningAi.DamageTaken;
            avgSurvivalTime += learningAi.GetSurvivalTime(currentTime);

            // Set the fitness for each learning AI
            var fitness = learningAIs[i].UpdateFitness(currentTime);

            avgFitness += fitness;

            if (fitness > maxFitness)
            {
                maxFitness = fitness;
                fittestIndex = i;
            }

            if (fitness < minFitness)
            {
                minFitness = fitness;
            }

            // Remove all existing instances from the scene
            Destroy(learningAi.transform.root.gameObject);
        }

        var fittest = learningAIs[fittestIndex];

        if (fittest != null)
        {
            fittestFitnessLabel.text = "Fitness: " + maxFitness;
            fittestKillsLabel.text = "Kills: " + fittest.Kills;
            fittestDmgGivenLabel.text = "Damage given: " + fittest.DamageGiven;
            fittestDmgTakenLabel.text = "Damage taken: " + fittest.DamageTaken;
            fittestSurvivalTimeLabel.text = "Survival time: " + fittest.GetSurvivalTime(currentTime);
        }

        fitnessLabel.text = "Highest fitness: " + fittest.network.fitness;

        avgKills /= aiCount;
        avgDamageGiven /= aiCount;
        avgDamageTaken /= aiCount;
        avgSurvivalTime /= aiCount;
        avgFitness /= aiCount;

        var change = ((avgFitness - previousAvgFitness) / avgFitness) * 100;

        var changeInFitness = generationCount > 1 ? change : 0;

        previousAvgFitness = avgFitness;

        var winRatio = ((float)greenTeamWins / (float)populationSize) * 100.0f;
        var survivalRatio = (float)survivalCount / (float)populationSize * 100.0f;

        avgFitnessCsv.AppendLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", generationCount, maxFitness, avgFitness, minFitness, winRatio, survivalRatio, avgSurvivalTime));

        File.WriteAllText("Assets/Results/FitnessPerGeneration.csv", avgFitnessCsv.ToString());

        changeInFitnessLabel.text = "Change in avg fitness (%): " + changeInFitness;
        avgFitnessLabel.text = "Fitness: " + avgFitness;
        avgKillsLabel.text = "Kills: " + avgKills;
        avgDmgGivenLabel.text = "Damage given: " + avgDamageGiven;
        avgDmgTakenLabel.text = "Damage taken: " + avgDamageTaken;
        avgSurvivalTimeLabel.text = "Survival time: " + avgSurvivalTime;
        greenTeamWinsLabel.text = "Green team win %: " + winRatio;
        survivedLabel.text = "Learning AIs survival %: " + survivalRatio;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SortNetworks()
    {
        // sort the networks according to fitness
        networks.Sort();

        // save the network weights and biases of the fittest instance to file
        networks[populationSize - 1].Save("Assets/Save.txt");

        // Keep the fittest half of the population and use them to breed some new ones
        for (int i = 0; i < populationSize / 2; i++)
        {
            var randomParentIndex1 = Random.Range(populationSize / 2, populationSize);
            var randomParentIndex2 = Random.Range(populationSize / 2, populationSize);

            // Perform crossover from two random parents in the fit half of the population
            networks[i] = networks[randomParentIndex1].Crossover(networks[randomParentIndex2]);

            // mutate
            networks[i].Mutate(mutationChance, mutationStrength);
        }
    }
}
