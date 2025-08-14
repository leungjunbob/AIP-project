using UnityEngine;
using SplendorUnity.Core;
using SplendorUnity.AI;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 游戏控制管理器 - 连接UI和游戏逻辑
    /// </summary>
    public class GameControlManager : MonoBehaviour
    {
        [Header("UI Components")]
        public GemCollectionPanel gemCollectionPanel;
        
        [Header("Game Components")]
        public HumanAgent humanAgent;
        public GameManager gameManager;
        
        [Header("Settings")]
        public bool enableDebugLog = true;
        
        private void Start()
        {
            // 查找必要组件
            FindRequiredComponents();
            
            // 设置事件监听
            SetupEventListeners();
        }
        
        /// <summary>
        /// 查找必要组件
        /// </summary>
        private void FindRequiredComponents()
        {
            if (gemCollectionPanel == null)
                gemCollectionPanel = FindObjectOfType<GemCollectionPanel>();
            
            if (humanAgent == null)
                humanAgent = FindObjectOfType<HumanAgent>();
            
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
        }
        
        /// <summary>
        /// 设置事件监听
        /// </summary>
        private void SetupEventListeners()
        {
            if (gameManager != null)
            {
                // 监听游戏状态变化
                GameManager.OnActionExecuted += OnActionExecuted;
                GameManager.OnGameStarted += OnGameStarted;
                GameManager.OnGameEnded += OnGameEnded;
                
            }
        }
        
        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted(GameManager gameManager)
        {
        }
        
        /// <summary>
        /// 游戏结束事件
        /// </summary>
        private void OnGameEnded(GameManager gameManager)
        {
        }
        
        /// <summary>
        /// 动作执行事件
        /// </summary>
        private void OnActionExecuted(int agentIndex, object action)
        {
            
            // 检查是否是玩家的回合
            CheckPlayerTurn();
        }
        
        /// <summary>
        /// 检查玩家回合
        /// </summary>
        private void CheckPlayerTurn()
        {
            if (humanAgent == null || gameManager == null) return;
            
            // 检查当前是否是玩家的回合
            if (humanAgent.IsPlayerTurn())
            {
                
                // 显示玩家动作选项
                ShowPlayerActionOptions();
            }
        }
        
        /// <summary>
        /// 显示玩家动作选项
        /// </summary>
        private void ShowPlayerActionOptions()
        {
            if (gemCollectionPanel == null) return;
            
            // 获取当前游戏状态
            var gameState = humanAgent.GetCurrentGameState();
            if (gameState != null)
            {
                // 显示宝石收集面板
                gemCollectionPanel.ShowPanel(gameState, gameState.agents[0]);
                
            }
        }
        
        /// <summary>
        /// 手动显示宝石收集面板
        /// </summary>
        [ContextMenu("Show Gem Collection Panel")]
        public void ShowGemCollectionPanel()
        {
            if (gemCollectionPanel == null)
            {
                return;
            }
            
            if (humanAgent == null)
            {
                return;
            }
            
            var gameState = humanAgent.GetCurrentGameState();
            if (gameState != null)
            {
                gemCollectionPanel.ShowPanel(gameState, gameState.agents[0]);
            }
        }
        
        
        
        private void OnDestroy()
        {
            // 移除事件监听
            if (gameManager != null)
            {
                GameManager.OnActionExecuted -= OnActionExecuted;
                GameManager.OnGameStarted -= OnGameStarted;
                GameManager.OnGameEnded -= OnGameEnded;
            }
        }
    }
}
