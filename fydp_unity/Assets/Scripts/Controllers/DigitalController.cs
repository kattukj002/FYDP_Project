using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FYDP {
    namespace Controllers {
        public abstract class DigitalController
        {
            public abstract float controlEffort(float input=0);
        }
    }
}


