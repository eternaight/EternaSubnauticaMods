using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomWorlds {
    public interface ITerrainApplicator {
        void Apply(Voxel voxel);
        Int3.Bounds GetBatchBounds();
    }
}
