using UnityEngine;
using SplendorUnity.Display;

namespace SplendorUnity.Core
{
    /// <summary>
    /// 场景启动管理器 - 简化版本
    /// 只负责确保必要的组件存在，不自动启动游戏
    /// </summary>
    public class SceneStartupManager : MonoBehaviour
    {
        [Header("组件引用")]
        public GameManager gameManager;
        public SplendorGameRule gameRule;
        public SplendorGameState gameState;
        public GemPileManager gemPileManager;
        
        [Header("配置")]
        public bool enableDebugLog = true;
        public bool ensureComponentsOnStart = true;
        
        private void Start()
        {
            if (ensureComponentsOnStart)
            {
                EnsureRequiredComponents();
            }
        }
        
        /// <summary>
        /// 确保必要的组件存在
        /// </summary>
        public void EnsureRequiredComponents()
        {
            
            // 确保有GameManager
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
            
            // 确保有SplendorGameRule
            if (gameRule == null)
            {
                if (gameManager != null)
                {
                    gameRule = gameManager.GetComponent<SplendorGameRule>();
                    if (gameRule == null)
                    {
                        gameRule = gameManager.gameObject.AddComponent<SplendorGameRule>();
                    }
                }
                else
                {
                    gameRule = FindObjectOfType<SplendorGameRule>();
                }
            }
            
            // 确保有SplendorGameState
            if (gameState == null)
            {
                gameState = FindObjectOfType<SplendorGameState>();
            }
            
            // 确保有GemPileManager
            if (gemPileManager == null)
            {
                gemPileManager = FindObjectOfType<GemPileManager>();
            }
        }
    }
}
