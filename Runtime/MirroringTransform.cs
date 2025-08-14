#nullable enable
using System;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [AddComponentMenu("ReinaSakiria's P-Angel's Stealers Utility/RsPASU MirroringTransform")]
    public sealed class MirroringTransform : MonoBehaviour, INDMFEditorOnly
    {
        public Axis MirrorAxis = Axis.X;
        public Transform? DestinationTransform = null;
        public Transform? MirrorOrigin = null;

        public void Start()
        {
            // no op
        }
    }

    [Flags]
    public enum Axis
    {
        X = 1,
        Y = 2,
        Z = 4,
    }
}
