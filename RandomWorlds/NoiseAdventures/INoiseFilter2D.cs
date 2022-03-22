using UnityEngine;

namespace RandomWorlds.NoiseAdventures {
    public interface INoiseFilter2D {
        float Evaluate(Vector2 p);
    }
}
