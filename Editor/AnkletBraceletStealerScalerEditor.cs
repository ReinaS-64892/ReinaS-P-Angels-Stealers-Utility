#nullable enable
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [CustomEditor(typeof(AnkletBraceletStealerScaler))]
    [CanEditMultipleObjects]
    public sealed class AnkletBraceletStealerScalerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var i in targets.OfType<AnkletBraceletStealerScaler>())
            {
                if (i == null) { continue; }
                if (Valid(i) is false) { continue; }

                Undo.RecordObject(i, "RsPASU DoScaling");
                Undo.RecordObject(i.GetComponent<ParticleSystem>(), "RsPASU DoScaling");

                DoScaling(i);
            }
        }

        public static bool Valid(AnkletBraceletStealerScaler ankletBraceletStealerScaler)
        {
            if (ankletBraceletStealerScaler.enabled is false) { return false; }
            return true;
        }
        public static void DoScaling(AnkletBraceletStealerScaler ankletBraceletStealerScaler)
        {
            var particleSystem = ankletBraceletStealerScaler.GetComponent<ParticleSystem>();
            var scaleFactor = ankletBraceletStealerScaler.Scale;

            var particleMain = particleSystem.main;
            particleMain.maxParticles = Mathf.CeilToInt(Mathf.LerpUnclamped(0f, ankletBraceletStealerScaler.BaseMaxParticle, scaleFactor));
            var particleShape = particleSystem.shape;
            particleShape.radius = Mathf.LerpUnclamped(0f, ankletBraceletStealerScaler.BaseRadius, scaleFactor);
            var particleEmission = particleSystem.emission;
            particleEmission.rateOverTime = Mathf.LerpUnclamped(0f, ankletBraceletStealerScaler.BaseRateOverTime, scaleFactor);
        }
    }
    public sealed class AnkletBraceletStealerScalerPass : Pass<AnkletBraceletStealerScalerPass>
    {
        protected override void Execute(BuildContext context)
        {
            foreach (var i in context.AvatarRootObject.GetComponentsInChildren<AnkletBraceletStealerScaler>(true))
            {
                if (AnkletBraceletStealerScalerEditor.Valid(i) is false) { continue; }
                AnkletBraceletStealerScalerEditor.DoScaling(i);
            }
        }
    }
}
