using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.AI
{
    /// <summary>
    /// AI代理基类，替代原Agent类
    /// </summary>
    public abstract class BaseAgent : MonoBehaviour
    {
        [Header("代理配置")]
        public int id;
        public string agentName = "AI Agent";
        public bool isFirstMove = false;
        
        /// <summary>
        /// 初始化代理
        /// </summary>
        protected virtual void Awake()
        {
            // 子类可以重写此方法进行初始化
        }
        
        /// <summary>
        /// 给定可用动作和当前游戏状态，选择一个动作
        /// </summary>
        /// <param name="actions">可用动作列表</param>
        /// <param name="gameState">当前游戏状态</param>
        /// <returns>选择的动作</returns>
        public abstract object SelectAction(List<object> actions, object gameState);
        
        /// <summary>
        /// 获取代理ID
        /// </summary>
        public int GetId()
        {
            return id;
        }
        
        /// <summary>
        /// 设置代理ID
        /// </summary>
        public void SetId(int newId)
        {
            id = newId;
        }
        
        /// <summary>
        /// 获取代理名称
        /// </summary>
        public string GetName()
        {
            return agentName;
        }
        
        /// <summary>
        /// 设置代理名称
        /// </summary>
        public void SetName(string name)
        {
            agentName = name;
        }
    }
}