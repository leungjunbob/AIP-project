using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.AI
{
    /// <summary>
    /// 首个动作AI代理，替代原FirstMoveAgent
    /// </summary>
    public class FirstMoveAgent : BaseAgent
    {
        [Header("首个动作AI配置")]
        public bool preferFirstAction = true;
        
        protected override void Awake()
        {
            base.Awake();
            agentName = "First Move Agent";
        }
        
        /// <summary>
        /// 选择第一个可用动作
        /// </summary>
        public override object SelectAction(List<object> actions, object gameState)
        {
            if (actions == null || actions.Count == 0)
            {
                return null;
            }
            
            // 总是选择第一个动作
            return actions[0];
        }
        
        /// <summary>
        /// 设置是否优先选择第一个动作
        /// </summary>
        public void SetPreferFirstAction(bool prefer)
        {
            preferFirstAction = prefer;
        }
    }
}