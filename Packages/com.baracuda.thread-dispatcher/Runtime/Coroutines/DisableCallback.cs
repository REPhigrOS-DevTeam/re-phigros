using System;
using UnityEngine;

namespace Baracuda.Threading.Coroutines
{
    [DisallowMultipleComponent]
    internal sealed class DisableCallback : MonoBehaviour, IDisableCallback
    {
        private void OnDisable()
        {
            Disabled?.Invoke();
            Disabled = null;
        }

        public event Action Disabled;
    }
}