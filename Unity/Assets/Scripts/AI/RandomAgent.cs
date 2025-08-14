using UnityEngine;
using System;
using System.Collections.Generic;

namespace SplendorUnity.AI
{
    /// <summary>
    /// 随机AI代理，替代原RandomAgent
    /// </summary>
    public class RandomAgent : BaseAgent
    {
        [Header("随机AI配置")]
        public bool useSeed = false;
        public int randomSeed = 42;
        
        private System.Random random;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeRandom();
        }
        
        /// <summary>
        /// 初始化随机数生成器
        /// </summary>
        private void InitializeRandom()
        {
            if (useSeed)
            {
                random = new System.Random(randomSeed);
            }
            else
            {
                random = new System.Random();
            }
        }
        
        /// <summary>
        /// 随机选择一个动作
        /// </summary>
        public override object SelectAction(List<object> actions, object gameState)
        {
            if (actions == null || actions.Count == 0)
            {
                return null;
            }
            
            int randomIndex = random.Next(actions.Count);
            return actions[randomIndex];
        }
        
        /// <summary>
        /// 设置随机种子
        /// </summary>
        public void SetSeed(int seed)
        {
            randomSeed = seed;
            random = new System.Random(seed);
        }
    }
}