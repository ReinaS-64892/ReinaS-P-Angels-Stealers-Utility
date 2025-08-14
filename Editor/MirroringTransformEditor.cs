#nullable enable
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [CustomEditor(typeof(MirroringTransform))]
    [CanEditMultipleObjects]
    public sealed class MirroringTransformEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var i in targets.OfType<MirroringTransform>())
            {
                if (i == null) { continue; }
                if (Valid(i) is false) { continue; }

                Undo.RecordObject(i, "RsPASU Mirroring");
                Undo.RecordObject(i.DestinationTransform, "RsPASU Mirroring");

                Mirroring(i);
            }
        }

        public static bool Valid(MirroringTransform mirroringTransform)
        {
            if (mirroringTransform.enabled is false) { return false; }

            var src = mirroringTransform.transform;
            var dist = mirroringTransform.DestinationTransform;

            var origin = mirroringTransform.MirrorOrigin;
            if (origin == null) { origin = src.parent; }

            if (src == null) { return false; }
            if (dist == null) { return false; }
            if (origin == null) { return false; }
            return true;
        }
        public static void Mirroring(MirroringTransform mirroringTransform)
        {
            var src = mirroringTransform.transform;
            var dist = mirroringTransform.DestinationTransform;

            var origin = mirroringTransform.MirrorOrigin;
            if (origin == null) { origin = src.parent; }

            if (src == null) { return; }
            if (dist == null) { return; }
            if (origin == null) { return; }

            var pos = src.position;
            pos = origin.InverseTransformPoint(pos);

            if (mirroringTransform.MirrorAxis is Axis.X) { pos.x *= -1; }
            if (mirroringTransform.MirrorAxis is Axis.Y) { pos.y *= -1; }
            if (mirroringTransform.MirrorAxis is Axis.Z) { pos.z *= -1; }

            pos = origin.TransformPoint(pos);
            dist.position = pos;

            // TODO : scale and rotation
        }
    }
    public sealed class MirroringTransformPass : Pass<MirroringTransformPass>
    {
        protected override void Execute(BuildContext context)
        {
            foreach (var i in context.AvatarRootObject.GetComponentsInChildren<MirroringTransform>(true))
            {
                if (MirroringTransformEditor.Valid(i) is false) { continue; }
                MirroringTransformEditor.Mirroring(i);
            }
        }
    }
}
