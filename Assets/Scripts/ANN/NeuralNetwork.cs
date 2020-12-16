using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ActivationType
{
    Sigmoid = 0,
    Tanh = 1,
    ReLU = 2
}

public class NeuralNetwork : MonoBehaviour
{
    // containers for the elements in the network
    private int[] layers; // contains the layers in the network. Inputlayer, hidden layers and output
    private float[][] neurons; // contains the neurons within each layer
    private float[][] biases; // the biases for each neuron
    private float[][][] weights; // the weights associated with each dendrite
    private int[] activations; // TODO comment this

    private ActivationType activationType = ActivationType.ReLU;

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
                    // use a small randon value to initialise the weight
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
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Activate(value + biases[i][j]);
            }
        }
        return neurons[neurons.Length - 1];
    }

    private float Activate(float value)
    {
        switch (activationType)
        {
            case ActivationType.Sigmoid:
                return Sigmoid(value);
            case ActivationType.Tanh:
                return HyperbolicTangent(value);
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

    private static float HyperbolicTangent(float value)
    {
        return (float)Math.Tanh(value);
    }

    private static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp(-value));
    }

    public void Mutate(int chance, float val)//used as a simple mutation function for any genetic implementations.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, chance) <= 5) ? biases[i][j] += UnityEngine.Random.Range(-val, val) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, chance) <= 5) ? weights[i][j][k] += UnityEngine.Random.Range(-val, val) : weights[i][j][k];
                }
            }
        }
    }

    // Create a deep copy of the network
    public NeuralNetwork copy(NeuralNetwork network) 
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                network.biases[i][j] = biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    network.weights[i][j][k] = weights[i][j][k];
                }
            }
        }

        return network;
    }


}
