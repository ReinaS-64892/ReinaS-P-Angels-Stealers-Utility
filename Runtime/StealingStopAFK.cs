#nullable enable
using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [AddComponentMenu("ReinaSakiria's P-Angel's Stealers Utility/RsPASU StealingStopAFK")]
    public sealed class StealingStopAFK : PASUComponent
    {
        public List<Texture2D> ReinaSEditTargetTextures = new();
        public List<Material> InSuspendingEmissionTargetMaterials = new();
    }


}
