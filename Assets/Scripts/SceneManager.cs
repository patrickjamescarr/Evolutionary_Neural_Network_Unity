using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    #region Public scene manager inputs

    // amount of game speed dependant time that each iteration shall run for
    public float timeframe;

    // game speed
    [Range(0.1f, 100f)] public float Gamespeed = 1f;

    // population size
    [Range(10, 50)] public int populationSize = 50;

    // network layer configuration {<input layer>, <hidden layers>..., <output layer>}
    public int[] layers;

    // mutation variables
    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;
    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    // reference to the instance prefab
    public GameObject InstancePrefab;

    // Output panel labels
    public Text interationsLabel;
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
    private int rowSize = 10;

    // the learning AIs from each instance 
    private List<LearningAIController> learningAIs;

    // how many interations/generations have passed
    private int iterationCount = 0;

    private float interationStartTime = 0f;
    private float elapsedTime = 0f;

    private float previousAvgFitness= 0f;

    private void Update()
    {
        elapsedTime = Time.time - interationStartTime;
        timeLabel.text = "Time: " + elapsedTime.ToString("0.##");
    }

    // Start is called before the first frame update
    void Start()
    {
        InitialiseNetworks();

        // Call the CreateInstances function to repeat in the given time frame
        //InvokeRepeating(nameof(NewIteration), 0.1f, timeframe);

        StartCoroutine(NewIteration());
    }

    /// <summary>
    /// 
    /// </summary>
    private void InitialiseNetworks()
    {
        networks = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            var network = new NeuralNetwork(layers);
            //network.Load("Assets/Results/Hits_WinBonus_AutoTargetAttack.txt");//on start load the network save
            networks.Add(network);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator NewIteration()
    {
        while (true)
        {
            // Let the engine run for a frame.
            yield return null;

            // capture the current scaled game time 
            var currentTime = Time.time;

            interationStartTime = currentTime;

            elapsedTime = 0.0f;

            // Set the game speed
            Time.timeScale = Gamespeed;

            // record metrics and sort networks from the previous iteration
            if (learningAIs != null)
            {
                RecordMetrics(currentTime);
                SortNetworks(currentTime);
            }

            // create the new instances
            CreateInstances();

            yield return new WaitForSeconds(timeframe);
        }
    }

    /// <summary>
    /// 
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
                var instance = Instantiate(InstancePrefab, new Vector3((12 * i) + (13.5f * j), (6 * i) - (6.75f * j), 0), new Quaternion(0, 0, 0, 0));

                // get the learning AI from the instance so we can keep a reference to it 
                var learningAi = instance.GetComponentInChildren<LearningAIController>();

                // Add a network to each learning AI
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
    /// <param name="currentTime"></param>
    private void RecordMetrics(float currentTime)
    {
        iterationCount++;
        interationsLabel.text = "Interations: " + iterationCount;

        int greenTeamWins = 0;
        float avgFitness = 0f;
        float avgKills = 0f;
        float avgDamageGiven = 0f;
        float avgDamageTaken = 0f;
        float avgSurvivalTime = 0f;
        int survivalCount = 0;

        int aiCount = learningAIs.Count;

        float maxFitness = 0;
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

        var changeInFitness = iterationCount > 1 ? change : 0;

        previousAvgFitness = avgFitness;

        changeInFitnessLabel.text = "Change in avg fitness (%): " + changeInFitness;
        avgFitnessLabel.text = "Fitness: " + avgFitness;
        avgKillsLabel.text = "Kills: " + avgKills;
        avgDmgGivenLabel.text = "Damage given: " + avgDamageGiven;
        avgDmgTakenLabel.text = "Damage taken: " + avgDamageTaken;
        avgSurvivalTimeLabel.text = "Survival time: " + avgSurvivalTime;
        greenTeamWinsLabel.text = "Green team wins: " + greenTeamWins;
        survivedLabel.text = "Learning AIs survived: " + survivalCount;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SortNetworks(float currentTime)
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

            //networks[i] = networks[i + populationSize / 2].Clone(new NeuralNetwork(layers));

            // Perform crossover from two random parents in the fit half of the population
            networks[i] = networks[randomParentIndex1].Crossover(networks[randomParentIndex2]);

            // mutate
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }
}
