using RandomWorlds.Layers;
using RandomWorlds.NoiseAdventures;

namespace RandomWorlds {
    public static class SNWorldBlueprint {
        public static void Apply() {

            int seed = "Subnautica".GetHashCode() + System.DateTime.Now.GetHashCode() * 31;

            WorldAssembler.PushWorldLayer(new BaseHeightmap(seed));

            var biomes = new SurfaceBiomeProvider(seed, 1600, 100);
            biomes.SubscribeBiomeSettings(new BiomeApplicator());
            biomes.Bake();
        }

        private class BaseHeightmap : WorldLayerHeightmap {
            public BaseHeightmap(int seed) {
                AddFilter(new RidgidNoiseFilter(0.0053f, 3, 1.76f, 0.3f, 800, seed));
                AddFilter(new FractalNoiseFilter(0.01f, 7, 2.1f, 0.4f, 175, seed));
            }
            public override float GetBaseHeight() {
                return 1000;
            }
        }

        private class BiomeApplicator : IBiomeApplicator {
            public BiomeEntry[] Retrieve() {
                return new BiomeEntry[] {
                    new BiomeEntry("safeShallows", 1, 35, 12,   new Int3(125, 127, 130), 3),
                    new BiomeEntry("kelpForest", 1, 35, 10,     new Int3(10, 210, 30), 3),
                    new BiomeEntry("grassyPlateaus", 1, 18, 7,  new Int3(200, 10, 20), 3),
                    new BiomeEntry("underwaterIslands", 1, 14, 5,   new Int3(210, 130, 10), 3),
                    new BiomeEntry("mushroomForest", 3, 18, 17,     new Int3(5, 25, 220), 3),
                    new BiomeEntry("kooshZone",3,18,12,     new Int3(90, 10, 230), 3),
                    new BiomeEntry("grandReef",151,18,10,   new Int3(10, 5, 240), 3),
                    new BiomeEntry("crashZone",20,18,6,     new Int3(220, 150, 30), 3),
                    new BiomeEntry("void",18,18,18,         new Int3(0, 0, 0), 3),
                    new BiomeEntry("sparseReef", 1, 18, 120,    new Int3(10, 240, 30), 3),
                    new BiomeEntry("dunes", 1, 18, 112,     new Int3(170, 170, 10), 3),
                    new BiomeEntry("bloodKelp", 1, 18, 120, new Int3(200, 10, 120), 3),
                    new BiomeEntry("mountains", 1, 18, 6,   new Int3(50, 50, 75), 3),
                    new BiomeEntry("seaTreaderPath", 152, 18, 152,  new Int3(40, 35, 210), 3),
                    new BiomeEntry("bloodKelpTwo", 1, 18, 120,      new Int3(240, 10, 150), 3),
                    new BiomeEntry("CragField", 199, 18, 199,       new Int3(230, 50, 50), 3),
                };
            }
        }
    }
}
