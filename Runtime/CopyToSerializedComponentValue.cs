#nullable enable
using System;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    [AddComponentMenu("ReinaSakiria's P-Angel's Stealers Utility/RsPASU CopyToSerializedComponentValue")]
    public sealed class CopyToSerializedComponentValue : PASUComponent
    {
        public Component? Source = null;
        public Component? Destination = null;
    }


}
