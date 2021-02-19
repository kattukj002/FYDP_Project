using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FYDP.Controllers;

public class PIDController_test : MonoBehaviour
{
    public int numSamples = 1000;
    
    void Start() {
        
        float[] step = GenerateDiscreteStep(start_time: 0, 
                                            numSamples: numSamples,
                                            magnitude: 1);
        float[] ramp = GenerateDiscreteRamp(start_time: 0, 
                                            numSamples: numSamples,
                                            slope: 1);
        
        float pGain = 5;
        float iGain = 0.01f;
        float dGain = 2;
        float samplingPeriod = 0.02f;
        float derivativeRollOffPole = -40f;

        DigitalController stepController = new PidController(
            pGain: pGain, iGain: iGain, dGain: dGain, 
            samplingPeriod: samplingPeriod, 
            derivativeRollOffPole: derivativeRollOffPole);

        DigitalController rampController = new PidController(
            pGain: pGain, iGain: iGain, dGain: dGain, 
            samplingPeriod: samplingPeriod, 
            derivativeRollOffPole: derivativeRollOffPole);
        
        float[] stepOut = new float[numSamples];
        float[] rampOut = new float[numSamples];
        
        for (int i = 0; i < numSamples; i++) {
            Debug.Log(step[0]);
            stepOut[i] = stepController.controlEffort(step[i]);
            rampOut[i] = stepController.controlEffort(ramp[i]);
        }

        StreamWriter outputFile = new StreamWriter("./pidControllerOut.csv");
        outputFile.WriteLine("Step,Ramp");
        
        for (int i = 0; i < numSamples; i++) {
            outputFile.WriteLine(stepOut[i].ToString() + "," + 
                                 rampOut[i].ToString());
        }
        outputFile.Flush();
        outputFile.Close();
    }

    private float[] GenerateDiscreteStep(int start_time, int numSamples, 
                                       float magnitude) {
        float[] signal = new float[numSamples];
        for (int i = start_time; i < numSamples; i++) {
            signal[i] = magnitude; 
        } 
        return signal;
    }
    private float[] GenerateDiscreteRamp(int start_time, int numSamples, 
                                       float slope) {

        float[] signal = new float[numSamples];
        for (int i = start_time; i < numSamples; i++) {
            signal[i] = slope * (i - start_time); 
        } 
        return signal;
    }
}