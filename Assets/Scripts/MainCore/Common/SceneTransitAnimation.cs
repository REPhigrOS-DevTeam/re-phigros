using UnityEngine;

namespace MainCore.Common
{
    public abstract class SceneTransitAnimation : MonoBehaviour
    {
        /// <summary>
        /// 初始化进入动画
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 进入动画
        /// </summary>
        /// <returns>动画持续多久</returns>
        public abstract int Enter();
        /// <summary>
        /// 退出动画
        /// </summary>
        /// <returns>动画持续多久</returns>
        public abstract int Quit();
    }
}