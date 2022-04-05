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
        public BiomeEntry[] biomes;
    }
    public class BiomeEntry {
        public BiomeProperties properties;
        public Int3 legendColor;
        public int count;
        
        public BiomeEntry(string name, int groundType, int bedrockType, int debugType, Int3 legendColor, int count) {
            properties = new BiomeProperties() {
                name = name,
                groundType = groundType,
                bedrockType = bedrockType,
                debugType = debugType
            };
            this.legendColor = legendColor;
            this.count = count;
        } 
    }
}
