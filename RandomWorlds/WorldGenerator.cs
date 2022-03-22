using RandomWorlds.NoiseAdventures;
using System.Collections.Generic;
using UnityEngine;

namespace RandomWorlds {
    public class WorldGenerator {

        private IVoxelFilter[] filters;
        private IBiomemapProvider biomes;
        public HeightmapVoxelFilter heightmap;

        public WorldGenerator(WorldSettings _settings) {

            EntitySpawnManager.Initialize();

            List<IVoxelFilter> list = new List<IVoxelFilter>();

            var hmSettings = Settings.mainHeightmapSettings;
            hmSettings.seed = _settings.seed;
            HeightmapVoxelFilter heightmap = new HeightmapVoxelFilter(hmSettings,
                new RidgidNoiseFilter (0.0005f, 3, 1.9f, 0.5f, 150, _settings.seed),
                new FractalNoiseFilter(0.00085f, 8, 2.1f, 0.45f, 225, _settings.seed));
            list.Add(heightmap);
            this.heightmap = heightmap;

            var bmSettings = Settings.biomeGenSettings;
            bmSettings.seed = _settings.seed;
            BiomemapGenerator bioms = new BiomemapGenerator(bmSettings);
            list.Add(bioms);
            biomes = bioms;

            CaveVoxelFilter caves = new CaveVoxelFilter(_settings.seed, 1, heightmap);
            list.Add(caves);

            filters = list.ToArray();
        }

        public Color32[] GetBiomemap(Dictionary<Int3, BiomeProperties> legend, out int width, out int height) {
            return biomes.GetBiomemap(legend, out width, out height);
        }
        public float GetHeightCached(Vector3 pos) {
            return heightmap.GetHeight(pos);
        }

        public void FillRasterWorkspace(Voxeland.RasterWorkspace ws, Int3 voxelWorldOrigin, int downsampleLevels) {
            var enumerator = Int3.Range(ws.size);
            while (enumerator.MoveNext()) {
                Int3 pos = enumerator.Current;
                Vector3 voxelPos = (voxelWorldOrigin + pos * (1 << downsampleLevels)).ToVector3();

                Voxel voxel = new Voxel(voxelPos);
                foreach (IVoxelFilter _filter in filters) {
                    _filter.Apply(voxel);
                }

                ws.typesGrid.Set(pos, (byte)(voxel.signedDistance >= 0 ? voxel.solidType : 0));
                byte d = VoxelandData.OctNode.EncodeDensity(voxel.signedDistance);
                ws.densityGrid.Set(pos, d);
            }
        }

        public bool CellContainsEntities(Int3 cellId, Int3 batchId) {
            var mins = cellId * 16 + batchId * 160;
            var bounds = new Int3.Bounds(mins, mins + 15);
            var height = heightmap.GetHeight(bounds.center);
            var steepness = heightmap.GetSteepness(bounds.center);
            return (height >= bounds.mins.y && height < bounds.maxs.y + 1 && steepness < 0.26f);
        }
        public void FillCellEntities(EntityCell cell, CellEntities entities) {
            var pos = cell.GetBlockBounds().center;
            var biome = biomes.GetBiome(pos);
            var height = heightmap.GetHeight(pos);
            entities.AddEntity(biome.decoration, new Vector3(pos.x, height, pos.z));
        }
        public EntitySlotData[] FillEntitySlots(Vector3 cellCenter) {
            int outcropCount = 3;
            int creatureCount = 3;
            var datas = new EntitySlotData[outcropCount + creatureCount];

            for (int i = 0; i < outcropCount; i++) {

                var pos = Random.onUnitSphere * 5;
                pos.y = GetHeightCached(pos + cellCenter) - cellCenter.y;

                if (Mathf.Abs(pos.y) > 5) return new EntitySlotData[0];

                datas[i] = new EntitySlotData() {
                    allowedTypes = EntitySlotData.EntitySlotType.Small | EntitySlotData.EntitySlotType.Medium | EntitySlotData.EntitySlotType.Large,
                    biomeType = BiomeType.SafeShallows_Grass,
                    density = 1,
                    localPosition = pos,
                    localRotation = Quaternion.identity
                };
            }

            for (int i = 0; i < creatureCount; i++) {

                var pos = Random.onUnitSphere * 5;

                datas[outcropCount + i] = new EntitySlotData() {
                    allowedTypes = EntitySlotData.EntitySlotType.Small | EntitySlotData.EntitySlotType.Medium | EntitySlotData.EntitySlotType.Large,
                    biomeType = BiomeType.SafeShallows_OpenShallow_CreatureOnly,
                    density = 1,
                    localPosition = pos,
                    localRotation = Quaternion.identity
                };
            }

            return datas;
        }
    }

    public class Voxel {
        public byte solidType;
        public float signedDistance;
        public Vector3 position;

        public Voxel(Vector3 _position) {
            solidType = 1;
            signedDistance = -1;
            position = _position;
        }
    }

    public interface IVoxelFilter {
        void Apply(Voxel voxel);
    }
    public interface IBiomemapProvider {
        Color32[] GetBiomemap(Dictionary<Int3, BiomeProperties> legend, out int _mapWidth, out int _mapHeight);
        BiomeSettings GetBiome(Vector3 worldPos);
    }
    public interface IHeightmapProvider {
        float GetHeight(Vector3 pos);
        float GetSteepness(Vector3 pos);
    }
}