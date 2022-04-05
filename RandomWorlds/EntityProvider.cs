using RandomWorlds.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace RandomWorlds {
    public class EntityProvider {

        private static EntityProvider main;

        public static EntityProvider GetInstance() {
            if (main == null) new EntityProvider();
            return main;
        }

        private readonly Dictionary<Int3, List<IEntityApplicator>> entityApplicators;
        private IEntityProvider activeProvider;

        private const int CELL_LEVEL = 1;
        private const int CELLS_PER_BATCH = 5;

        private EntityProvider() {
            main = this;

            entityApplicators = new Dictionary<Int3, List<IEntityApplicator>>();
        }

        public void SubscribeApplicator(IEntityApplicator entityApplicator) {
            var batchBounds = entityApplicator.GetBatchBounds();
            var cellBounds = new Int3.Bounds(batchBounds.mins * CELLS_PER_BATCH, batchBounds.maxs * CELLS_PER_BATCH + CELLS_PER_BATCH - 1);
            foreach (var cellId in cellBounds) {
                if (!entityApplicators.TryGetValue(cellId, out List<IEntityApplicator> things)) {
                    things = new List<IEntityApplicator>();
                    entityApplicators.Add(cellId, things);
                }

                things.Add(entityApplicator);
            }
        }

        public static void OnCellRootAwoken(EntityCell cell) {
            var globalCellId = cell.cellId + cell.batchId * CELLS_PER_BATCH;
            
            if (main.entityApplicators.TryGetValue(globalCellId, out var applicators)) {

                // apply all entity additions
                var cellEntities = new CellEntities(globalCellId);
                applicators.ForEach(a => a.Apply(cellEntities));
                EnterCellLoading(cellEntities);
            } else {
                EnterCellLoading(null); // reset!
            }
        }

        public static bool PrecomputeCellMask(Int3 batchId) {
            var tet = main.activeProvider as CellEntityProvider;
            return tet.PrecomputeCellMask(batchId);
        }
        public static void ProvideCellFileHeader(CellManager.CellsFileHeader cfh, Int3 batchId) {
            var tet = main.activeProvider as CellEntityProvider;
            tet.ProvideCellsFileHeader(cfh, batchId);
        }

        public static void ProvideCellHeader(CellManager.CellHeaderEx che, int cellIndex) {
            var tet = main.activeProvider as CellEntityProvider;
            tet.ProvideCellHeader(che, cellIndex);
        }

        public static void FillGameObjectCount(ProtobufSerializer.LoopHeader lh) {
            lh.Count = main.activeProvider.CountGameObjects();
        }
        public static void FillDataForGameObject(ProtobufSerializer.GameObjectData goData, int i) {

            goData.Id = System.Guid.NewGuid().ToString();
            goData.Layer = 0;
            goData.Tag = "Untagged";
            goData.OverridePrefab = false;
            goData.CreateEmptyObject = false;
            goData.MergeObject = false;
            goData.IsActive = true;

            main.activeProvider.ProvideGameObjectData(i, goData); 
        }

        public static void FillComponentCount(ProtobufSerializer.LoopHeader lh) {
            lh.Count = main.activeProvider.CountComponentsNow();
        }

        public static void FillComponentHeader(ProtobufSerializer.ComponentHeader ch, int i) {
            main.activeProvider.ProvideComponentHeader(i, ch);
        }
        public static void ProcessComponent(Component component) {
            main.activeProvider.ProvideComponent(component);
        }

        public static void EnterBatchObjectsLoad() {
            main.activeProvider = new BatchEntityProvider();
            ProtobufSerializer_TryDeserializeStreamHeaderPatch.ignoreHeader = false;
        }
        public static void EnterCellLoading(CellEntities entities) {
            main.activeProvider = new CellEntityProvider(entities);
            ProtobufSerializer_TryDeserializeStreamHeaderPatch.ignoreHeader = true;
        }

        private interface IEntityProvider {
            int CountComponentsNow();
            void ProvideGameObjectData(int i, ProtobufSerializer.GameObjectData goData);
            int CountGameObjects();
            void ProvideComponentHeader(int i, ProtobufSerializer.ComponentHeader compHeader);
            void ProvideComponent(Component component);
        }

        private class CellEntityProvider : IEntityProvider {
            private CellEntities _entitiesNow;
            private int _currentGOIndex;
            private string _cellRootUId; 
            
            private static List<Int3> nonemptyCells = new List<Int3>();

            public CellEntityProvider(CellEntities entitiesNow) {
                _currentGOIndex = 0;
                _entitiesNow = entitiesNow;
            }

            public bool PrecomputeCellMask(Int3 batchId) {

                nonemptyCells.Clear();

                foreach (Int3 index in Int3.Range(CELLS_PER_BATCH)) {
                    var globalcid = index + batchId * CELLS_PER_BATCH;
                    //if (_entitiesNow..ContainsKey(globalcid)) {
                    //    nonemptyCells.Add(index);
                    //}
                }

                return nonemptyCells.Count > 0;
            }
            public void ProvideCellsFileHeader(CellManager.CellsFileHeader cfh, Int3 batch) {
                cfh.numCells = nonemptyCells.Count;
                cfh.version = 9;

                RandomWorldsJournalist.LogStream($"Creating cell file for batch: {batch} with {cfh.numCells} cells.\n");
            }
            public void ProvideCellHeader(CellManager.CellHeaderEx che, int cellIndex) {
                che.cellId = nonemptyCells[cellIndex];
                che.level = CELL_LEVEL;
                // dataLengths don't matter
            }
            public int CountGameObjects() {
                return _entitiesNow.NumEntities;
            }

            public void ProvideGameObjectData(int i, ProtobufSerializer.GameObjectData goData) {
                goData.ClassId = _entitiesNow[_currentGOIndex].classId;
                if (_currentGOIndex == 0) {
                    _cellRootUId = goData.Id;
                    goData.Parent = "";
                } else {
                    goData.Parent = _cellRootUId;
                }

                _currentGOIndex = i;
            }

            public int CountComponentsNow() {
                return _entitiesNow.CountComponentsOfEntity(_currentGOIndex);
            }

            public void ProvideComponentHeader(int i, ProtobufSerializer.ComponentHeader compHeader) {
                _entitiesNow.ProvideComponent(i, compHeader);
            }

            public void ProvideComponent(Component component) {
                if (component is null) return;
                if (component is Transform) {

                    var data = _entitiesNow[_currentGOIndex];
                    (component as Transform).localPosition = data.globalPosition;
                    (component as Transform).rotation = data.rotation;

                    RandomWorldsJournalist.LogStream($"Transform's local position is: {(component as Transform).localPosition}\n");
                } else if (component is EntitySlotsPlaceholder) {
                    var slots = component as EntitySlotsPlaceholder;
                    //slots.slotsData = WorldManager.generator.FillEntitySlots(entitiesNow.GetRootVoxelPosition());
                }
            }
        }

        private class BatchEntityProvider : IEntityProvider {
            public int CountComponentsNow() {
                return 2;
            }

            public void ProvideGameObjectData(int i, ProtobufSerializer.GameObjectData goData) {
                goData.ClassId = "94a577fe-b9bc-4f37-a2d4-24a59b0bba2d"; // batch root
                goData.Parent = "";
            }

            public int CountGameObjects() {
                return 1;
            }

            public void ProvideComponentHeader(int i, ProtobufSerializer.ComponentHeader compHeader) {
                if (i == 0) {
                    compHeader.TypeName = typeof(Transform).FullName;
                } else {
                    compHeader.TypeName = typeof(LargeWorldBatchRoot).FullName;
                }
                compHeader.IsEnabled = true;
            }

            public void ProvideComponent(Component component) {
            }
        }
    }

    public class CellEntities {
        private List<EntityData> _entities;

        public EntityData this[int i] {
            get {
                return _entities[i];
            }
        }

        public int NumEntities {
            get {
                return (_entities is null) ? 0 : _entities.Count;
            }
        }

        public CellEntities() {
            Clear();
        }
        public CellEntities(Int3 cellIndex) {
            Clear();
            AddEntityRoot(cellIndex.ToVector3() * 32);
        }

        public void Clear() {
            if (_entities is null) _entities = new List<EntityData>();
            _entities.Clear();
        }

        public void AddEntityRoot(Vector3 rootPos) {
            var root_data = new EntityData("55d7ab35-de97-4d95-af6c-ac8d03bb54ca");
            root_data.globalPosition = rootPos;
            _entities.Insert(0, root_data);
        }
        
        public void AddEntity(EntityData newEntity, Vector3 voxelandPos) {
            newEntity.globalPosition = voxelandPos - WorldConfiguration.ORIGIN_VOXEL - _entities[0].globalPosition;
            _entities.Add(newEntity);
        }

        public int CountComponentsOfEntity(int i) {
            return _entities[i].componentHeaders.Count;
        }
        public void ProvideComponent(int i, ProtobufSerializer.ComponentHeader ch) {
            ch.TypeName = _entities[i].componentHeaders[i].TypeName;
            ch.IsEnabled = _entities[i].componentHeaders[i].IsEnabled;
        }
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
