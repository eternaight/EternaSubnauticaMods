using System.Collections.Generic;
using UnityEngine;

namespace RandomWorlds {
    public static class EntitySpawnManager {

        private static CellEntities entitiesNow = new CellEntities();

        private static Dictionary<Int3, CellEntities> cellsGlobal;

        private static bool deserializingCells;
        private static string cellRootUid;
        private static int gameObjectIndex;

        private const int CELL_SIZE = 32;
        private const int CELL_LEVEL = 1;
        private const int CELLS_PER_BATCH = 5;

        public static void Initialize() {
            cellsGlobal = new Dictionary<Int3, CellEntities>();
        }

        public static void AddEntity(EntityData ent, Vector3 position) {
            var cellIndex = Int3.FloorDiv(new Int3((int)position.x, (int)position.y, (int)position.z), CELL_SIZE);
            if (!cellsGlobal.ContainsKey(cellIndex)) {
                cellsGlobal.Add(cellIndex, new CellEntities(cellIndex));
            }

            cellsGlobal[cellIndex].AddEntity(ent, position);
        }

        public static void OnCellRootAwoken(EntityCell cell) {

            if (!deserializingCells) {
                EnterCellLoading();
            }

            var bounds = cell.GetBlockBounds();
            var rootPosition = bounds.center - WorldManager.worldCenterVoxel;
            var globalcid = cell.cellId + cell.batchId * CELLS_PER_BATCH;
            if (cellsGlobal.ContainsKey(globalcid)) {
                entitiesNow = cellsGlobal[globalcid];
            } else {
                entitiesNow = null;
            }
            //AddSlotsAlt(bounds.center);
        }

        private static void AddSlotsAlt(Vector3 cellCenter) {
            int outcropCount = 3;
            int creatureCount = 3;
            bool oopsie = false;

            for (int i = 0; i < outcropCount; i++) {
                var pos = Random.onUnitSphere * 5;
                pos.y = WorldManager.generator.GetHeightCached(pos + cellCenter) - cellCenter.y + 1;

                if (Mathf.Abs(pos.y) < 5) {
                    entitiesNow.AddEntity(new EntityData("3aa21048-531d-40bc-8e92-846e124762f0"), pos + cellCenter);
                } else {
                    oopsie = true;
                }
            }

            if (oopsie) return;

            for (int i = 0; i < creatureCount; i++) {

                var pos = Random.onUnitSphere * 5;
                entitiesNow.AddEntity(new EntityData("1862748b-ebc8-4b00-8284-1b7763bee766"), pos + cellCenter);
            }
        }

        private static List<Int3> containTerrain = new List<Int3>();
        public static bool PrecomputeBatchEntities(Int3 batch) {
            containTerrain.Clear();
            
            foreach (Int3 index in Int3.Range(CELLS_PER_BATCH)) {
                var globalcid = index + batch * CELLS_PER_BATCH;
                if (cellsGlobal.ContainsKey(globalcid)) {
                    containTerrain.Add(index);
                }
            }

            return containTerrain.Count > 0;
        }
        public static void FillCellsFileHeader(CellManager.CellsFileHeader cfh, Int3 batch) {
            cfh.numCells = containTerrain.Count;
            cfh.version = 9;

            RandomWorldsJournalist.LogStream($"Creating cell file for batch: {batch} with {cfh.numCells} cells.\n");
        } 
        public static void FillCellHeader(CellManager.CellHeaderEx che, int cellIndex) {
            che.cellId = containTerrain[cellIndex];
            che.level = CELL_LEVEL;
            // dataLengths don't matter
        }
        public static void FillGameObjectCount(ProtobufSerializer.LoopHeader lh) {
            if (!deserializingCells) {
                lh.Count = 1;
            }
            
            lh.Count = entitiesNow != null ? entitiesNow.NumEntities : 0;
        }
        public static void FillDataForGameObject(ProtobufSerializer.GameObjectData goData, int i) {

            goData.Id = System.Guid.NewGuid().ToString();

            if (!deserializingCells) {
                goData.ClassId = "94a577fe-b9bc-4f37-a2d4-24a59b0bba2d"; // batch root
                goData.Parent = "";
                return;
            }

            goData.ClassId = entitiesNow[i].classId;
            gameObjectIndex = i;
            if (i == 0) {
                cellRootUid = goData.Id;
                goData.Parent = "";
            } else {
                goData.Parent = cellRootUid;
            }
            goData.Layer = 0;
            goData.Tag = "Untagged";
            goData.OverridePrefab = false;
            goData.CreateEmptyObject = false;
            goData.MergeObject = false;
            goData.IsActive = true;
        }
        public static void FillComponentCount(ProtobufSerializer.LoopHeader lh) {
            
            lh.Count = (deserializingCells) ? entitiesNow.GetComponentCount(gameObjectIndex) : 2;
        }
        public static void FillComponentHeader(ProtobufSerializer.ComponentHeader ch, int i) {

            if (!deserializingCells) {
                if (i == 0) {
                    ch.TypeName = typeof(Transform).FullName;
                } else {
                    ch.TypeName = typeof(LargeWorldBatchRoot).FullName;
                }
                ch.IsEnabled = true;
                return;
            }

            entitiesNow.CopyComponent(ch, i);
        }
        public static void ProcessComponent(Component component) {
            if (!deserializingCells) return;
            if (component is null) return;
            if (component is Transform) {

                var data = entitiesNow[gameObjectIndex];
                (component as Transform).localPosition = data.globalPosition;
                (component as Transform).rotation = data.rotation;

                RandomWorldsJournalist.LogStream($"Transform's local position is: {(component as Transform).localPosition}\n");
            }
            else if (component is EntitySlotsPlaceholder) {
                var slots = component as EntitySlotsPlaceholder;
                slots.slotsData = WorldManager.generator.FillEntitySlots(entitiesNow.GetRootVoxelPosition());
            }
        }

