using UnityEngine;

namespace MainCore.Common
{
    public abstract class SettingBase<T> : MonoBehaviour, IValueSet<T>
    {
        [SerializeField] protected MonoBehaviour dataContainer;

        [SerializeField] protected string dataTag;

        [SerializeField] protected T defaultValue;

        void Awake()
        {
            OnStart();
        }

        public abstract T GetValue();

        public abstract void SetValue(T value);

        public abstract void SaveValue();

        protected abstract void OnStart();
    }

    public interface IValueSet<T>
    {
        public T GetValue();

        public void SetValue(T value);

        public void SaveValue();
    }
}