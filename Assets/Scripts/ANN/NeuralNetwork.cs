using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

enum ActivationType
{
    Sigmoid = 0,
    ReLU = 2
}

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    // containers for the elements in the network
    private readonly int[] layers; // contains the layers in the network. Inputlayer, hidden layers and output
    private float[][] neurons; // contains the neurons within each layer
    private float[][] biases; // the biases for each neuron
    private float[][][] weights; // the weights associated with each dendrite

    private readonly ActivationType activationType = ActivationType.Sigmoid;

    public float fitness = 0;

    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        InitialiseNeurons();
        InititialseBiases();
        InitialiseWeights();
    }

    // initialise the neurons array to the appropriate size
    private void InitialiseNeurons()
    {
        var neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }

        neurons = neuronsList.ToArray();
    }

    // initialise the biases
    private void InititialseBiases()
    {
        var biasList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++)
        {
            var bias = new float[layers[i]];

            for (int j = 0; j < layers[i]; j++)
            {
                // use a small randon value to initialise the bias
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }

            biasList.Add(bias);
        }

        biases = biasList.ToArray();
    }

    // initialise the weights
    private void InitialiseWeights()
    {
        var weightsList = new List<float[][]>();

        // start at 1 to access the first hidden layer
        // we look back to grab the amount of neurons from previous layer
        for (int i = 1; i < layers.Length; i++)
        {
            var layerWeightsList = new List<float[]>();

            var neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {
                var neuronWeights = new float[neuronsInPreviousLayer];

                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    // use a small random value to initialise the weight
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }

                layerWeightsList.Add(neuronWeights);
            }

            weightsList.Add(layerWeightsList.ToArray());
        }

        weights = weightsList.ToArray();
    }

    // feed forward algorithm
    public float[] FeedForward(float[] inputs)
    {
        // populate the input layer
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                // calculate the weighted sum
                float weightedSum = 0f;

                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    weightedSum += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                // apply the activation function
                neurons[i][j] = Activate(weightedSum + biases[i][j]);
            }
        }

        // return the output layer
        return neurons[neurons.Length - 1];
    }

    private float Activate(float value)
    {
        switch (activationType)
        {
            case ActivationType.Sigmoid:
                return Sigmoid(value);
            case ActivationType.ReLU:
                return ReLU(value);
            default:
                return Sigmoid(value);
        }
    }

    // activation functions
    private static float ReLU(float value)
    {
        return Mathf.Max(0, value);
    }

    private static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp(-value));
    }

    /// <summary>
    /// Gets a random float in a given range to apply as a mutation
    /// </summary>
    private float GetRandomMutationValue(float mutationChance, float mutationStrength)
    {
        // get a random float value between 0 and 1
        var randomVal = UnityEngine.Random.value;

        // if the random value is less than the given chance of mutation then 
        // return a random float value in the range of the given mutation strength,
        // otherwise return 0
        return randomVal < mutationChance ? UnityEngine.Random.Range(-mutationStrength, mutationStrength) : 0f;
    }

    /// <summary>
    /// Performs mutation on this network
    /// </summary>
    public void Mutate(float mutationChance, float mutationStrength)
    {
        // loop through the biases
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                // apply a random mutation value
                biases[i][j] += GetRandomMutationValue(mutationChance, mutationStrength);
            }
        }

        // loop through the weights
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    // apply a random mutation value
                    weights[i][j][k] += GetRandomMutationValue(mutationChance, mutationStrength);
                }
            }
        }
    }

    /// <summary>
    /// Crossover this network with another network to produce a new child network
    /// </summary>
    public NeuralNetwork Crossover(NeuralNetwork otherNetwork)
    {
        // create a new child network
        var child = new NeuralNetwork(layers);

        // loop through the biases and randomly assign the child network 
        // bias values from either this network or the other one
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                child.biases[i][j] = UnityEngine.Random.value < 0.5 ? biases[i][j] : otherNetwork.biases[i][j];
            }
        }

        // loop through the weights and randomly assign the child network 
        // weight values from either this network or the other one
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    child.weights[i][j][k] = UnityEngine.Random.value < 0.5 ? weights[i][j][k] : otherNetwork.weights[i][j][k];
                }
            }
        }

        return child;
    }

    /// <summary>
    /// allows sorting of networks from within a list.
    /// sort by fitness
    /// </summary>
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }

    /// <summary>
    /// Save this network's weights and bias values to file
    /// </summary>
    public void Save(string path)
    {
        File.Create(path).Close();
        var streamWriter = new StreamWriter(path, true);

        // loop through all the bias values and write them to the file
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                streamWriter.WriteLine(biases[i][j]);
            }
        }

        // loop through all the weight values and write them to the file
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    streamWriter.WriteLine(weights[i][j][k]);
                }
            }
        }

        streamWriter.Close();
    }

    /// <summary>
    /// Load a previously saved set of weights and biases into this network
    /// </summary>
    public void Load(string path)
    {
        var fileInfo = new FileInfo(path);
        var streamReader = new StreamReader(path);
        var lines = new string[fileInfo.Length];
        int lineIndex = 1;

        // read in each line from the saved file
        for (int i = 1; i < fileInfo.Length; i++)
        {
            lines[i] = streamReader.ReadLine();
        }

        streamReader.Close();

        if (fileInfo.Length > 0)
        {
            // loop through the baises and assign the values from the file
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(lines[lineIndex]);
                    lineIndex++;
                }
            }

            // loop through the weights and assign the values from the file
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(lines[lineIndex]); ;
                        lineIndex++;
                    }
                }
            }
        }
    }
}
