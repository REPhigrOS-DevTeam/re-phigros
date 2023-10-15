using System;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace MainCore.Common
{
    public abstract class SettingBase<T, T1> : MonoBehaviour, IValueSet<T1> where T : MonoBehaviour
    {
        protected T DataContainer;

        [SerializeField] protected string dataTag;

        public string DataTag => dataTag;

        [SerializeField] protected T1 defaultValue;

        void Awake()
        {
            DataContainer = gameObject.GetComponent<T>();
            OnStart();
        }

        public abstract T1 GetValue();

        public abstract void SetValue(T1 value);

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