using UnityEngine;

namespace MainCore.Common
{
    public abstract class SceneTransitAnimation : MonoBehaviour
    {
        // 返回值是毫秒，表示动画持续多久
        public abstract int Enter();
        public abstract int Quit();
    }
}