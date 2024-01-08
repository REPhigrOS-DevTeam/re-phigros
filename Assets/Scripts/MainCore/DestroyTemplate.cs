using UnityEngine;

namespace MainCore
{
    public class DestroyTemplate : MonoBehaviour
    {
        public void DestroyThis()
        {
            Destroy(gameObject);
        }
    }
}