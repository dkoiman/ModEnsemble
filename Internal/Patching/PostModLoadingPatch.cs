using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;
using UnityModManagerNet;

using ModEnsemble.Library;

namespace ModEnsemble.Internal.Patching {

    [HarmonyPatch(typeof(UnityModManager))]
    class PostModLoadingPatch {
        static void Postfix() {
            VLog.Debug(VLog.Level.VLOG_1, "Reload templates after all mods are loaded");
            AlternateTemplateLoadManager.Reset();
        }

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> PatchInventoryMethods() {
            var nested = typeof(UnityModManager).GetNestedType("GameScripts", BindingFlags.NonPublic);
            yield return nested.GetMethod("OnAfterLoadMods");
            yield return nested.GetMethod("OnModToggle");
        }
    }
}
