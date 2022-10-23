using System.Reflection;

using HarmonyLib;
using UnityModManagerNet;
using UnityEngine;

using ModEnsemble.Library;

namespace ModEnsemble {
    public class Main {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;

        static bool Load(UnityModManager.ModEntry modEntry) {
            // Boiler plate initialization
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

            // Configure mod compoenents
            VLog.logLevel = 0;

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Soft-reload templates")) {
                AlternateTemplateLoadManager.Reload();
            }
            if (GUILayout.Button("Hard-reload templates (may cause crash)")) {
                AlternateTemplateLoadManager.Reset();
            }
            GUILayout.EndHorizontal();
        }
    }
}