        public static void EnterBatchObjectsLoad() {
            deserializingCells = false;
            RandomWorlds.ignoreHeader = false;
        }
        public static void EnterCellLoading() {
            deserializingCells = true;
            RandomWorlds.ignoreHeader = true;
        }
    }

    public class CellEntities {
        private List<EntityData> entities;
        public int NumEntities {
            get {
                return (entities is null) ? 0 : entities.Count;
            }
        }

        public CellEntities() {
            Clear();
        }
        public CellEntities(Int3 cellIndex) {
            Clear();
            AddRoot(cellIndex.ToVector3() * 32);
        }

        public void Clear() {
            if (entities is null) entities = new List<EntityData>();
            entities.Clear();
        }
        public void AddRoot(Vector3 rootPos) {
            var root_data = new EntityData("55d7ab35-de97-4d95-af6c-ac8d03bb54ca");
            root_data.globalPosition = rootPos;
            entities.Insert(0, root_data);
        }
        public void AddEntity(EntityData newEntity, Vector3 voxelandPos) {
            newEntity.globalPosition = (voxelandPos - WorldManager.worldCenterVoxel) - entities[0].globalPosition;
            entities.Add(newEntity);
        }

        public EntityData this[int i] {
            get {
                return entities[i];
            }
        }

        public int GetComponentCount(int i) {
            return entities[i].componentHeaders.Count;
        }
        public void CopyComponent(ProtobufSerializer.ComponentHeader ch, int i) {
            ch.TypeName = entities[i].componentHeaders[i].TypeName;
            ch.IsEnabled = entities[i].componentHeaders[i].IsEnabled;
        }
        public Vector3 GetRootVoxelPosition() => entities[0].globalPosition;
    }

    public class EntityData {
        public string classId;
        public Vector3 globalPosition;
        public Quaternion rotation;
        public List<ProtobufSerializer.ComponentHeader> componentHeaders;

        public EntityData(string _classId) {
            classId = _classId;
            globalPosition = Vector3.zero;
            rotation = Quaternion.identity;
            var transformHeader = new ProtobufSerializer.ComponentHeader() {
                TypeName = typeof(Transform).FullName,
                IsEnabled = true
            };
            componentHeaders = new List<ProtobufSerializer.ComponentHeader>() { transformHeader };
        }
        public EntityData(string _classId, Vector3 _rotation) {
            classId = _classId;
            globalPosition = Vector3.zero;
            rotation = Quaternion.Euler(_rotation);

            var transformHeader = new ProtobufSerializer.ComponentHeader() {
                TypeName = typeof(Transform).FullName,
                IsEnabled = true
            };
            componentHeaders = new List<ProtobufSerializer.ComponentHeader>() { transformHeader };
        }

        public void AddComponent(System.Type componentType, bool enabled = true) {
            componentHeaders.Add(new ProtobufSerializer.ComponentHeader() { TypeName = componentType.FullName, IsEnabled = enabled });
        }
    }
}
