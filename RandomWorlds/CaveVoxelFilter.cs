using RandomWorlds.NoiseAdventures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomWorlds {
    class CaveVoxelFilter : IVoxelFilter {

        private int numCaverns = 1;
        private int numConnectors = 2;
        private readonly List<LargeCave> caverns;
        private readonly List<CaveConnector> connectors;

        public CaveVoxelFilter(int _seed, int _numCaverns, IHeightmapProvider heightmap) {
            numCaverns = _numCaverns;

            Random.InitState(_seed + 31);
            var randomPoint = GetRandomPoint(500);

            connectors = new List<CaveConnector>();
            for (int i = 0; i < numConnectors; i++) {
                var entrance = GetSurfacePoint(500, heightmap);
                Random.InitState(_seed + 217 * i);
                connectors.Add(new CaveConnector(entrance, randomPoint, 100));
                RandomWorldsJournalist.Log(0, $"Cave voxel filter placed a cave entrance at world coordinate: {entrance - WorldManager.worldCenterVoxel}");
            }

        }
        
        public void Apply(Voxel voxel) {

            var distance = NoiseUtils.BooleanIntersect(connectors[0].GetDensity(voxel.position), connectors[1].GetDensity(voxel.position));
            
            if (voxel.signedDistance > distance) {
                voxel.signedDistance = distance;
            }
        }

        private Vector3 GetSurfacePoint(float radius, IHeightmapProvider heightmap) {
            var globalCoord2D = Random.insideUnitCircle * radius;
            var voxelCoord = new Vector3(globalCoord2D.x, 0, globalCoord2D.y) + WorldManager.worldCenterVoxel;
            // bring point away from center
            //point = new Vector2(NoiseUtils.SmoothStop(point.x), NoiseUtils.SmoothStop(point.y));
            var height = heightmap.GetHeight(voxelCoord) ;
            return new Vector3(voxelCoord.x, height, voxelCoord.z);
        }
        private Vector3 GetRandomPoint(float radius) {
            var globalCoord2D = Random.insideUnitCircle * radius;
            var depth = 2000 + 600 * NoiseUtils.SmoothStart(Random.value);
            return new Vector3(globalCoord2D.x, depth, globalCoord2D.y) + WorldManager.worldCenterVoxel;
        }

        private class LargeCave {
            public Vector3 origin;
            public Vector3 halfsize;

            public LargeCave(int seed, float maxRadius) {
                Random.InitState(seed);
                var planeOrigin = Random.insideUnitCircle * maxRadius;
                var depth = NoiseUtils.SmoothStart(Random.value);
                var worldSize = WorldManager.worldSizeInVoxels;
                origin = new Vector3(planeOrigin.x * worldSize.x, depth * worldSize.y, planeOrigin.y * worldSize.z);
                halfsize = new Vector3(NoiseUtils.SmoothStop(Random.value), Random.value, NoiseUtils.SmoothStop(Random.value)) * 300;
            }

            public float GetDensity(Vector3 voxelPosition) {
                var dx = NoiseUtils.BooleanIntersect(origin.x - voxelPosition.x - halfsize.x, voxelPosition.x - origin.x - halfsize.x);
                var dy = NoiseUtils.BooleanIntersect(origin.y - voxelPosition.y - halfsize.y, voxelPosition.y - origin.y - halfsize.y);
                var dz = NoiseUtils.BooleanIntersect(origin.z - voxelPosition.z - halfsize.z, voxelPosition.z - origin.z - halfsize.z);
                return NoiseUtils.BooleanIntersect(dx, dy, dz);
            }
        }

        private class CaveConnector {
            public Vector3 pointA;
            public Vector3 pointB;
            public float radius;
            public float length;

            public CaveConnector(Vector3 _voxelA, Vector3 _voxelB, float connectorRadius) {
                pointA = _voxelA;
                pointB = _voxelB;
                length = Vector3.Distance(_voxelA, _voxelB);
                radius = connectorRadius;
            }

            public float GetDensity(Vector3 voxelPosition) {
                var top = Vector3.Cross((voxelPosition - pointA), (voxelPosition - pointB)).magnitude / 2;
                return (top / length) - radius;
            }
        }
    }
}
