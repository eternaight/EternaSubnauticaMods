using RandomWorlds.Layers;

namespace RandomWorlds {
    public static class WorldAssembler {
        public static void PushWorldLayer(WorldLayer layer) {
            if (layer is ITerrainApplicator) TerrainProvider.GetInstance().SubscribeApplicator(layer as ITerrainApplicator);
            if (layer is IEntityApplicator) EntityProvider.GetInstance().SubscribeApplicator(layer as IEntityApplicator);
            if (layer is IBiomeApplicator) SurfaceBiomeProvider.GetInstance().SubscribeBiomeSettings(layer as IBiomeApplicator);
        }

        public static void Initialize() {

        }
    }
}