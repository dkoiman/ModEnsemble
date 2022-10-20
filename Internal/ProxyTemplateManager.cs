using System.Reflection;

using PavonisInteractive.TerraInvicta;

namespace ModEnsemble.Internal {
    static class ProxyTemplateManager {
        public static void RegisterFileBasedTemplates(string templatePath) {
            var method = typeof(TemplateManager).GetMethod(
                "RegisterFileBasedTemplates",
                BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(
                obj: null,
                parameters: new object[] { templatePath });
        }

        public static void ValidateAllTemplates() {
            var method = typeof(TemplateManager).GetMethod(
                "ValidateAllTemplates",
                BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(
                obj: null,
                parameters: new object[] { });
        }

        public static TIDataTemplate Find(string name, System.Type type, bool allow_children) {
            var method = typeof(TemplateManager).GetMethod("Find", BindingFlags.Static | BindingFlags.NonPublic);
            return method.Invoke(
                obj: null,
                parameters: new object[] { name, type, allow_children }) as TIDataTemplate;
        }

        public static bool foundGlobal {
            get {
                return (bool)typeof(TemplateManager).GetField("foundGlobal").GetValue(TemplateManager.self);
            }
        }
    }
}
