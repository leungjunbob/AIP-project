using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SplendorUnity.Core;
using SplendorUnity.Models;

namespace SplendorUnity.UI
{
    public class EndGameCanvas : MonoBehaviour
    {
        [Header("UI References")]
        public Canvas endGameCanvas;
        public TextMeshProUGUI winnerText;
        public Button restartButton;
        public Button backToMenuButton;
        
        [Header("Settings")]
        public bool enableDebugLog = true;
        
        private GameManager gameManager;
        private GameStartPage gameStartPage;
        private SplendorGameState gameState;
        
        private void Awake()
        {
            // 初始时隐藏EndGameCanvas
            if (endGameCanvas != null)
            {
                endGameCanvas.gameObject.SetActive(false);
            }
            
            // 查找必要的组件
            gameManager = FindObjectOfType<GameManager>();
            gameStartPage = FindObjectOfType<GameStartPage>();
            
            if (enableDebugLog)
                Debug.Log("EndGameCanvas: 初始化完成");
        }
        
        private void Start()
        {
            // 设置按钮事件
            SetupButtonEvents();
            
            // 监听游戏状态变化
            if (gameManager != null)
            {
                // 连接到GameStateListener以监听游戏状态变化
                var gameStateListener = FindObjectOfType<GameStateListener>();
                if (gameStateListener != null)
                {
                    // 这里可以添加游戏结束的监听逻辑
                    if (enableDebugLog)
                        Debug.Log("EndGameCanvas: 已连接到GameManager和GameStateListener");
                }
            }
        }
        
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (backToMenuButton != null)
            {
                backToMenuButton.onClick.RemoveAllListeners();
                backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            }
        }
        
        /// <summary>
        /// 显示游戏结束界面
        /// </summary>
        /// <param name="winnerPlayerId">获胜玩家ID</param>
        public void ShowEndGame(int winnerPlayerId)
        {
            if (endGameCanvas == null)
            {
                Debug.LogError("EndGameCanvas: endGameCanvas引用为空");
                return;
            }
            
            // 更新获胜者文本
            if (winnerText != null)
            {
                winnerText.text = $"Winner: Player {winnerPlayerId}";
            }
            
            // 显示EndGameCanvas
            endGameCanvas.gameObject.SetActive(true);
            
            if (enableDebugLog)
                Debug.Log($"EndGameCanvas: 显示游戏结束界面，获胜者: Player {winnerPlayerId}");
        }
        
        /// <summary>
        /// 隐藏游戏结束界面
        /// </summary>
        public void HideEndGame()
        {
            if (endGameCanvas != null)
            {
                endGameCanvas.gameObject.SetActive(false);
            }
            
            if (enableDebugLog)
                Debug.Log("EndGameCanvas: 隐藏游戏结束界面");
        }
        
        /// <summary>
        /// 重新开始游戏按钮点击事件
        /// </summary>
        private void OnRestartClicked()
        {
            if (enableDebugLog)
                Debug.Log("EndGameCanvas: 重新开始游戏按钮被点击");
            
            // 隐藏EndGameCanvas
            HideEndGame();
            
            // 重新开始游戏
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }
        
        /// <summary>
        /// 返回主菜单按钮点击事件
        /// </summary>
        private void OnBackToMenuClicked()
        {
            if (enableDebugLog)
                Debug.Log("EndGameCanvas: 返回主菜单按钮被点击");
            
            // 隐藏EndGameCanvas
            HideEndGame();
            
            // 显示GameStartPage并重置游戏状态
            if (gameStartPage != null)
            {
                // 通过反射调用GameStartPage的ResetGameState方法
                var resetGameStateMethod = typeof(GameStartPage).GetMethod("ResetGameState", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (resetGameStateMethod != null)
                {
                    resetGameStateMethod.Invoke(gameStartPage, null);
                    if (enableDebugLog)
                        Debug.Log("EndGameCanvas: 已调用GameStartPage.ResetGameState()");
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("EndGameCanvas: 无法找到GameStartPage.ResetGameState方法");
                }
                
                gameStartPage.ShowPage();
            }
            else
            {
                Debug.LogWarning("EndGameCanvas: 找不到GameStartPage");
            }
        }
        
        /// <summary>
        /// 测试方法：给Player2加15分让他获胜
        /// </summary>
        [ContextMenu("Test: Give Player2 +15 Points")]
        public void TestGivePlayer2Victory()
        {
            if (gameManager == null || gameManager.gameRule == null)
            {
                Debug.LogWarning("EndGameCanvas: GameManager或GameRule为空，无法测试");
                return;
            }
            
            // 获取当前游戏状态
            gameState = gameManager.gameRule.currentGameState;
            if (gameState == null || gameState.agents == null || gameState.agents.Count < 2)
            {
                Debug.LogWarning("EndGameCanvas: 游戏状态无效，无法测试");
                return;
            }
            
            // 给Player2加15分
            var player2State = gameState.agents[1];
            if (player2State != null)
            {
                int currentScore = player2State.score;
                int newScore = currentScore + 15;
                player2State.score = newScore;
                
                if (enableDebugLog)
                    Debug.Log($"EndGameCanvas: 测试成功！Player2分数从{currentScore}增加到{newScore}");
                
                // 检查是否达到胜利条件
                CheckGameEnd();
            }
        }
        
        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        private void CheckGameEnd()
        {
            if (gameState == null || gameState.agents == null)
                return;
            
            // 检查是否有玩家达到15分
            for (int i = 0; i < gameState.agents.Count; i++)
            {
                var agentState = gameState.agents[i];
                if (agentState != null)
                {
                    int score = agentState.score;
                    if (score >= 15)
                    {
                        if (enableDebugLog)
                            Debug.Log($"EndGameCanvas: Player {i + 1}达到15分，游戏结束！");
                        
                        // 显示游戏结束界面
                        ShowEndGame(i + 1);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// 监听游戏状态变化（由GameManager调用）
        /// </summary>
        public void OnGameStateChanged(SplendorGameState newGameState)
        {
            gameState = newGameState;
            
            // 检查游戏是否结束
            CheckGameEnd();
        }
    }
}
