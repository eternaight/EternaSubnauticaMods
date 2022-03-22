using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace RandomWorlds {
    [Menu("Random Words Options")]
    class RandomWorldsOptions : ConfigFile {
        [Toggle("Generate new world"), OnChange(nameof(OnGeneratePressed))]
        public bool toggleValue;

        [Slider("Progress slider", 0, 1, DefaultValue = 1)]
        public float loadProgress;

        private void OnGeneratePressed(ToggleChangedEventArgs e) {
#if !RUNTIME_GENERATION
            WorldManager.Initialize();
#endif
        }
    }
}
