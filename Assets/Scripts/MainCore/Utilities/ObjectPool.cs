using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainCore.Utilities
{
    [Obsolete("这个类唯一的使用被我改成统一的HitEffectManager了")]
    public class ObjectPool
    {
        /// <summary>
        /// 对象池
        /// </summary>
        private Dictionary<string, List<GameObject>> pool;

        /// <summary>
        /// 预设体
        /// </summary>
        private Dictionary<string, GameObject> prefabs;

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public GameObject GetObj(string objName)
        {
            //结果对象
            GameObject result = null;
            //判断是否有该名字的对象池
            if (pool.ContainsKey(objName))
            {
                //对象池里有对象
                if (pool[objName].Count > 0)
                {
                    //获取结果
                    result = pool[objName][0];
                    //激活对象
                    result.gameObject.SetActive(true);
                    //从池中移除该对象
                    pool[objName].Remove(result);
                    //返回结果
                    for (int i = 0; i < result.transform.childCount; i++)
                        result.transform.GetChild(i).gameObject.SetActive(true);
                    return result;
                }
            }
            //如果没有该名字的对象池或者该名字对象池没有对象

            GameObject prefab = null;
            //如果已经加载过该预设体
            if (prefabs.ContainsKey(objName))
            {
                prefab = prefabs[objName];
            }
            else //如果没有加载过该预设体
            {
                //加载预设体
                prefab = Resources.Load<GameObject>(objName);
                //更新字典
                prefabs.Add(objName, prefab);
            }

            //生成
            result = GameObject.Instantiate(prefab);
            //改名（去除 Clone）
            result.name = objName;
            //返回
            return result;
        }

        /// <summary>
        /// 回收对象到对象池
        /// </summary>
        /// <param name="objName"></param>
        public void RecycleObj(GameObject obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
                obj.transform.GetChild(i).gameObject.SetActive(false);

            //设置为非激活
            obj.SetActive(false);
            //判断是否有该对象的对象池
            if (pool.ContainsKey(obj.name))
            {
                //放置到该对象池
                pool[obj.name].Add(obj);
            }
            else
            {
                //创建该类型的池子，并将对象放入
                pool.Add(obj.name, new List<GameObject>() {obj});
            }
        }

        public void reset()
        {
            pool.Clear();
            prefabs.Clear();
        }

        #region 单例

        private static ObjectPool instance;

        private ObjectPool()
        {
            pool = new Dictionary<string, List<GameObject>>();
            prefabs = new Dictionary<string, GameObject>();
        }

        public static ObjectPool GetInstance()
        {
            if (instance == null)
            {
                instance = new ObjectPool();
            }

            return instance;
        }

        #endregion
    }
}