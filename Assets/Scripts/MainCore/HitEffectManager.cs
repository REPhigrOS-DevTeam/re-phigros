using System.Collections.Generic;
using UnityEngine;

namespace MainCore
{
    public class HitEffectManager
    {
        private const int MaxEffectCount = 500;
        private static HitEffectManager instance;

        private Dictionary<string, List<EffectManager>> objectsInUse;

        private Dictionary<string, List<EffectManager>> pool;

        private Dictionary<string, EffectManager> prefabs;

        private HitEffectManager()
        {
            pool = new Dictionary<string, List<EffectManager>>();
            prefabs = new Dictionary<string, EffectManager>();
            objectsInUse = new();
        }

        public static HitEffectManager GetInstance()
        {
            if (instance == null)
            {
                instance = new HitEffectManager();
            }

            return instance;
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="judgeType"></param>
        /// <returns></returns>
        public EffectManager GetObj(HitFxJudgeType judgeType)
        {
            string objName = $"clickRaw_{GlobalSetting.HitFxType}_{judgeType}";
            //结果对象
            EffectManager result = null;
            //判断是否有该名字的对象池
            if (pool.ContainsKey(objName))
            {
                //对象池里有对象
                if (objectsInUse[objName].Count > MaxEffectCount)
                {
                    objectsInUse[objName][^1].ForceRecycle();
                }

                if (pool[objName].Count > 0)
                {
                    //获取结果
                    result = pool[objName][0];
                    //激活动画
                    result.animator.enabled = true;
                    result.sr.enabled = true;
                    //从池中移除该对象
                    pool[objName].Remove(result);
                    objectsInUse[objName].Add(result);
                    result.Enable();
                    //返回结果
                    return result;
                }
            }
            //如果没有该名字的对象池或者该名字对象池没有对象

            EffectManager prefab = null;
            //如果已经加载过该预设体
            if (prefabs.ContainsKey(objName))
            {
                prefab = prefabs[objName];
            }
            else //如果没有加载过该预设体
            {
                //加载预设体
                prefab = Resources.Load<EffectManager>("HitFX/" + objName);
                //更新字典
                prefabs.Add(objName, prefab);
                objectsInUse.Add(objName, new List<EffectManager>());
            }

            //生成
            result = Object.Instantiate(prefab);
            //改名（去除 Clone）
            result.name = objName;
            result.Enable();
            //返回
            return result;
        }

        /// <summary>
        /// 回收对象到对象池
        /// </summary>
        public void RecycleObj(EffectManager obj)
        {
            //设置动画为非激活
            obj.animator.enabled = false;
            obj.transform.position = new Vector3(1000, 1000, 0);
            obj.sr.enabled = false;
            //判断是否有该对象的对象池
            if (pool.ContainsKey(obj.name))
            {
                //放置到该对象池
                pool[obj.name].Add(obj);
                objectsInUse[obj.name].Remove(obj);
            }
            else
            {
                //创建该类型的池子，并将对象放入
                pool.Add(obj.name, new List<EffectManager>() {obj});
            }
        }

        public void Reset()
        {
            pool.Clear();
            objectsInUse.Clear();
            prefabs.Clear();
        }
    }

    public enum HitFxJudgeType
    {
        Perfect = 0,
        Good = 1
    }

    public enum HitFxType
    {
        Official = 0,
        Phira = 1,
        Sacabam = 2
    }
}