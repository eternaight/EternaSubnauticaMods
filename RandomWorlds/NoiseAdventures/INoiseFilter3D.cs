using UnityEngine;

namespace RandomWorlds.NoiseAdventures {
    public interface INoiseFilter3D {
        float Evaluate(Vector3 p);
    }
}
