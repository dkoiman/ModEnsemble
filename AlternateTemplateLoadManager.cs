using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems.Bootstrap;

using ModEnsemble.Internal;
using ModEnsemble.Library;

namespace ModEnsemble {
    public struct ModTemplate {
        public string typeName;
        public string modName;
        public string[] jsonFiles;
    }

    public class AlternateTemplateLoadManager {
        private readonly Dictionary<string, Dictionary<string, ModTemplate>> templates =
            new Dictionary<string, Dictionary<string, ModTemplate>>();

        private readonly Dictionary<string, System.Type> types =
            new Dictionary<string, System.Type>();

        private readonly Dictionary<string, System.Type> typeExtensions =
            new Dictionary<string, System.Type>();

        public static readonly AlternateTemplateLoadManager self = new AlternateTemplateLoadManager();

        public const string kVanilaId = "___V_A_N_I_L_A___";

        AlternateTemplateLoadManager() {
            FixVanilaJson(Application.streamingAssetsPath + Utilities.templateFolder);
        }

        // Some Vanila Json files are malformed, so we have to fix them to load correctly.
        private void FixVanilaJson(string templatePath) {
            File.Copy(
                "Mods/Enabled/ModEnsemble/JsonFixes/TIMapGroupVisualizerTemplate.json_",
                templatePath + "/TIMapGroupVisualizerTemplate.json",
                true);
            File.Copy(
                "Mods/Enabled/ModEnsemble/JsonFixes/TISpaceFleetTemplate.json_",
                templatePath + "/TISpaceFleetTemplate.json",
                true);
        }

        private void Initialize(string templatePath) {
            VLog.Debug(VLog.Level.VLOG_1, "Initializing AlternateTemplateManager");
            RegisterVanilaTemplates(templatePath);
            ScanForJsonMods();
            ModHook_ManualTemplateRegistration();
        }

        private void Reload(string templatePath, bool soft = false) {
            if (!soft) {
                TemplateManager.ClearAllTemplates();
            }
            templates.Clear();
            types.Clear();
            typeExtensions.Clear();
            Initialize(templatePath);
            LoadImpl();
        }

        private void RegisterImpl(ModTemplate template) {
            VLog.Debug(
                VLog.Level.VLOG_1, 
                "Registering Template. Source: " + template.modName + " Template: " + template.typeName);
            var type = TemplateLoader.FindDataTemplateType(template.typeName);
            if (type == null) {
                VLog.Debug(VLog.Level.VLOG_0, "Template type not found: " + template.typeName);
                return;
            }
            types[template.typeName] = type;

            if (!templates.ContainsKey(template.typeName)) {
                templates.Add(template.typeName, new Dictionary<string, ModTemplate>());
            }
            var templateMods = templates[template.typeName];
            templateMods.Add(template.modName, template);
        }

        private void RegisterVanilaTemplates(string templatePath) {
            string[] files = System.IO.Directory.GetFiles(templatePath, "*.json");
            foreach (var file in files) {
                ModTemplate template = new ModTemplate();
                template.typeName = System.IO.Path.GetFileNameWithoutExtension(file);
                template.modName = kVanilaId;
                template.jsonFiles = new string[] { file };
                RegisterImpl(template);
            }
        }

        private void ScanForJsonMods() {
            VLog.Debug(VLog.Level.VLOG_0, "Scanning for mods");
            string[] mods = Directory.EnumerateDirectories("Mods/Enabled/").ToArray<string>();
            foreach (var mod in mods) {
                string modName = Path.GetFileName(mod);
                foreach (var file in Directory.GetFiles(mod, "*.json", SearchOption.AllDirectories)) {
                    string templateType = Path.GetFileNameWithoutExtension(file);
                    if (templateType == "ModInfo") {
                        continue;
                    }
                    ModTemplate template = new ModTemplate();
                    template.typeName = templateType;
                    template.modName = modName;
                    template.jsonFiles = new string[] { file };
                    RegisterImpl(template);
                }
            }
        }

        private System.Type ResolveTemplateType(string templateType) {
            if (self.typeExtensions.ContainsKey(templateType)) {
                return self.typeExtensions[templateType];
            }
            return self.types[templateType];
        }

        private void LoadImpl() {
            Log.Time("Load Templates (alternative loader)", delegate () {
                foreach (var templateType in self.templates) {
                    List<string> jsonFiles = new List<string>();
                    if (templateType.Value.ContainsKey(kVanilaId)) {
                        jsonFiles.AddRange(templateType.Value[kVanilaId].jsonFiles);
                    }
                    foreach (var mod in templateType.Value) {
                        if (mod.Key != kVanilaId) {
                            jsonFiles.AddRange(mod.Value.jsonFiles);
                        }
                    }
                    VLog.Debug(VLog.Level.VLOG_3, "Loading: " + templateType.Key);
                    TemplateLoader.MergeAndLoad(jsonFiles.ToArray(), ResolveTemplateType(templateType.Key));
                }
                ProxyTemplateManager.ValidateAllTemplates();
            });

            ModHook_InCodeTemplateOverride();
        }

        private Dictionary<string, ModTemplate> FindImpl(string typeName) {
            if (!self.templates.ContainsKey(typeName)) {
                return null;
            }
            return self.templates[typeName];
        }

        private void RemoveImpl(string typeName, string modName) {
            if (!self.templates.ContainsKey(typeName) || !self.templates[typeName].ContainsKey(modName)) {
                return;
            }
            self.templates[typeName].Remove(modName);
            if (self.templates[typeName].Count == 0) {
                self.templates.Remove(typeName);
            }
        }

        // Public API

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ModHook_ManualTemplateRegistration() { }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ModHook_InCodeTemplateOverride() { }

        public static void Reset() {
            self.Reload(Application.streamingAssetsPath + Utilities.templateFolder, false);
        }

        public static void Reload() {
            self.Reload(Application.streamingAssetsPath + Utilities.templateFolder, true);
        }

        public static void RegisterTemplateExtension(string baseType, System.Type extension) {
            self.typeExtensions.Add(baseType, extension);
        }

        public static void Register(ModTemplate template) {
            self.RegisterImpl(template);
        }

        public static Dictionary<string, ModTemplate> Find(string typeName) {
            return self.FindImpl(typeName);
        }

        public static void Remove(string typeName, string modName) {
            self.RemoveImpl(typeName, modName);
        }

        public static IEnumerable<KeyValuePair<string, System.Type>> IterateExtensions() {
            foreach (var pair in self.typeExtensions) {
                yield return pair;
            }
        }
    }

}
