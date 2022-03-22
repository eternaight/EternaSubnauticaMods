using ECCLibrary;
using System.Collections.Generic;
using UnityEngine;

namespace EternalCreatureTest {
    class EternalCompanion : CreatureAsset {
        public override BehaviourType BehaviourType => BehaviourType.Shark;

        public override LargeWorldEntity.CellLevel CellLevel => LargeWorldEntity.CellLevel.Far;

        public override SwimRandomData SwimRandomSettings => new SwimRandomData(true, Vector3.one, 1, 1, 3);

        public override EcoTargetType EcoTargetType => EcoTargetType.Shark;

        public override AnimateByVelocityData AnimateByVelocitySettings => new AnimateByVelocityData(true);
        public override float UnderwaterDrag => 1;
        public override float Mass => 25;

        public override List<LootDistributionData.BiomeData> BiomesToSpawnIn => new List<LootDistributionData.BiomeData>()
        {
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.SafeShallows_Plants,
                count = 1,
                probability = 0.4f
            },
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.SafeShallows_ShellTunnel,
                count = 1,
                probability = 1f
            },
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.Kelp_Sand,
                count = 1,  
                probability = 0.2f
            }
        };

        public EternalCompanion(string classId, string friendlyName, string description, GameObject model, Texture2D spriteTexture) : base(classId, friendlyName, description, model, spriteTexture) {
        }

        public override void AddCustomBehaviour(CreatureComponents components) {
        }

        public override void SetLiveMixinData(ref LiveMixinData liveMixinData) {
            liveMixinData.maxHealth = 100;
        }
    }
}
