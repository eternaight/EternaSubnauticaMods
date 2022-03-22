using RandomWorlds.NoiseAdventures;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RandomWorlds {

    class BiomemapGenerator : IVoxelFilter, IBiomemapProvider {

        public Vector2[] biomePoints;

        Noise noise;
        private const float angleLowFreq = 0.000135f;
        private const float angleHighFreq = angleLowFreq * 12.1f;
        private const float distanceLowFreq = 0.00215f;
        private const float distanceHighFreq = distanceLowFreq * 11f;

        BiomeSettings[] biomes;
        private float craterRadius;

        private const int voidBiome = 11;

        private int mapWidth, mapHeight;
        private int[] biomemap;

        public BiomemapGenerator(BiomeGenerationSettings _settings) {

            biomes = _settings.biomes;
            craterRadius = _settings.craterRadius;

            biomePoints = new Vector2[_settings.biomePointCount];

            noise = new Noise(_settings.seed);

            float cellSize = _settings.poissonSpacing / Mathf.Sqrt(2);
            var worldSize = WorldManager.worldSizeInVoxels;
            var pointGrid = new int[Mathf.CeilToInt(worldSize.x / cellSize), Mathf.CeilToInt(worldSize.z / cellSize)];

            for (int i = 0; i < biomePoints.Length; i++) {

                bool occupied = true;
                int loops = 0;
                while (occupied) {
                    var randomPos = new Vector2(Random.value * 0.7f + 0.15f, Random.value * 0.7f + 0.15f);
                    biomePoints[i] = new Vector2(randomPos.x * worldSize.x, randomPos.y * worldSize.z);

                    Vector2Int cell = new Vector2Int(Mathf.FloorToInt(biomePoints[i].x / cellSize), Mathf.FloorToInt(biomePoints[i].y / cellSize));
                    var dist = Vector2.Distance(biomePoints[i], new Vector2(WorldManager.worldCenterVoxel.x, WorldManager.worldCenterVoxel.z));

                    if ((pointGrid[cell.x, cell.y] == 0 && dist < _settings.craterRadius) || loops >= 32) {
                        occupied = false;
                    }

                    loops++;
                }
            }

            GenerateBiomemapCache();
        }

        private void GenerateBiomemapCache() {
            var worldSize = WorldManager.worldSizeInVoxels;
            mapWidth = worldSize.x >> 2;
            mapHeight = worldSize.z >> 2;
            biomemap = new int[mapWidth * mapHeight];
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    int biome = EvaluateBiome(x * 4, y * 4);
                    biomemap[x + y * mapWidth] = biome;
                }
            }
        }
        private int EvaluateBiome(int x, int y) {

            return voidBiome;

            int closestPointIndex = -1;
            float minDist = float.PositiveInfinity;
            Vector2 point = new Vector3(x, y);
            Vector2 warpedPoint = NoiseUtils.DomainWarp(point, AngleNoise, DistanceNoise, 375);

            for (int i = 0; i < biomePoints.Length; i++) {

                float dist = ManhattanDistance(biomePoints[i], warpedPoint);

                if (dist < minDist) {
                    closestPointIndex = i;
                    minDist = dist;
                }
            }

            if (Vector2.Distance(warpedPoint, new Vector2(WorldManager.worldCenterVoxel.x, WorldManager.worldCenterVoxel.z)) > craterRadius) return voidBiome;

            return PointToBiome(closestPointIndex);
        }
        private float ManhattanDistance(Vector2 a, Vector2 b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        private float AngleNoise(Vector2 p) {
            float lowFreq = (noise.Evaluate(p * angleLowFreq) + 1) * 0.5f;
            float highFreq = (noise.Evaluate(p * angleHighFreq) + 1) * 0.1f;
            return (lowFreq + highFreq) / 1.2f;
        }
        private float DistanceNoise(Vector2 p) {
            float lowFreq = (noise.Evaluate(p * distanceLowFreq) + 1) * 0.5f;
            float highFreq = (noise.Evaluate(p * distanceHighFreq) + 1) * 0.05f;
            return (lowFreq + highFreq) / 1.1f;
        }

        public void Apply(Voxel source) {
            var biome = GetBiome(source.position);
            source.solidType = (source.solidType == 1) ? biome.surfaceType : biome.bedrockType;
        }

        public Color32[] GetBiomemap(Dictionary<Int3, BiomeProperties> legend, out int _mapWidth, out int _mapHeight) {
            var legendKeys = legend.Keys.ToArray();

            var output = new Color32[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {

                    Int3 key;
                    int biome = biomemap[x + y * mapWidth];
                    if (biome < legendKeys.Length) {
                        key = legendKeys[biome];
                    } else {
                        key = legendKeys[0];
                    }
                    output[x + y * mapWidth] = new Color32((byte)key.x, (byte)key.y, (byte)key.z, 255);
                }
            }

            // Writing to file
            Texture2D tex = new Texture2D(mapWidth, mapHeight);
            tex.SetPixels32(output);
            var path = Path.Combine(WorldManager.GetWorldPath(), "biomeMap.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());

            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            return output;
        }

        public BiomeSettings GetBiome(Vector3 worldPos) {
            return biomes[GetBiomeCached(worldPos)];
        }
        private int GetBiomeCached(Vector3 worldPos) {
            try {
                var x_ds = (int)worldPos.x >> 2;
                var y_ds = (int)worldPos.z >> 2;
                return biomemap[x_ds + y_ds * mapWidth];
            } catch (System.IndexOutOfRangeException ex) {
                RandomWorldsJournalist.Log(2, ex.Message);
                return voidBiome;
            }
        }

        // Maps point index to the biome index.
        // All unused biomes like Arctic, Unassigned, ILZ, as well as the void biome, get mapped to additional instances of some surface biomes
        // so there are two safe shallows, three kelp forests, two mushroom forests, etc.
        private int PointToBiome(int i) {
            if (i > 21) return 4;
            switch (i) {
                case 0:
                case 7:
                    return 0;
                case 1:
                case 8:
                case 9:
                    return 1;
                case 2:
                case 11:
                case 19:
                case 20:
                    return 2;
                case 4:
                case 21:
                    return 4;
                default:
                    return i;
            }
        }
    }
}
