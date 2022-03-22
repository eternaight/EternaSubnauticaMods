namespace RandomWorlds {
    class Settings {
        public static WorldSettings worldSettings {
            get {
                return new WorldSettings() {
                    seed = System.DateTime.Now.GetHashCode() + "Subnautica".GetHashCode() * 217
                };
            }
        }

        public static HeightmapSettings mainHeightmapSettings {
            get {
                return new HeightmapSettings() {
                    craterFalloff = 30,
                    craterFalloffStart = 50,
                    craterRadius = 1800,
                    edgeNoiseFrequency = 0.004f,
                    domainAngleFrequency = 0.0002f,
                    domainDistanceFrequency = 0.0015f,
                };
            }
        }

        public static BiomeGenerationSettings biomeGenSettings = new BiomeGenerationSettings() {
            biomePointCount = 22,
            poissonSpacing = 100,
            craterRadius = 1600,
            biomes = new BiomeSettings[] {
                // safe
                new BiomeSettings() {
                    bedrockType = 171,
                    surfaceType = 85,
                    decoration = new EntityData("7f4525ec-110b-42fd-9b36-eede1480f07d", new UnityEngine.Vector3(-90, 0, 0))
                },
                // kelp
                new BiomeSettings() {
                    bedrockType = 64,
                    surfaceType = 113,
                    decoration = new EntityData("7329db6b-7385-4e77-8afa-71830ead9350")
                },
                // grassy
                new BiomeSettings() {
                    bedrockType = 38,
                    surfaceType = 44,
                    decoration = new EntityData("898efb6d-b57b-41a3-9d3e-753fdc537651", new UnityEngine.Vector3(-90, 0, 0))
                },
                // islands
                new BiomeSettings() {
                    bedrockType = 224,
                    surfaceType = 221,
                    decoration = new EntityData("1b968abb-6b50-4679-8793-c66a1ce17e97")
                },
                // mushrooms
                new BiomeSettings() {
                    bedrockType = 39,
                    surfaceType = 3,
                    decoration = new EntityData("775b6835-bd08-40d2-b80e-ab0ddc539c45", new UnityEngine.Vector3(-90, 0, 0))
                },
                // koosh
                new BiomeSettings() {
                    bedrockType = 60,
                    surfaceType = 2,
                    decoration = new EntityData("a5076433-b586-4c4f-adff-b002028e8014", new UnityEngine.Vector3(-90, 0, 0))
                },
                // grandreef
                new BiomeSettings() {
                    bedrockType = 164,
                    surfaceType = 151,
                    decoration = new EntityData("1cafd118-47e6-48c4-bfd7-718df9984685")
                },
                // arctic
                new BiomeSettings() {
                    bedrockType = 151
                },
                // ilz
                new BiomeSettings() {
                    bedrockType = 151
                },
                // unassigned
                new BiomeSettings() {
                    bedrockType = 151
                },
                // crash
                new BiomeSettings() {
                    bedrockType = 19,
                    surfaceType = 21,
                    decoration = new EntityData("2f56b14c-d84c-407e-ad84-eab2df2fc09b")
                },
                // void
                new BiomeSettings() {
                    bedrockType = 18
                },
                // sparse
                new BiomeSettings() {
                    bedrockType = 38,
                    surfaceType = 62,
                    decoration = new EntityData("11ea0dd6-015f-4528-bed7-18de03f54911")
                },
                // dunes
                new BiomeSettings() {
                    bedrockType = 77,
                    surfaceType = 94,
                    decoration = new EntityData("c3316113-2d89-45d4-81d7-7dd7c254fa6d")
                },
                // blood
                new BiomeSettings() {
                    bedrockType = 72,
                    surfaceType = 70,
                    decoration = new EntityData("8c4ba581-e392-41ab-80a9-a4a2745dcfdb")
                },
                // mountains
                new BiomeSettings() {
                    bedrockType = 39,
                    surfaceType = 52,
                    decoration = new EntityData("7c6d23d1-4d59-49f8-ac12-b12dfa530beb", new UnityEngine.Vector3(-90,0,0))
                },
                // treaders
                new BiomeSettings() {
                    bedrockType = 206,
                    surfaceType = 213,
                    decoration = new EntityData("0e2a3f36-881b-4c84-8a02-5bb1da4b9f29")
                },
                // blood 2 electric boogalo
                new BiomeSettings() {
                    bedrockType = 73,
                    surfaceType = 71,
                    decoration = new EntityData("d69d04e9-bef6-4229-9bea-a76378cb0018")
                },
                // crag
                new BiomeSettings() {
                    bedrockType = 199,
                    surfaceType = 184,
                    decoration = new EntityData("99bbd145-d50e-4afb-bff0-27b33243642b")
                }
            }
        };
    }

    public class WorldSettings {
        public int seed;
    }

    public class HeightmapSettings {
        public int seed;
        public float craterRadius;
        public float craterFalloff;
        public float craterFalloffStart;
        public float edgeNoiseFrequency;
        public float domainAngleFrequency;
        public float domainDistanceFrequency;
    }

    public class BiomeGenerationSettings {
        public int seed;
        public int biomePointCount;
        public int poissonSpacing;
        public float craterRadius;
        public BiomeSettings[] biomes;
    }
    public class BiomeSettings {
        public byte bedrockType = 1;
        public byte surfaceType = 1;
        public EntityData decoration;
    }
}
