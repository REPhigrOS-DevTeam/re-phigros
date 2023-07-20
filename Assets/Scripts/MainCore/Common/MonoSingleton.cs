using UnityEngine;

namespace DRFV.inokana
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {

        public static T Instance { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = GetComponent<T>();
            }
            else Destroy(this);
            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}