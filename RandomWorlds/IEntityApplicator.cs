using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomWorlds {
    public interface IEntityApplicator {
        void Apply(CellEntities entities);
        Int3.Bounds GetBatchBounds();
    }
}
