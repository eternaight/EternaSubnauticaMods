using UnityEngine;

namespace RandomWorlds.NoiseAdventures {
    class RidgidNoiseFilter : INoiseFilter2D {
        public float baseRoughness;
        public float numOctaves;
        public float roughness;
        public float persistence;
        public float amplitude;
        private Noise noise;

        float angleFrequency = 0.00085f;
        float distanceFrequency = 0.005f;

        public RidgidNoiseFilter(float _baseRoughness, int _numOctaves, float _roughness, float _persistence, float _amplitude, int seed) {
            noise = new Noise(seed);
            baseRoughness = _baseRoughness;
            numOctaves = _numOctaves;
            roughness = _roughness;
            persistence = _persistence;
            amplitude = _amplitude;
        }

        public float Evaluate(Vector2 point) {
            var i1 = NoiseUtils.DomainWarp(point, AngleNoise, DistanceNoise, 75);
            var i2 = NoiseUtils.DomainWarp(i1, AngleNoise, DistanceNoise, 25);
            return RidgidNoise(i2);
        }

        public float RidgidNoise(Vector2 point) {
            float noiseValue = 0;
            float maxNoiseValue = 0;
            float frequency = baseRoughness;
            float _amplitude = 1;

            for (int i = 0; i < numOctaves; i++) {
                var sample = point * frequency;
                float v = -Mathf.Abs(noise.Evaluate(sample)) + 1;
                v *= v;
                v *= v;
                noiseValue += v * _amplitude;

                maxNoiseValue += _amplitude;

                frequency *= roughness;
                _amplitude *= persistence;
            }
            if (maxNoiseValue > 0)
                noiseValue /= maxNoiseValue;
            return noiseValue * amplitude;
        }

        public float AngleNoise(Vector2 p) {
            return noise.Evaluate(p * angleFrequency);
        }
        public float DistanceNoise(Vector2 p) {
            return noise.Evaluate(p * distanceFrequency);
        }
    }
}
