using System.IO;
using Logger = QModManager.Utility.Logger;

namespace RandomWorlds {
    public static class RandomWorldsJournalist {

        private static StreamWriter stream;
        public static void Log(int level, string msg) {
            Logger.Level logLevel;
            switch (level) {
                case 0:
                default:
                    logLevel = Logger.Level.Info;
                    break;
                case 1:
                    logLevel = Logger.Level.Warn;
                    break;
                case 2:
                    logLevel = Logger.Level.Error;
                    break;

            }
            Logger.Log(logLevel, msg);
        }

        public static void LogDeserializedLoop(ProtobufSerializer.LoopHeader deserializedObject) {
            LogStream($"{deserializedObject.Count} entries:\n");
        }
        public static void LogDeserializedGOData(ProtobufSerializer.GameObjectData goData) {

            UWE.PrefabDatabase.TryGetPrefabFilename(goData.ClassId, out string filename);
            var entry = $"Deserialized game object: {filename}({goData.ClassId}) \nparent: {goData.Parent} \nmerge:{goData.MergeObject}, override:{goData.OverridePrefab}, active: {goData.IsActive}, createEmpty:{goData.CreateEmptyObject}\n";
            LogStream(entry);
        }
        public static void LogComponentHeader(ProtobufSerializer.ComponentHeader deserializedObject) {
            LogStream($"Deserialized component: {deserializedObject.TypeName}\n");
        }
        public static void LogCellHeader(CellManager.CellHeaderEx chex) {
            LogStream($"\nDeserializing cell header: id: {chex.cellId}, level: {chex.level}\n");
        }
        public static void LogCellFileHeader(CellManager.CellsFileHeader chf) {
            LogStream($"\nDeserializing cell file header: {chf.numCells} cell roots\n");
        }

        public static void LogStream(string message) {
            if (stream == null) {
                stream = File.CreateText(Path.Combine(RandomWorlds.ModDirectory, "entityLog.txt"));
            }

            stream.Write(message);
        }
    }
}
