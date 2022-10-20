using UnityEngine;

namespace ModEnsemble.Library {
    // Replace a component attached to GameObject with an extended version of it.
    public class UnityComponentExtender {
        public static void Extend(GameObject obj, System.Type extType, System.Type baseType) {
            if (obj == null) {
                VLog.Debug(VLog.Level.VLOG_3, "Can't extend components for null");
                return;
            }

            var baseComponent = obj.GetComponent(baseType);
            if (baseComponent == null) {
                VLog.Debug(VLog.Level.VLOG_3, "Can't find component: " + baseType.Name);
                return;
            }

            var extComponent = obj.AddComponent(extType);
            ObjectManipulation.OverrideContent(extComponent, baseComponent, baseType);
            GameObject.Destroy(baseComponent);
        }
    }
}
