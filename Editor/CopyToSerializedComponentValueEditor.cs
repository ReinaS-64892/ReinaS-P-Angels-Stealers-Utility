#nullable enable
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [CustomEditor(typeof(CopyToSerializedComponentValue))]
    [CanEditMultipleObjects]
    public sealed class CopyToSerializedComponentValueEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var i in targets.OfType<CopyToSerializedComponentValue>())
            {
                if (i == null) { continue; }
                if (Valid(i) is false) { continue; }

                Undo.RecordObject(i.Source, "RsPASU DoCopy");
                Undo.RecordObject(i.Destination, "RsPASU DoCopy");

                DoCopy(i);
            }
        }

        public static bool Valid(CopyToSerializedComponentValue copyToSerializedComponentValue)
        {
            if (copyToSerializedComponentValue.enabled is false) { return false; }

            var src = copyToSerializedComponentValue.Source;
            var dist = copyToSerializedComponentValue.Destination;

            if (src == null) { return false; }
            if (dist == null) { return false; }

            if (src.GetType() != dist.GetType()) { return false; }

            return true;
        }
        public static void DoCopy(CopyToSerializedComponentValue copyToSerializedComponentValue)
        {
            var src = copyToSerializedComponentValue.Source;
            var dist = copyToSerializedComponentValue.Destination;

            EditorUtility.CopySerialized(src, dist);
        }
    }
    public sealed class CopyToSerializedComponentValuePass : Pass<CopyToSerializedComponentValuePass>
    {
        protected override void Execute(BuildContext context)
        {
            foreach (var i in context.AvatarRootObject.GetComponentsInChildren<CopyToSerializedComponentValue>(true))
            {
                if (CopyToSerializedComponentValueEditor.Valid(i) is false) { continue; }
                CopyToSerializedComponentValueEditor.DoCopy(i);
            }
        }
    }
}
