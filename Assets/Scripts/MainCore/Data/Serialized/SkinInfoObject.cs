using UnityEngine;

namespace MainCore.Data.Serialized
{
    [CreateAssetMenu(fileName = "New Skin Info", menuName = "RPGR/ Skin Info")]
    public class SkinInfoObject : ScriptableObject
    {
        [SerializeField] private SkinInfo currentData = new() { isExternal = false };

        public SkinInfo CurrentData => currentData;
    }
}