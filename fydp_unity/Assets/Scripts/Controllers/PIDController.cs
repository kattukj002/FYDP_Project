using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDController : DigitalController
{
    private float p_gain;
    private float i_gain;
    private float d_gain;
    private float sampling_period;

    private float derivative_roll_off_pole;

    private float[] input_records = {0,0,0};
    private float[] output_records = {0,0,0};
    private int curr_index = 2;

    private float[] difference_coefficients = {0,0,0,0,0};

    public PIDController(float in_p_gain, float in_i_gain, float in_d_gain, float in_sampling_period, 
                        float in_derivative_roll_off_pole)
    {
        p_gain = in_p_gain;
        i_gain = in_i_gain;
        d_gain = in_d_gain;
        sampling_period = in_sampling_period;
        derivative_roll_off_pole = in_derivative_roll_off_pole;

        for ( int i = 0; i < input_records.Length; i++ )
        {
            input_records[i] = 0;
            output_records[i] = 0;
        }
        
        curr_index = 2;

        float discretized_pole = Mathf.Exp(derivative_roll_off_pole*sampling_period);
        
        difference_coefficients[0] = p_gain + d_gain;
        difference_coefficients[1] = -1 + i_gain*sampling_period+
                                    (d_gain - 1) * discretized_pole - d_gain;
        difference_coefficients[2] = (p_gain - i_gain)*discretized_pole + d_gain;
        difference_coefficients[3] = discretized_pole + 1;
        difference_coefficients[4] = -discretized_pole;

    }

    public override float controlEffort(float input=0)
    {
        curr_index = (curr_index + 1) % 3;

        input_records[curr_index] = input;

        input_records[curr_index] = difference_coefficients[0]*input_records[curr_index] +
                                difference_coefficients[1]*input_records[(curr_index - 1) % 3] +
                                difference_coefficients[2]*input_records[(curr_index - 2) % 3] +
                                difference_coefficients[3]*input_records[(curr_index - 1) % 3] +
                                difference_coefficients[4]*input_records[(curr_index - 2) % 3];

        return input_records[curr_index];
    }
}
