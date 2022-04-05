using RandomWorlds.NoiseAdventures;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RandomWorlds {

    public class SurfaceBiomeProvider {

        private static SurfaceBiomeProvider main;

        public static SurfaceBiomeProvider GetInstance() {
            return main;
        }

        private const float angleLowFreq = 0.000135f;
        private const float angleHighFreq = angleLowFreq * 12.1f;
        private const float distanceLowFreq = 0.00215f;
        private const float distanceHighFreq = distanceLowFreq * 11f;

        private readonly List<BiomeEntry> _biomes;
        private readonly List<int> _pointToBiomeMap;
        private readonly float _craterRadius;
        private readonly int _seed;
        private readonly float _poissonSpacing;
        private Noise _noise;

        private bool baked;
        private int mapWidth, mapHeight;
        private int[] biomemap;
        private Vector2[] biomePoints;
        
        public SurfaceBiomeProvider(int seed, float craterRadius, float poissonSpacing) {
            main = this;

            _biomes = new List<BiomeEntry>();
            _pointToBiomeMap = new List<int>();
            _craterRadius = craterRadius;
            _seed = seed;
            _poissonSpacing = poissonSpacing;

            baked = false;
        }

        public void SubscribeBiomeSettings(IBiomeApplicator biomeApplicator) {

            if (baked) return;

            var newSettings = biomeApplicator.Retrieve();

            int newBiomeIndex = _biomes.Count;
            _biomes.AddRange(newSettings);
            foreach (var item in newSettings) {
                for (int i = 0; i < item.count; i++) {
                    _pointToBiomeMap.Add(newBiomeIndex);
                }
            }
            _pointToBiomeMap.Shuffle();
        }

        public void Bake() {
            _noise = new Noise(_seed);
            PlaceBiomePoints();
            BakeBiomemap();
            baked = true;
        }

        private void PlaceBiomePoints() {

            biomePoints = new Vector2[_pointToBiomeMap.Count];

            float cellSize = _poissonSpacing / Mathf.Sqrt(2);
            var worldSize = WorldConfiguration.WORLD_SIZE_VOXELS;
            var pointGrid = new int[Mathf.CeilToInt(worldSize.x / cellSize), Mathf.CeilToInt(worldSize.z / cellSize)];

            for (int i = 0; i < biomePoints.Length; i++) {

                bool occupied = true;
                int loops = 0;
                while (occupied) {
                    var randomPos = new Vector2(Random.value * 0.7f + 0.15f, Random.value * 0.7f + 0.15f);
                    biomePoints[i] = new Vector2(randomPos.x * worldSize.x, randomPos.y * worldSize.z);

                    Vector2Int cell = new Vector2Int(Mathf.FloorToInt(biomePoints[i].x / cellSize), Mathf.FloorToInt(biomePoints[i].y / cellSize));
                    var dist = Vector2.Distance(biomePoints[i], new Vector2(WorldConfiguration.ORIGIN_VOXEL.x, WorldConfiguration.ORIGIN_VOXEL.z));

                    if ((pointGrid[cell.x, cell.y] == 0 && dist < _craterRadius) || loops >= 32) {
                        occupied = false;
                    }

                    loops++;
                }
            }
        }

        private void BakeBiomemap() {
            var worldSize = WorldConfiguration.WORLD_SIZE_VOXELS;
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

            var closestPointIndex = -1;
            var minDist = float.PositiveInfinity;
            var point = new Vector2(x, y);
            var warpedPoint = NoiseUtils.DomainWarp(point, AngleNoise, DistanceNoise, 375);
            var hmOrigin = new Vector2(WorldConfiguration.ORIGIN_VOXEL.x, WorldConfiguration.ORIGIN_VOXEL.z);

            for (int i = 0; i < biomePoints.Length; i++) {

                float dist = ManhattanDistance(biomePoints[i], warpedPoint);

                if (dist < minDist) {
                    closestPointIndex = i;
                    minDist = dist;
                }
            }

            if (Vector2.Distance(warpedPoint, hmOrigin) > _craterRadius) return GetVoidBiomeIndex();

            return _pointToBiomeMap[closestPointIndex];
        }
        private float ManhattanDistance(Vector2 a, Vector2 b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        private float AngleNoise(Vector2 p) {
            float lowFreq = (_noise.Evaluate(p * angleLowFreq) + 1) * 0.5f;
            float highFreq = (_noise.Evaluate(p * angleHighFreq) + 1) * 0.1f;
            return (lowFreq + highFreq) / 1.2f;
        }
        private float DistanceNoise(Vector2 p) {
            float lowFreq = (_noise.Evaluate(p * distanceLowFreq) + 1) * 0.5f;
            float highFreq = (_noise.Evaluate(p * distanceHighFreq) + 1) * 0.05f;
            return (lowFreq + highFreq) / 1.1f;
        }

        public int GetBiomeCached(Vector3 worldPos) {
            try {
                var x_ds = (int)worldPos.x >> 2;
                var y_ds = (int)worldPos.z >> 2;
                return biomemap[x_ds + y_ds * mapWidth];
            }
            catch (System.IndexOutOfRangeException ex) {
                RandomWorldsJournalist.Log(2, ex.Message);
                return GetVoidBiomeIndex();
            }
        }

        public void ModifyBiomemapLegend(Dictionary<Int3, BiomeProperties> legend) {
            legend.Clear();
            foreach (var item in _biomes) {
                legend.Add(item.legendColor, item.properties);
            }
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
            var path = Path.Combine(RandomWorlds.GetWorldPath(), "biomeMap.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());

            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            return output;
        }

        private int GetVoidBiomeIndex() {
            return 0;
        }
    }
}
