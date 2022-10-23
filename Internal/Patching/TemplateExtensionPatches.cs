using HarmonyLib;

using PavonisInteractive.TerraInvicta;

namespace ModEnsemble.Library.Internal {
    [HarmonyPatch(typeof(TemplateManager), "Initialize")]
    static class UseAlternateTemplateLoadManager {
        static bool Prefix(ref bool ___initialized) {
            if (!___initialized) {
                AlternateTemplateLoadManager.Reset();
                ___initialized = true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(StartMenuController), "OnStartSkirmishModeClicked")]
    static class ReloadWhenEnteringSkirmish {
        static void Prefix() {
            VLog.Debug(VLog.Level.VLOG_1, "Reload templates for skirmish");
            AlternateTemplateLoadManager.Reset();
        }
    }

    // Resolve extended classes localization to the base one, if not resolved already.
    [HarmonyPatch(typeof(Loc), "T", new System.Type[] { typeof(string) })]
    static class LocalizationPatch {
        static void Postfix(ref string __result, string key) {
            if (__result == key) {
                foreach (var ext in AlternateTemplateLoadManager.IterateExtensions()) {
                    if (key.StartsWith(ext.Value.Name)) {
                        __result = Loc.T(key.Replace(ext.Value.Name, ext.Key));
                    }
                }
            }
        }
    }

    // Resolve extended classes localization to the base one, if not resolved already.
    [HarmonyPatch(typeof(Loc), "T", new System.Type[] { typeof(string), typeof(object[]) })]
    static class LocalizationPatch2 {
        static void Postfix(ref string __result, string key, params object[] args) {
            if (__result == key) {
                foreach (var ext in AlternateTemplateLoadManager.IterateExtensions()) {
                    if (key.StartsWith(ext.Value.Name)) {
                        __result = Loc.T(key.Replace(ext.Value.Name, ext.Key), args);
                    }
                }
            }
        }
    }
}
