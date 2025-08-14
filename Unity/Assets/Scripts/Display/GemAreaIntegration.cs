using UnityEngine;
using SplendorUnity.Core;
using SplendorUnity.Display;

namespace SplendorUnity.Display
{
    /// <summary>
    /// GemArea集成脚本，连接游戏状态更新和GemArea显示
    /// </summary>
    public class GemAreaIntegration : MonoBehaviour
    {
        [Header("组件引用")]
        public GemAreaManager gemAreaManager;
        public GemAreaClickHandler gemAreaClickHandler;
        public SplendorGameState gameState;
        
        [Header("自动更新配置")]
        public bool enableAutoUpdate = true;
        public bool updateOnStart = true;
        
        
        private void Start()
        {
            InitializeIntegration();
        }
        
        /// <summary>
        /// 初始化集成
        /// </summary>
        private void InitializeIntegration()
        {
            // 自动查找组件
            if (gemAreaManager == null)
                gemAreaManager = FindObjectOfType<GemAreaManager>();
                
            if (gemAreaClickHandler == null)
                gemAreaClickHandler = FindObjectOfType<GemAreaClickHandler>();
                
            if (gameState == null)
                gameState = FindObjectOfType<SplendorGameState>();
            
            if (gemAreaManager == null)
            {
                return;
            }
            
            
            // 如果启用自动更新且游戏状态存在，立即更新一次
            if (updateOnStart && gameState != null)
            {
                UpdateGemArea();
            }
            
            // 初始化点击事件
            InitializeClickEvents();
        }
        
        /// <summary>
        /// 更新GemArea显示
        /// </summary>
        public void UpdateGemArea()
        {
            if (gemAreaManager == null)
            {
                return;
            }
            
            if (gameState == null)
            {
                return;
            }
            
            gemAreaManager.UpdateGemArea(gameState);
        }
        
        /// <summary>
        /// 初始化点击事件
        /// </summary>
        private void InitializeClickEvents()
        {
            if (gemAreaClickHandler == null)
            {
                return;
            }
            
            // 设置游戏状态到点击处理器
            if (gameState != null)
            {
                gemAreaClickHandler.SetGameState(gameState);
            }
            
            // 为所有gem添加点击事件
            gemAreaClickHandler.AddClickEventsToAllGems();
        }
        
        /// <summary>
        /// 设置游戏状态并更新显示
        /// </summary>
        /// <param name="newGameState">新的游戏状态</param>
        public void SetGameState(SplendorGameState newGameState)
        {
            gameState = newGameState;
            
            if (enableAutoUpdate)
            {
                UpdateGemArea();
            }
        }
        
        /// <summary>
        /// 模拟宝石收集（用于测试）
        /// </summary>
        /// <param name="gemType">宝石类型</param>
        /// <param name="amount">收集数量</param>
        [ContextMenu("测试宝石收集")]
        public void TestGemCollection(string gemType = "red", int amount = 1)
        {
            if (gameState?.board?.gems == null)
            {
                return;
            }
            
            if (gameState.board.gems.ContainsKey(gemType))
            {
                int currentAmount = gameState.board.gems[gemType];
                int newAmount = Mathf.Max(0, currentAmount - amount);
                gameState.board.gems[gemType] = newAmount;
                
                // 更新显示
                UpdateGemArea();
            }
        }
        
        /// <summary>
        /// 重置宝石堆到初始状态
        /// </summary>
        /// <param name="playerCount">玩家数量</param>
        [ContextMenu("重置宝石堆")]
        public void ResetGemPiles(int playerCount = 4)
        {
            if (gameState?.board?.gems == null)
            {
                return;
            }
            
            // 根据玩家数量设置初始宝石数量
            int n = playerCount == 2 ? 4 : playerCount == 3 ? 5 : 7;
            
            gameState.board.gems["black"] = n;
            gameState.board.gems["red"] = n;
            gameState.board.gems["yellow"] = 5; // 黄色宝石固定为5个
            gameState.board.gems["green"] = n;
            gameState.board.gems["blue"] = n;
            gameState.board.gems["white"] = n;
            
            
            // 更新显示
            UpdateGemArea();
        }
        
    }
}
