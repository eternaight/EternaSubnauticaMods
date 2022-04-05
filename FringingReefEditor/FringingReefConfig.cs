using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace FringingReefEditor {

    [Menu("Reef Editor Config")]
    public class FringingReefConfig : ConfigFile {
        [Toggle("Enable world editing mode")]
        public bool editing;
    }
}