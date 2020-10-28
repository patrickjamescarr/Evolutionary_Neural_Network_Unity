using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

public class MultiLayerPerceptron : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] public float Gain = 0.7f;

    public List<Perceptron> inputPerceptrons;
    public List<Perceptron> hiddenPerceptrons;
    public List<Perceptron> outputPerceptrons;

    // Start is called before the first frame update
    void Start()
    {
        inputPerceptrons = new List<Perceptron>();
        outputPerceptrons = new List<Perceptron>();
        hiddenPerceptrons = new List<Perceptron>();
    }


    // Update is called once per frame
    void Update()
    {

    }

    // Learn the given output for the given input.
    public void LearnPattern(List<float> input, List<float> output)
    {
        // Generate the unlearned output.
        GenerateOutput(input);
        // perform the backpropagation
        Backprop(output);
    }    

    // Generate output for the given set of inputs
    private void GenerateOutput(List<float> input)
    {
        // Go through each input perceptron and set its state
        for (var index = 0; index <  inputPerceptrons.Count; index++)
        {
            inputPerceptrons[index].state = input[index];
        }

        // Go through each hidden perceptron and feed forward
        foreach (var hp in hiddenPerceptrons)
        {
           hp.FeedForward();
        }

        // Do the same for outputs
        foreach (var op in outputPerceptrons)
        {
            op.FeedForward();
        }
    }
    // Run the backpropagation learning algorithm. We
    // assume that the inputs have already been presented
    // and the feedforward step is complete.
    private void Backprop(List<float> output)
    {

        // Go through each output perceptron
        for (var index = 0; index < outputPerceptrons.Count; index++)
        {
            // find its generated state
            var perceptron = outputPerceptrons[index];
            var state = perceptron.state;

            // calc its error term
            var error = state * (1 - state) * (output[index] - state);

            // get the perceptron to adjust its weights
            perceptron.AdjustWeights(error);
        }

        // Go through each hidden perceptron
        for (var index = 0; index < hiddenPerceptrons.Count; index++)
        {
            // find its generated state
            var perceptron = outputPerceptrons[index];
            var state = perceptron.state;

            // calc its error term
            float sum = 0.0f;
            foreach (var op in outputPerceptrons)
            {
                var weight = op.GetIncommingWeight(perceptron);
                sum += weight * op.error;
            }

            var error = state * (1 - state) * (sum);

            // get the perceptron to adjust its weights
            perceptron.AdjustWeights(error);
        }
    }
}
