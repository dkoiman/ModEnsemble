using PavonisInteractive.TerraInvicta;

namespace ModEnsemble.Library {
    public static class VLog {
        public enum Level {
            VLOG_0 = 0,
            VLOG_1 = 1,
            VLOG_2 = 2,
            VLOG_3 = 3
        }

        public static int logLevel = 0;

        public static void Debug(Level lvl, string message) {
            if (logLevel >= (int)lvl) {
                Log.Debug(message);
            }
        }
    }
}
