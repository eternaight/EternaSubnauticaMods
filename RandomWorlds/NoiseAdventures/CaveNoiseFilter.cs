using UnityEngine;

namespace RandomWorlds.NoiseAdventures {
    class CaveNoiseFilter : INoiseFilter3D {
        private readonly Noise noise;
        
        private float _cheeseFrequency;
        private float _cheeseFloor;

        private float _spaghettiFrequency;
        private float _spaghettiCoreLevel;
        private float _spaghettiThickness;

        public CaveNoiseFilter(float cheeseFrequency, float cheeseFloor, float spaghettiFrequency, float spaghettiCoreLevel, float spaghettiThickness) {
            noise = new Noise();

            _cheeseFrequency = cheeseFrequency;
            _cheeseFloor = cheeseFloor;
            _spaghettiFrequency = spaghettiFrequency;
            _spaghettiCoreLevel = spaghettiCoreLevel;
            _spaghettiThickness = spaghettiFrequency; 
        }

        // Returns distance to cave walls
        public float Evaluate(Vector3 p) {
            return CheeseNoise(p);
        }

        // Returns distance from cheese noise
        // If noise if lower than the floor, distance is positive and the voxel is solid, else - voxel is air
        private float CheeseNoise(Vector3 p) {
            // TODO: multiply by some amount to control density weirdness
            return (_cheeseFloor - noise.Evaluate(p * _cheeseFrequency)) / _cheeseFrequency;
        }

        // Returns distance from spaghetti noise
        // Same thing as cheese noise, except we do an intersection of 2 values
        private float SpaghettiNoise(Vector3 p) {

            var floor = _spaghettiCoreLevel - _spaghettiThickness / 2;
            var ceil = _spaghettiCoreLevel + _spaghettiThickness / 2;

            var a = floor - noise.Evaluate(p * _spaghettiFrequency);
            var b = noise.Evaluate(p * _spaghettiFrequency) - ceil;

            return NoiseUtils.BooleanIntersect(a, b);
        }
    }
}
