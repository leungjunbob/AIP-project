using UnityEngine;
using UnityEngine.UI;
using SplendorUnity.Core;
using SplendorUnity.AI;
using SplendorUnity.Display;

namespace SplendorUnity.Core
{
    /// <summary>
    /// 游戏启动器，管理游戏启动、配置和测试
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        [Header("游戏配置")]
        public bool autoStartOnPlay = true; // 点击Play时自动开始
        public int agentCount = 2; // 玩家数量
        public int warningLimit = 3; // 警告限制
        
        [Header("组件引用")]
        public GameManager gameManager;
        public GemPileManager gemPileManager;
        
        [Header("AI代理")]
        public BaseAgent aiAgent; // AI代理（对手）
        public BaseAgent humanAgent; // 人类代理（玩家）
        
        [Header("UI控制")]
        public Button startGameButton;
        public Button stopGameButton;
        public Button testGemPilesButton;
        public Text gameStatusText;
        
        [Header("调试设置")]
        public bool enableDebugLog = true;
        
        private bool gameRunning = false;
        
        private void Awake()
        {
            // 自动查找组件
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            if (gemPileManager == null)
                gemPileManager = FindObjectOfType<GemPileManager>();
            
            // 设置按钮事件
            SetupButtons();
            
            // 确保有必要的组件
            EnsureRequiredComponents();
        }
        
        /// <summary>
        /// 确保必要的组件存在
        /// </summary>
        private void EnsureRequiredComponents()
        {
            // 只创建最基本的GameManager，其他组件等待Start Game按钮点击后创建
            if (gameManager == null)
            {
                gameManager = gameObject.AddComponent<GameManager>();
            }
        }
        
        /// <summary>
        /// 确保GameStartPage存在
        /// </summary>
        private void EnsureGameStartPageExists()
        {
            // 查找场景中是否已有GameStartPage
            var existingGameStartPage = FindObjectOfType<UI.GameStartPage>();
            if (existingGameStartPage == null)
            {
                // 如果没有找到，创建一个新的
                var gameStartPageGO = new GameObject("GameStartPage");
                var gameStartPage = gameStartPageGO.AddComponent<UI.GameStartPage>();
                
                if (enableDebugLog)
                    Debug.Log("GameLauncher: 创建了新的GameStartPage");
                
                // 显示页面
                if (gameManager != null)
                {
                    gameManager.ShowGameStartPage();
                }
            }
            else
            {
                if (enableDebugLog)
                    Debug.Log("GameLauncher: 找到已存在的GameStartPage，直接显示");
                
                // 显示页面
                if (gameManager != null)
                {
                    gameManager.ShowGameStartPage();
                }
            }
        }
        
        private void Start()
        {
            // 检查是否已经有多个GameStartPage实例
            var allGameStartPages = FindObjectsOfType<UI.GameStartPage>();
            if (allGameStartPages.Length > 1)
            {
                Debug.LogWarning($"GameLauncher: 发现 {allGameStartPages.Length} 个GameStartPage实例，这可能导致重复初始化");
                // 保留第一个，删除其他的
                for (int i = 1; i < allGameStartPages.Length; i++)
                {
                    if (allGameStartPages[i] != null)
                    {
                        Debug.Log($"GameLauncher: 删除重复的GameStartPage实例 {i}");
                        DestroyImmediate(allGameStartPages[i].gameObject);
                    }
                }
            }
            
            // 确保GameStartPage存在
            EnsureGameStartPageExists();            
            UpdateUI();
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtons()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(StartNewGame);
            
            if (stopGameButton != null)
                stopGameButton.onClick.AddListener(StopGame);
            
            if (testGemPilesButton != null)
                testGemPilesButton.onClick.AddListener(TestGemPiles);
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            
            // 设置游戏运行状态
            gameRunning = true;
            
            // 不再自动设置AI代理，等待GameStartPage设置
            
            // 开始游戏
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
            
            // 更新UI
            UpdateUI();
        }
        
        /// <summary>
        /// 停止游戏
        /// </summary>
        public void StopGame()
        {            
            // 设置游戏停止状态
            gameRunning = false;
            
            // 这里可以添加停止游戏的逻辑
            // 例如：停止GameManager的协程等
            
            // 更新UI
            UpdateUI();
        }
        
        /// <summary>
        /// 测试宝石堆
        /// </summary>
        public void TestGemPiles()
        {            
            // 更新宝石堆
            UpdateGemPilesAfterGameStart();
        }
        

        
        /// <summary>
        /// 游戏开始后更新宝石堆
        /// </summary>
        private void UpdateGemPilesAfterGameStart()
        {
            if (gemPileManager != null && gameManager != null && gameManager.gameRule != null)
            {
                var gameState = gameManager.gameRule.CurrentGameState as SplendorGameState;
                if (gameState != null)
                {
                    gemPileManager.UpdateGemPiles(gameState);
                }
            }
        }
        
        /// <summary>
        /// 更新UI状态
        /// </summary>
        private void UpdateUI()
        {
            if (startGameButton != null)
                startGameButton.interactable = !gameRunning;
            
            if (stopGameButton != null)
                stopGameButton.interactable = gameRunning;
            
            if (gameStatusText != null)
                gameStatusText.text = gameRunning ? "游戏运行中..." : "游戏已停止";
        }
    }
}