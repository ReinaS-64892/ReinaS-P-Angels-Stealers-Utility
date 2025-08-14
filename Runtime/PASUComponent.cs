#nullable enable
using System;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    public abstract class PASUComponent : MonoBehaviour, INDMFEditorOnly
    {
        public void Start()
        {
            // no op
        }
    }
}
