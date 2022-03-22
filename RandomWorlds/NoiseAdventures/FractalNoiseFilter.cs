using UnityEngine;

namespace RandomWorlds.NoiseAdventures {
    class FractalNoiseFilter : INoiseFilter2D {
        public float baseRoughness;
        public int numOctaves;
        public float roughness;
        public float persistence;
        public float amplitude;
        private Noise noise;

        public FractalNoiseFilter(float _baseRoughness, int _numOctaves, float _roughness, float _persistence, float _amplitude, int seed) {
            noise = new Noise(seed);
            baseRoughness = _baseRoughness;
            numOctaves = _numOctaves;
            roughness = _roughness;
            persistence = _persistence;
            amplitude = _amplitude;
        }

        public float Evaluate(Vector2 point) {
            float noiseValue = 0;
            float maxNoiseValue = 0;
            float frequency = baseRoughness;
            float amplitude = 1;

            for (int i = 0; i < numOctaves; i++) {
                var sample = point * frequency;
                float v = noise.Evaluate(sample);
                noiseValue += (v + 1) * 0.5f * amplitude;
                maxNoiseValue += amplitude;
                frequency *= roughness;
                amplitude *= persistence;
            }

            return noiseValue * this.amplitude;
        }
    }
}
