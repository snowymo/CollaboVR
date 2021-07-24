﻿using UnityEngine;
using System.Collections;

namespace CT {
    /// <summary>
    /// A flat square in the XY plane distorted by some random noise in the Z direction.
    /// </summary>
    public class NoisySquare : Parametric {
        protected override Vector3 ParametricFunction(float u, float v) {
            return new Vector3(2 * u - 1,
                               2 * v - 1,
                               0.5f * Noise.GetNoise(new Vector3(3 * u, 3 * v, 0.5f)));
        }
    }
}