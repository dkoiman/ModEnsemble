using System.Reflection;

namespace ModEnsemble.Library {
    public class ObjectManipulation {
        // Copies internal state of one object into another.
        public static void OverrideContent(System.Object dst, System.Object src, System.Type type) {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields) {
                if (field == null) {
                    continue;
                }
                VLog.Debug(VLog.Level.VLOG_2, "Trying patching: " + field.Name);
                field.SetValue(dst, field.GetValue(src));
            }
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props) {
                if (prop == null || !prop.CanWrite) {
                    continue;
                }
                VLog.Debug(VLog.Level.VLOG_2, "Trying patching: " + prop.Name);
                prop.SetValue(dst, prop.GetValue(src));
            }
        }

        // Generic overload of the above.
        public static void OverrideContent<T>(System.Object dst, System.Object src) where T : TIDataTemplate {
            OverrideContent(dst, src, typeof(T));
        }
    }
}
