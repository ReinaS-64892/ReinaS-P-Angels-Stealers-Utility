#nullable enable
using System;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [AddComponentMenu("ReinaSakiria's P-Angel's Stealers Utility/RsPASU AnkletBraceletStealerScaler")]
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class AnkletBraceletStealerScaler : MonoBehaviour, INDMFEditorOnly
    {
        [Range(0, 3)] public float Scale = 1f;
        public float BaseRadius;
        public float BaseRateOverTime;
        public int BaseMaxParticle;
        public void Start()
        {
            // no op
        }
    }

}
