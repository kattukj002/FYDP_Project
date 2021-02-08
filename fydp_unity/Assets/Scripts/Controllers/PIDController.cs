using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FYDP {
    namespace Controllers {
        public class PidController : DigitalController
        {
            private float _pGain;
            private float _iGain;
            private float _dGain;
            private float _samplingPeriod;

            private float _derivativeRollOffPole;

            private int _controllerOrder = 2;
            private float[] _inputRecords; 
            private float[] _outputRecords;
            private int _currRecordIndex;

            private float[] _differenceCoefficients = new float[5];

            public PidController(float pGain, float iGain, float dGain, float samplingPeriod, 
                                float derivativeRollOffPole)
            {
                _pGain = pGain;
                _iGain = iGain;
                _dGain = dGain;
                _samplingPeriod = samplingPeriod;
                _derivativeRollOffPole = derivativeRollOffPole;
                
                _inputRecords = new float[_controllerOrder + 1];
                _outputRecords = new float[_controllerOrder + 1];
                _currRecordIndex = _controllerOrder;

                float discretizedPole = Mathf.Exp(derivativeRollOffPole*samplingPeriod);
                
                _differenceCoefficients[0] = _pGain + _dGain;
                _differenceCoefficients[1] = -1 + _iGain*_samplingPeriod+
                                            (_dGain - 1) * discretizedPole - _dGain;
                _differenceCoefficients[2] = (_pGain - _iGain)*discretizedPole + _dGain;
                _differenceCoefficients[3] = discretizedPole + 1;
                _differenceCoefficients[4] = -discretizedPole;

            }

            public override float controlEffort(float input=0)
            {
                _currRecordIndex = (_currRecordIndex + 1) % 3;

                _inputRecords[_currRecordIndex] = input;

                _inputRecords[_currRecordIndex] = _differenceCoefficients[0]*_inputRecords[_currRecordIndex] +
                                        _differenceCoefficients[1]*_inputRecords[(_currRecordIndex - 1) % 3] +
                                        _differenceCoefficients[2]*_inputRecords[(_currRecordIndex - 2) % 3] +
                                        _differenceCoefficients[3]*_inputRecords[(_currRecordIndex - 1) % 3] +
                                        _differenceCoefficients[4]*_inputRecords[(_currRecordIndex - 2) % 3];

                return _inputRecords[_currRecordIndex];
            }
        }
    }
}


