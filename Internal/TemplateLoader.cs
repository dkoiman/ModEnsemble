using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Modding;

using ModEnsemble.Library;

namespace ModEnsemble.Internal {
    static class TemplateLoader {
        private static JsonController jsonController = new JsonController();

        // In-memory template json merger/updater
        public static void MergeAndLoad(string[] jsonFiles, System.Type type) {
            // Combine all input jsons into a single JObject array.
            List<JObject> combined = null;
            foreach (var jsonFile in jsonFiles) {
                VLog.Debug(VLog.Level.VLOG_3, "Processing json: " + jsonFile);
                JsonMod jsonMod = jsonController.LoadJson(jsonFile);
                if (jsonMod == null) {
                    VLog.Debug(VLog.Level.VLOG_0, "Invalid JSON: " + jsonFile);
                    continue;
                }
                combined =
                    combined == null
                    ? jsonMod.FileContents
                    : jsonController.CombineJson(combined, jsonMod.FileContents);
            }

            if (combined == null) {
                VLog.Debug(VLog.Level.VLOG_0, "No valid JSONs for type: " + type.Name);
                return;
            }

            // Deserialize into tempaltes.
            JArray jsonArray = new JArray(combined.ToArray());
            TIDataTemplate[] templates = StringSerializationAPI.Deserialize(type.MakeArrayType(), jsonArray.ToString()) as TIDataTemplate[];

            // Add or Update templates to the TemplateManager
            foreach (var template in templates) {
                if (template.dataName != null) {
                    TIDataTemplate currentTemplate = ProxyTemplateManager.Find(template.dataName, type, false);
                    if (currentTemplate == null) {
                        TemplateManager.Add(template, type, false);
                    } else {
                        VLog.Debug(VLog.Level.VLOG_3, "Override: " + template.dataName);
                        ObjectManipulation.OverrideContent(currentTemplate, template, type);
                    }

                } else {
                    VLog.Debug(VLog.Level.VLOG_0, "Attempting to add template of type " + type.ToString() + " with null dataName;");
                }
            }
        }

        // Generic overload of the above.
        public static void MergeAndLoad<T>(string[] jsons) where T : TIDataTemplate {
            MergeAndLoad(jsons, typeof(T));
        }

        public static System.Type FindDataTemplateType(string templateName) {
            var type = TemplateLoader.FindVanilaDataTemplateType(templateName);
            if (type == null) {
                type = TemplateLoader.FindModDataTemplateType(templateName);
            }
            return type;
        }

        public static System.Type FindModDataTemplateType(string templateName) {
            return Assembly.GetExecutingAssembly().GetType(templateName);
        }

        public static System.Type FindVanilaDataTemplateType(string templateName) {
            return Assembly.GetAssembly(typeof(TIDataTemplate)).GetType(templateName);
        }
    }
}
