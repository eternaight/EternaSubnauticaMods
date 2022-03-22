using RandomWorlds.NoiseAdventures;
using System.Collections;
using UnityEngine;

namespace RandomWorlds {
    public class HeightmapVoxelFilter : IVoxelFilter, IHeightmapProvider, IVoxelGrid {

        public bool ready;

        private INoiseFilter2D[] heightmapFilters;
        private HeightmapSettings settings;
        private Noise noise;

        private int hmWidth, hmHeight;
        private float[] heightmapCache;
        private float[] steepnessCache;

        //float[] curvatureCache;

        public HeightmapVoxelFilter(HeightmapSettings _settings, params INoiseFilter2D[] filters) {

            hmWidth = WorldManager.worldSizeInVoxels.x;
            hmHeight = WorldManager.worldSizeInVoxels.z;
            settings = _settings;
            noise = new Noise(_settings.seed);
            heightmapFilters = filters;
            ready = false;

            GenerateHeightmapCache();
        }

        private void GenerateHeightmapCache() {
            heightmapCache = new float[hmWidth * hmHeight];
            steepnessCache = new float[hmWidth * hmHeight];
            for (int z = 0; z < hmHeight; z++) {
                for (int x = 0; x < hmWidth; x++) {
                    var height = EvaluateHeight(new Vector2(x, z));
                    var i = x + z * hmWidth;
                    heightmapCache[i] = height;
                    if (i >= hmWidth) {
                        var dx = heightmapCache[i - 1] - height;
                        var dz = heightmapCache[i - hmWidth] - height;
                        steepnessCache[i] = (Mathf.Abs(dx) + Mathf.Abs(dz)) * 0.5f;
                    }
                }
            }
            ready = true;
        }
        private float EvaluateHeight(Vector2 pos2D) {

            var i1 = NoiseUtils.DomainWarp(pos2D, AngleNoise, DistanceNoise, 500);
            var mask = EvaluateCraterMask((int)i1.x, (int)i1.y, i1);
            mask = NoiseUtils.SmoothStop(mask);

            float elevation = 1000 * mask;
            for (int i = 0; i < heightmapFilters.Length; i++) {
                elevation += heightmapFilters[i].Evaluate(pos2D) * mask;
            }
            return elevation;
        }
        private float EvaluateCraterMask(int x, int y, Vector2 p) {

            var worldSize = WorldManager.worldSizeInVoxels;
            float edgeStart = settings.craterFalloffStart / worldSize.x * 0.5f, edgeEnd = edgeStart + (settings.craterFalloff / worldSize.x * 0.5f);

            const float edgeNoiseSize = 0.75f;
            const float edgeNoiseFrequency = 3;

            float edgeDistX = Mathf.Min(x, worldSize.x - x) / (worldSize.x * 0.5f) * (CraterDistanceNoise(new Vector2(x * edgeNoiseFrequency, y * edgeNoiseFrequency)) * edgeNoiseSize + 1 - edgeNoiseSize);
            float edgeDistY = Mathf.Min(y, worldSize.y - y) / (worldSize.y * 0.5f) * (CraterDistanceNoise(new Vector2(x * edgeNoiseFrequency + 10000, y * edgeNoiseFrequency + 10000)) * edgeNoiseSize + 1 - edgeNoiseSize);
            float edgedist = (Mathf.Clamp(NoiseUtils.smin(edgeDistX, edgeDistY, .5f), edgeStart, edgeEnd) - edgeStart) / (edgeEnd - edgeStart);

            float craterDist = Vector2.Distance(p, new Vector2(WorldManager.worldCenterVoxel.x, WorldManager.worldCenterVoxel.z));
            craterDist = Mathf.Clamp01((settings.craterRadius + settings.craterFalloff - craterDist) / settings.craterFalloff);

            return craterDist * edgedist;
        }
        private float CraterDistanceNoise(Vector2 p) {
            float lowFreq = (noise.Evaluate(p * settings.edgeNoiseFrequency) + 1) * 0.5f;
            float highFreq = (noise.Evaluate(p * settings.edgeNoiseFrequency * 7) + 1) * 0.1f;
            return (lowFreq + highFreq) / 1.2f;
        }
        private float AngleNoise(Vector2 p) {
            float lowFreq = (noise.Evaluate(p * settings.domainAngleFrequency) + 1) * 0.5f;
            float highFreq = (noise.Evaluate(p * settings.domainAngleFrequency * 7) + 1) * 0.05f;
            return (lowFreq + highFreq) / 1.1f;
        }
        private float DistanceNoise(Vector2 p) {
            float lowFreq = (noise.Evaluate(p * settings.domainDistanceFrequency) + 1) * 0.5f;
            float highFreq = (noise.Evaluate(p * settings.domainDistanceFrequency * 7) + 1) * 0.05f;
            return (lowFreq + highFreq) / 1.1f;
        }

        public void Apply(Voxel source) {

            var delta = GetHeight(source.position) - source.position.y;

            source.solidType = (byte)(GetSteepness(source.position) > 0.3f ? 2 : 1);
            source.signedDistance = delta;

            Random.InitState((int)(source.position.x + source.position.y + source.position.z));
            if (source.signedDistance > 0 && source.signedDistance < 1 && Random.value > 0.1f) {
                EntitySpawnManager.AddEntity(new EntityData("c72724f3-125d-4e87-b82f-a91b5892c936"), source.position);
            }
        }

        public float GetHeight(Vector3 worldPos) {
            return heightmapCache[((int)worldPos.x + (int)worldPos.z * hmWidth)];
        }
        public float GetSteepness(Vector3 worldPos) {
            return steepnessCache[((int)worldPos.x + (int)worldPos.z * hmWidth)]; 
        }

        public VoxelandData.OctNode GetVoxel(int x, int y, int z) {
            var heightDelta = heightmapCache[x + z * hmWidth] - y;
            return new VoxelandData.OctNode(System.Convert.ToByte(heightDelta >= 0 ? 1 : 0), VoxelandData.OctNode.EncodeDensity(heightDelta));
        }
        public bool GetVoxelMask(int x, int y, int z) {
            var delta = heightmapCache[x + z * hmWidth] - y;
            return delta < 32 && delta > -32;
        }
    }
}
