using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SplendorUnity.Core;
using SplendorUnity.Models;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 顶部信息栏 - 显示当前游戏状态信息
    /// 包括：当前玩家、回合数、玩家分数
    /// </summary>
    public class TopInfoBar : MonoBehaviour
    {
        [Header("UI组件")]
        public TextMeshProUGUI currentPlayerText;      // 当前玩家文本
        public TextMeshProUGUI turnNumberText;         // 回合数文本
        public TextMeshProUGUI player1ScoreText;       // 玩家1分数
        public TextMeshProUGUI player2ScoreText;       // 玩家2分数
        public TextMeshProUGUI gameStatusText;         // 游戏状态文本
        public TextMeshProUGUI countdownText;          // 倒计时文本
        
        [Header("Display Settings")]
        public Color currentPlayerColor = Color.yellow; // Current player highlight color
        public Color normalPlayerColor = Color.white;   // Normal player color
        public string player1Name = "Player 1";         // Player 1 name
        public string player2Name = "Player 2";         // Player 2 name
        
        [Header("Auto Find Settings")]
        public bool autoFindTextComponents = true;      // Auto find text components
        
        [Header("倒计时设置")]
        public Color normalTimeColor = Color.white;     // 正常时间颜色
        public Color warningTimeColor = Color.yellow;   // 警告时间颜色（剩余时间少）
        public Color dangerTimeColor = Color.red;       // 危险时间颜色（时间即将用完）
        public float warningThreshold = 10.0f;          // 警告阈值（秒）
        public float dangerThreshold = 5.0f;            // 危险阈值（秒）
        
        [Header("自动更新设置")]
        public bool enableAutoUpdate = true;            // 启用自动更新
        public float updateInterval = 0.5f;             // 更新间隔（秒）
        
        private GameStateListener gameStateListener;    // Game state listener reference
        private GameManager gameManager;                // 游戏管理器引用
        private Coroutine countdownCoroutine;           // 倒计时协程
        private Coroutine autoUpdateCoroutine;          // 自动更新协程
        private float currentTimeLimit;                 // 当前回合时间限制
        private float remainingTime;                    // 剩余时间
        private SplendorGameState lastKnownGameState;   // 上次已知的游戏状态
        
        private void Start()
        {
            // Auto find text components
            if (autoFindTextComponents)
            {
                FindTextComponents();
            }
            
            // 延迟查找GameStateListener，确保它已经被创建
            StartCoroutine(ConnectToGameStateListenerAfterFrame());
            
            // 查找GameManager
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("TopInfoBar: Connected to GameManager");
            }
            else
            {
                Debug.LogWarning("TopInfoBar: GameManager not found");
            }
            
            // Initialize display
            UpdateDisplay();
            
            // 设置初始回合数为1
            if (turnNumberText != null)
            {
                turnNumberText.text = "Turn: 1";
                Debug.Log("TopInfoBar: 设置初始回合数为1");
            }
            
            // 启动自动更新
            if (enableAutoUpdate)
            {
                StartAutoUpdate();
            }
        }
        

        
        /// <summary>
        /// 延迟连接到GameStateListener
        /// </summary>
        private System.Collections.IEnumerator ConnectToGameStateListenerAfterFrame()
        {
            yield return null; // 等待一帧
            
            // 查找GameStateListener
            gameStateListener = FindObjectOfType<GameStateListener>();
            if (gameStateListener != null)
            {
                gameStateListener.SetTopInfoBar(this);
                Debug.Log("TopInfoBar: Connected to GameStateListener");
            }
            else
            {
                Debug.LogWarning("TopInfoBar: GameStateListener not found");
            }
        }
        
        /// <summary>
        /// 启动自动更新
        /// </summary>
        private void StartAutoUpdate()
        {
            if (autoUpdateCoroutine != null)
            {
                StopCoroutine(autoUpdateCoroutine);
            }
            
            autoUpdateCoroutine = StartCoroutine(AutoUpdateCoroutine());
            Debug.Log("TopInfoBar: 自动更新已启动");
        }
        
        /// <summary>
        /// 停止自动更新
        /// </summary>
        public void StopAutoUpdate()
        {
            if (autoUpdateCoroutine != null)
            {
                StopCoroutine(autoUpdateCoroutine);
                autoUpdateCoroutine = null;
                Debug.Log("TopInfoBar: 自动更新已停止");
            }
        }
        
        /// <summary>
        /// 自动更新协程
        /// </summary>
        private System.Collections.IEnumerator AutoUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);
                
                // 检查游戏状态是否有变化
                CheckAndUpdateIfChanged();
            }
        }
        
        /// <summary>
        /// 检查游戏状态是否有变化，如果有变化则更新
        /// </summary>
        private void CheckAndUpdateIfChanged()
        {
            // 尝试获取最新的游戏状态
            SplendorGameState currentGameState = GetCurrentGameState();
            
            if (currentGameState == null) return;
            
            // 检查是否有变化
            bool hasChanged = HasGameStateChanged(currentGameState);
            
            // 检查回合数是否有变化
            int currentTurnNumber = GetCurrentTurnNumber();
            if (lastKnownGameState == null || currentTurnNumber != GetLastKnownTurnNumber())
            {
                hasChanged = true;
            }
            
            if (hasChanged)
            {
                Debug.Log($"TopInfoBar: 检测到游戏状态变化，自动更新显示，当前回合: {currentTurnNumber}");
                UpdateGameInfoFromGameState(currentGameState);
                lastKnownGameState = currentGameState;
            }
        }
        
        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        private SplendorGameState GetCurrentGameState()
        {
            // 优先从GameManager获取
            if (gameManager != null && gameManager.gameRule != null)
            {
                return gameManager.gameRule.currentGameState;
            }
            
            // 从GameStateListener获取
            if (gameStateListener != null)
            {
                return gameStateListener.gameState;
            }
            
            // 直接查找
            return FindObjectOfType<SplendorGameState>();
        }
        
        /// <summary>
        /// 检查游戏状态是否有变化
        /// </summary>
        private bool HasGameStateChanged(SplendorGameState currentState)
        {
            if (lastKnownGameState == null) return true;
            
            // 检查当前玩家是否有变化
            if (lastKnownGameState.agentToMove != currentState.agentToMove) return true;
            
            // 检查玩家分数是否有变化
            for (int i = 0; i < Mathf.Min(lastKnownGameState.agents.Count, currentState.agents.Count); i++)
            {
                if (lastKnownGameState.agents[i].score != currentState.agents[i].score) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Auto find text components
        /// </summary>
        private void FindTextComponents()
        {
            if (currentPlayerText == null)
                currentPlayerText = transform.Find("CurrentPlayerText")?.GetComponent<TextMeshProUGUI>();
                
            if (turnNumberText == null)
                turnNumberText = transform.Find("TurnNumberText")?.GetComponent<TextMeshProUGUI>();
                
            if (player1ScoreText == null)
                player1ScoreText = transform.Find("Player1ScoreText")?.GetComponent<TextMeshProUGUI>();
                
            if (player2ScoreText == null)
                player2ScoreText = transform.Find("Player2ScoreText")?.GetComponent<TextMeshProUGUI>();
                
            if (gameStatusText == null)
                gameStatusText = transform.Find("GameStatusText")?.GetComponent<TextMeshProUGUI>();
                
            if (countdownText == null)
                countdownText = transform.Find("CountdownText")?.GetComponent<TextMeshProUGUI>();
        }
        
        /// <summary>
        /// 从游戏状态更新游戏信息显示
        /// </summary>
        private void UpdateGameInfoFromGameState(SplendorGameState gameState)
        {
            if (gameState == null || gameState.agents == null) return;
            
            int currentPlayerId = gameState.agentToMove;
            int turnNumber = GetCurrentTurnNumber();
            
            UpdateGameInfo(currentPlayerId, turnNumber, gameState.agents);
        }
        
        /// <summary>
        /// 获取上次已知的回合数
        /// </summary>
        private int GetLastKnownTurnNumber()
        {
            if (lastKnownGameState == null) return 0;
            
            // 尝试从GameStateListener获取
            if (gameStateListener != null)
            {
                return gameStateListener.GetCurrentTurnNumber();
            }
            
            return 1; // 默认值
        }
        
        /// <summary>
        /// 获取当前回合数
        /// </summary>
        private int GetCurrentTurnNumber()
        {
            // 尝试从GameStateListener获取
            if (gameStateListener != null)
            {
                int turnNumber = gameStateListener.GetCurrentTurnNumber();
                if (turnNumber > 0)
                {
                    return turnNumber;
                }
            }
            
            // 尝试从GameManager获取
            if (gameManager != null)
            {
                // 通过actionCounter判断游戏是否已经开始
                if (gameManager.actionCounter > 0)
                {
                    return gameManager.actionCounter + 1;
                }
                else
                {
                    return 1; // 游戏开始前显示Turn 1
                }
            }
            
            return 1; // 默认值
        }
        
        /// <summary>
        /// Update game information display
        /// </summary>
        /// <param name="currentPlayer">Current player index</param>
        /// <param name="turnNumber">Current turn number</param>
        /// <param name="agents">Player state list</param>
        public void UpdateGameInfo(int currentPlayer, int turnNumber, List<SplendorGameState.AgentState> agents)
        {
            // Update current player display
            if (currentPlayerText != null)
            {
                string playerName = currentPlayer == 0 ? player1Name : player2Name;
                currentPlayerText.text = $"Current Player: {playerName}";
                
                // Set color
                currentPlayerText.color = currentPlayerColor;
            }
            
            // Update turn number display
            if (turnNumberText != null)
            {
                turnNumberText.text = $"Turn: {turnNumber}";
            }
            
            // Update player score display
            if (agents != null && agents.Count >= 2)
            {
                if (player1ScoreText != null)
                {
                    player1ScoreText.text = $"{player1Name}: {agents[0].score} pts";
                    // Highlight if current player
                    player1ScoreText.color = currentPlayer == 0 ? currentPlayerColor : normalPlayerColor;
                }
                
                if (player2ScoreText != null)
                {
                    player2ScoreText.text = $"{player2Name}: {agents[1].score} pts";
                    // Highlight if current player
                    player2ScoreText.text = $"{player2Name}: {agents[1].score} pts";
                    // Highlight if current player
                    player2ScoreText.color = currentPlayer == 1 ? currentPlayerColor : normalPlayerColor;
                }
            }
            
            // Update game status
            if (gameStatusText != null)
            {
                if (agents != null && agents.Count >= 2)
                {
                    // Check if any player reached victory condition (15 points)
                    bool gameEnded = agents[0].score >= 15 || agents[1].score >= 15;
                    if (gameEnded)
                    {
                        int winner = agents[0].score >= 15 ? 0 : 1;
                        string winnerName = winner == 0 ? player1Name : player2Name;
                        gameStatusText.text = $"Game Over! {winnerName} Wins!";
                        gameStatusText.color = Color.green;
                    }
                    else
                    {
                        gameStatusText.text = "Game in Progress...";
                        gameStatusText.color = Color.white;
                    }
                }
                else
                {
                    gameStatusText.text = "Waiting for Game Start...";
                    gameStatusText.color = Color.gray;
                }
            }
            
            // 启动倒计时
            StartCountdown();
            
            Debug.Log($"TopInfoBar: Display updated - Player {currentPlayer}, Turn {turnNumber}");
        }
        
        /// <summary>
        /// 启动倒计时
        /// </summary>
        private void StartCountdown()
        {
            if (gameManager == null) return;
            
            // 停止之前的倒计时
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            
            // 获取当前回合的时间限制
            currentTimeLimit = (float)gameManager.timeLimit;
            remainingTime = currentTimeLimit;
            
            // 启动新的倒计时
            countdownCoroutine = StartCoroutine(CountdownCoroutine());
            
            Debug.Log($"TopInfoBar: 启动倒计时，时间限制: {currentTimeLimit}秒");
        }
        
        /// <summary>
        /// 倒计时协程
        /// </summary>
        private System.Collections.IEnumerator CountdownCoroutine()
        {
            while (remainingTime > 0)
            {
                // 更新倒计时显示
                UpdateCountdownDisplay();
                
                // 等待一帧
                yield return null;
                
                // 减少剩余时间
                remainingTime -= Time.deltaTime;
            }
            
            // 时间用完
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                UpdateCountdownDisplay();
                
                // 触发时间到事件
                OnTimeUp();
            }
        }
        
        /// <summary>
        /// 更新倒计时显示
        /// </summary>
        private void UpdateCountdownDisplay()
        {
            if (countdownText != null)
            {
                // 格式化时间显示 - 对于小于1分钟的时间，只显示秒数
                if (remainingTime < 60)
                {
                    countdownText.text = $"Time: {remainingTime:F1}s";
                }
                else
                {
                    int minutes = Mathf.FloorToInt(remainingTime / 60);
                    int seconds = Mathf.FloorToInt(remainingTime % 60);
                    countdownText.text = $"Time: {minutes}:{seconds:00}";
                }
                
                // 根据剩余时间设置颜色
                if (remainingTime <= dangerThreshold)
                {
                    countdownText.color = dangerTimeColor;
                }
                else if (remainingTime <= warningThreshold)
                {
                    countdownText.color = warningTimeColor;
                }
                else
                {
                    countdownText.color = normalTimeColor;
                }
            }
        }
        
        /// <summary>
        /// 时间到事件处理
        /// </summary>
        private void OnTimeUp()
        {
            Debug.Log("TopInfoBar: 回合时间到！");
            
            // 这里可以添加时间到时的处理逻辑
            // 比如自动结束当前玩家的回合，或者显示警告等
            
            if (countdownText != null)
            {
                countdownText.text = "TIME UP!";
                countdownText.color = dangerTimeColor;
            }
        }
        
        /// <summary>
        /// 停止倒计时
        /// </summary>
        public void StopCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
                Debug.Log("TopInfoBar: 倒计时已停止");
            }
        }
        
        /// <summary>
        /// 重置倒计时
        /// </summary>
        public void ResetCountdown()
        {
            StopCountdown();
            remainingTime = currentTimeLimit;
            UpdateCountdownDisplay();
            Debug.Log("TopInfoBar: 倒计时已重置");
        }
        
        /// <summary>
        /// Update display (for manual refresh)
        /// </summary>
        public void UpdateDisplay()
        {
            if (gameStateListener != null)
            {
                var gameManager = gameStateListener.GetComponent<GameManager>();
                if (gameManager != null && gameManager.gameRule != null)
                {
                    var gameState = gameManager.gameRule.currentGameState;
                    if (gameState != null)
                    {
                        UpdateGameInfo(
                            gameState.agentToMove,
                            gameStateListener.GetCurrentTurnNumber(),
                            gameState.agents
                        );
                    }
                }
            }
        }
        
        /// <summary>
        /// Set player names
        /// </summary>
        public void SetPlayerNames(string player1, string player2)
        {
            player1Name = player1;
            player2Name = player2;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Set color theme
        /// </summary>
        public void SetColorTheme(Color currentPlayer, Color normal)
        {
            currentPlayerColor = currentPlayer;
            normalPlayerColor = normal;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Manual refresh display
        /// </summary>
        [ContextMenu("Manual Refresh Display")]
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void OnDestroy()
        {
            // 停止倒计时协程
            StopCountdown();
        }
        
        /// <summary>
        /// Get info bar status
        /// </summary>
        [ContextMenu("Get Info Bar Status")]
        public string GetInfoBarStatus()
        {
            string status = $"=== TopInfoBar Status ===\n";
            status += $"Current Player Text: {(currentPlayerText != null ? "Found" : "Not Found")}\n";
            status += $"Turn Number Text: {(turnNumberText != null ? "Found" : "Not Found")}\n";
            status += $"Player 1 Score Text: {(player1ScoreText != null ? "Found" : "Not Found")}\n";
            status += $"Player 2 Score Text: {(player2ScoreText != null ? "Found" : "Not Found")}\n";
            status += $"Game Status Text: {(gameStatusText != null ? "Found" : "Not Found")}\n";
            status += $"Countdown Text: {(countdownText != null ? "Found" : "Not Found")}\n";
            status += $"GameStateListener: {(gameStateListener != null ? "Connected" : "Not Connected")}\n";
            status += $"GameManager: {(gameManager != null ? "Connected" : "Not Connected")}\n";
            status += $"Player 1 Name: {player1Name}\n";
            status += $"Player 2 Name: {player2Name}\n";
            status += $"Current Time Limit: {currentTimeLimit}s\n";
            status += $"Remaining Time: {remainingTime:F1}s\n";
            status += $"Countdown Active: {(countdownCoroutine != null ? "Yes" : "No")}";
            
            return status;
        }
    }
}
