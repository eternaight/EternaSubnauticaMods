using RandomWorlds.OctreeGen;
using System;
using System.IO;
using UnityEngine;

namespace RandomWorlds {

    /*
     The purpose of this class is to create and manage WorldGenerator - supply it with correct settings and stuff
     */
    public static class WorldManager {

        public static WorldGenerator generator;
        private static WorldOctrees octrees;

        // Soft constants related to world setup. What does it take to change these?
        public static readonly Vector3 worldCenterVoxel = new Vector3(2048, 3040, 2048);
        public static readonly Int3 worldSizeInVoxels = new Int3(4096, 3200, 4096);

        public static readonly Int3.Bounds batchBounds = new Int3.Bounds(new Int3(10, 17, 10), new Int3(14, 19, 14));

        public static void Initialize() {
            RandomWorldsJournalist.Log(0, "Intitializing World Generator...");
            generator = new WorldGenerator(Settings.worldSettings);

            RandomWorldsJournalist.Log(0, "Writing octrees...");
            octrees = new WorldOctrees(generator.heightmap);

            RandomWorldsJournalist.Log(0, "Serializing octrees...");
            WorldSerializer.SerializeOctrees(octrees);
        }
        public static void InitializeRuntime() {
            RandomWorldsJournalist.Log(0, "Intitializing World Generator...");
            generator = new WorldGenerator(Settings.worldSettings);
        }

        public static string GetCompiledOctreesPath() {
            var path = Path.Combine(GetWorldPath(), "CompiledOctreesCache");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        public static string GetWorldPath() {
            var path = Path.Combine(RandomWorlds.ModDirectory, "World");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static bool OverrideBatchExists(Int3 batchId) {
            return false;
        }

        public static Vector3 GetStartPoint() {
            var angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
            var offset = UnityEngine.Random.insideUnitCircle * 10f;
            Vector3 hmPoint = new Vector3(Mathf.Cos(angle) * 1536 + offset.x, 0, Mathf.Sin(angle) * 1536 + offset.y);
            hmPoint.y = generator.GetHeightCached(hmPoint) + 5;

            return hmPoint;
        }
    }
}
