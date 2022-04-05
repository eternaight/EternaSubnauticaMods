using System;
using System.Collections.Generic;
using RandomWorlds.NoiseAdventures;
using UnityEngine;

namespace RandomWorlds.Layers {
    public class WorldLayerHeightmap : WorldLayer, ITerrainApplicator {

        private List<INoiseFilter2D> filterStack;
        public WorldLayerHeightmap() {
            filterStack = new List<INoiseFilter2D>();
        }

        public void AddFilter(INoiseFilter2D hmFilter) {
            filterStack.Add(hmFilter);
        }
        
        public virtual float GetBaseHeight() { return 0; }
        
        public void Apply(Voxel voxel) {
            var height = GetBaseHeight();
            var hmPos = new Vector2(voxel.position.x, voxel.position.z);
            filterStack.ForEach(filter => filter.Evaluate(hmPos));
            voxel.signedDistance = height - voxel.position.y;
        }

        public Int3.Bounds GetBatchBounds() {
            return WorldConfiguration.WORLD_BOUNDS;
        }
    }
}
