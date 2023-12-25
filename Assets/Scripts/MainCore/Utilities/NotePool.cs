using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MainCore.Utilities
{
    public class NotePool
    {
        private const int MaxEffectCount = 1000;
        private static NotePool instance;

        private Dictionary<string, List<NoteMovement>> pool;

        private Dictionary<string, NoteMovement> prefabs;

        private NotePool()
        {
            pool = new Dictionary<string, List<NoteMovement>>();
            prefabs = new Dictionary<string, NoteMovement>();
        }

        public static NotePool GetInstance()
        {
            if (instance == null)
            {
                instance = new NotePool();
            }

            return instance;
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public NoteMovement GetObj(string objName)
        {
            //结果对象
            NoteMovement result = null;
            //判断是否有该名字的对象池
            if (pool.ContainsKey(objName))
            {
                if (pool[objName].Count > 0)
                {
                    //获取结果
                    result = pool[objName][0];
                    //激活动画
                    result.gameObject.SetActive(true);
                    //从池中移除该对象
                    pool[objName].Remove(result);
                    //返回结果
                    return result;
                }
            }
            //如果没有该名字的对象池或者该名字对象池没有对象

            NoteMovement prefab = null;
            //如果已经加载过该预设体
            if (prefabs.ContainsKey(objName))
            {
                prefab = prefabs[objName];
            }
            else //如果没有加载过该预设体
            {
                //加载预设体
                prefab = Resources.Load<NoteMovement>("Notes/" + objName);
                prefab.UpdateNoteSkin(GlobalSetting.CurrentSkinInfo, objName switch
                {
                    "Tap" => 0,
                    "Drag" => 1,
                    "Flick" => 2,
                    "Hold" => 3,
                    _ => throw new ArgumentException()
                });
                //更新字典
                prefabs.Add(objName, prefab);
            }

            //生成
            result = Object.Instantiate(prefab);
            //改名（去除 Clone）
            result.name = objName;
            result.gameObject.SetActive(true);
            //返回
            return result;
        }

        /// <summary>
        /// 回收对象到对象池
        /// </summary>
        public void RecycleObj(NoteMovement obj)
        {
            //设置动画为非激活
            obj.gameObject.SetActive(false);
            //判断是否有该对象的对象池
            if (pool.ContainsKey(obj.name))
            {
                //放置到该对象池
                pool[obj.name].Add(obj);
            }
            else
            {
                //创建该类型的池子，并将对象放入
                pool.Add(obj.name, new List<NoteMovement>() {obj});
            }
        }

        public void Reset()
        {
            pool.Clear();
            prefabs.Clear();
        }

        private void OverrideAnimator()
        {
            
        }
    }
}