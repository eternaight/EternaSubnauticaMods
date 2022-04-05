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

        public static void LogStream(string message) {
            if (stream == null) {
                stream = File.CreateText(Path.Combine(RandomWorlds.ModDirectory, "entityLog.txt"));
            }

            stream.Write(message);
        }
    }
}
