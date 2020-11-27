using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Perceptron
{
    public List<PerceptronInput> inputs;
    public float state;
    public float error;

    private float gain = 0.7f;

    // Perform the feedforward algorithm.
    public void FeedForward()
    {
        // Go through each input and sum its contribution
        float sum = 0.0f;

        foreach(var input in inputs)
        {
            sum += input.inputPerceptorn.state * input.weight;
        }

        // Apply the thresholding function
        this.state = Threshold(sum);
    }

    // Perform the update in the backpropogation algorithm
    public void AdjustWeights(float currentError)
    {
        // go through each input

        foreach(var input in inputs)
        {
            // Find the change in weight required
            state = input.inputPerceptorn.state;
            float deltaWeight = gain * currentError * state;

            // Apply it
            input.weight += deltaWeight;
        }

        // Store the error, perceptrons in preceeding layers will need it.
        error = currentError;
    }

    // Find the weight of the input that arrived from the 
    // given perceptron. This is used in hidden layers to calculate 
    // the outgoing error contribution
    public float GetIncommingWeight(Perceptron perceptron)
    {
        //foreach (var input in inputs)
        //{
        //    if(input.inputPerceptorn == perceptron)
        //    {
        //        return input.weight;
        //    }    
        //}

        // find the first matching perceptron in the inputs.
        var input = inputs.FirstOrDefault(x => x.inputPerceptorn == perceptron);

        // if we find a match return the weight, otherwise we have no weight.
        return input ? input.weight : 0;
    }

    private float Threshold(float input)
    {
        return Mathf.Max(0, input);
    }
}
