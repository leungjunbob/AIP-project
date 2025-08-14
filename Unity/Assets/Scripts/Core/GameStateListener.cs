using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SplendorUnity.Display;
using SplendorUnity.Core;
using SplendorUnity.UI;

namespace SplendorUnity.Core
{
    /// <summary>
    /// GameState监听器
    /// 统一协调所有管理器对GameState变化的响应，确保UI同步更新
    /// 只使用事件驱动更新，不使用定时刷新
    /// </summary>
    public class GameStateListener : MonoBehaviour
    {
        [Header("管理器引用")]
        public CardManager cardManager;
        public GemPileManager gemPileManager;
        public NobleManager nobleManager;
        
        [Header("UI组件")]
        public TopInfoBar topInfoBar; // 顶部信息栏
        public List<PlayerInfoDisplay> playerInfoDisplays = new List<PlayerInfoDisplay>(); // 玩家信息显示列表
        
        [Header("监听设置")]
        public bool updateOnGameEvents = true;
        public bool enableDebugLog = true;
        
        [Header("组件引用")]
        public GameManager gameManager;
        public SplendorGameState gameState;
        [Header("EndGameCanvas引用")]
        public SplendorUnity.UI.EndGameCanvas endGameCanvas; // 手动设置的EndGameCanvas引用
        
        private bool isInitialized = false;
        private int currentTurnNumber = 0; // 当前回合数
        
        private void Start()
        {
               
            // 延迟一帧后开始初始化，确保其他组件已经初始化完成
            StartCoroutine(InitializeAfterFrame());
        }
        
        /// <summary>
        /// 延迟一帧后初始化
        /// </summary>
        private IEnumerator InitializeAfterFrame()
        {
            yield return null;
        
                
            // 自动查找必要的组件
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
            
            // 尝试从GameManager获取SplendorGameRule，然后获取SplendorGameState
            if (gameState == null && gameManager != null && gameManager.gameRule != null)
            {
                var gameRule = gameManager.gameRule;
                if (gameRule.currentGameState != null)
                {
                    gameState = gameRule.currentGameState;
                }
            }
            
            // 如果还是没找到，尝试直接查找
            if (gameState == null)
            {
                gameState = FindObjectOfType<SplendorGameState>();
            }
            
            // 自动查找管理器
            if (cardManager == null)
            {
                cardManager = FindObjectOfType<CardManager>();
            }
            
            if (gemPileManager == null)
            {
                gemPileManager = FindObjectOfType<GemPileManager>();
            }
            
            if (nobleManager == null)
            {
                nobleManager = FindObjectOfType<NobleManager>();
            }
            
            // 自动查找TopInfoBar
            if (topInfoBar == null)
            {
                topInfoBar = FindObjectOfType<TopInfoBar>();
            }
            
            // 自动查找PlayerInfoDisplay
            if (playerInfoDisplays.Count == 0)
            {
                var allPlayerDisplays = FindObjectsOfType<PlayerInfoDisplay>();
                playerInfoDisplays.AddRange(allPlayerDisplays);
            }
            
            // 自动查找EndGameCanvas - 包括被隐藏的
            if (endGameCanvas == null)
            {
                // 首先尝试查找激活的
                endGameCanvas = FindObjectOfType<SplendorUnity.UI.EndGameCanvas>();
                
                // 如果没找到，尝试查找所有MonoBehaviour（包括被隐藏的）
                if (endGameCanvas == null)
                {
                    var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>(true); // true表示包括被隐藏的
                    foreach (var mb in allMonoBehaviours)
                    {
                        if (mb.GetType().Name == "EndGameCanvas")
                        {
                            endGameCanvas = mb as SplendorUnity.UI.EndGameCanvas;
                            if (enableDebugLog)
                                Debug.Log("GameStateListener: 通过类型名称找到被隐藏的EndGameCanvas");
                            break;
                        }
                    }
                }
                
                if (endGameCanvas != null && enableDebugLog)
                    Debug.Log("GameStateListener: 已找到EndGameCanvas");
                else if (enableDebugLog)
                    Debug.LogWarning("GameStateListener: 未找到EndGameCanvas");
            }
            
            // 订阅游戏事件
            if (updateOnGameEvents)
            {
                SubscribeToGameEvents();
            }
            
            // 延迟一帧后标记为已初始化
            StartCoroutine(MarkAsInitializedAfterFrame());
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }
        
        /// <summary>
        /// 延迟一帧后标记为已初始化
        /// </summary>
        private IEnumerator MarkAsInitializedAfterFrame()
        {
            yield return null;
            isInitialized = true;
            
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToGameEvents()
        {
            if (gameManager != null)
            {
                GameManager.OnGameStarted += OnGameStarted;
                GameManager.OnActionExecuted += OnActionExecuted;
                GameManager.OnGameEnded += OnGameEnded;
                
                if (enableDebugLog)
                    Debug.Log("GameStateListener: 已订阅游戏事件");
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning("GameStateListener: gameManager为null，无法订阅游戏事件");
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            if (gameManager != null)
            {
                GameManager.OnGameStarted -= OnGameStarted;
                GameManager.OnActionExecuted -= OnActionExecuted;
                GameManager.OnGameEnded -= OnGameEnded;
            }
        }
        
        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted(GameManager manager)
        {
            if (enableDebugLog)
                Debug.Log("GameStateListener: 游戏开始事件被触发");
                
            currentTurnNumber = 1; // 游戏开始时显示Turn 1
                
            if (enableDebugLog)
                Debug.Log($"GameStateListener: 设置回合数为: {currentTurnNumber}");
                
            // 立即更新UI，不等待协程
            UpdateAllManagers();
                
            if (enableDebugLog)
                Debug.Log("GameStateListener: 已立即更新所有管理器");
        }
        
        /// <summary>
        /// 游戏动作执行事件
        /// </summary>
        private void OnActionExecuted(int agentIndex, object action)
        {
            if (enableDebugLog)
                Debug.Log($"GameStateListener: 动作执行事件被触发，Agent {agentIndex}，当前回合: {currentTurnNumber}");
                
            currentTurnNumber++; // 增加回合数
        
            if (enableDebugLog)
                Debug.Log($"GameStateListener: 回合数增加到: {currentTurnNumber}");
                
            StartCoroutine(UpdateAllManagersAfterFrame());
        }
        
        /// <summary>
        /// 游戏结束事件
        /// </summary>
        private void OnGameEnded(GameManager manager)
        {
            if (enableDebugLog)
                Debug.Log("GameStateListener: 游戏结束事件被触发");
                
            // 更新TopInfoBar显示最终状态
            UpdateTopInfoBar();
            
            // 通知EndGameCanvas显示游戏结束界面
            NotifyEndGameCanvas();
        }
        
        /// <summary>
        /// 通知EndGameCanvas显示游戏结束界面
        /// </summary>
        private void NotifyEndGameCanvas()
        {
            if (enableDebugLog)
                Debug.Log("GameStateListener: 尝试通知EndGameCanvas");
            
            // 首先使用缓存的引用
            if (endGameCanvas != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStateListener: 使用缓存的EndGameCanvas引用");
            }
            else
            {
                // 如果缓存为空，尝试重新查找
                if (enableDebugLog)
                    Debug.Log("GameStateListener: 缓存为空，尝试重新查找EndGameCanvas");
                
                endGameCanvas = FindObjectOfType<SplendorUnity.UI.EndGameCanvas>();
                if (endGameCanvas == null)
                {
                    // 尝试查找所有MonoBehaviour，然后检查类型
                    var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
                    foreach (var mb in allMonoBehaviours)
                    {
                        if (mb.GetType().Name == "EndGameCanvas")
                        {
                            endGameCanvas = mb as SplendorUnity.UI.EndGameCanvas;
                            if (enableDebugLog)
                                Debug.Log("GameStateListener: 通过类型名称找到EndGameCanvas");
                            break;
                        }
                    }
                }
                
                if (endGameCanvas == null)
                {
                    // 尝试通过GameObject名称查找
                    var endGameCanvasGO = GameObject.Find("EndGameCanvas");
                    if (endGameCanvasGO != null)
                    {
                        endGameCanvas = endGameCanvasGO.GetComponent<SplendorUnity.UI.EndGameCanvas>();
                        if (enableDebugLog)
                            Debug.Log("GameStateListener: 通过GameObject名称找到EndGameCanvas");
                    }
                }
            }
            
            if (endGameCanvas != null)
            {
                // 获取当前游戏状态，确定获胜者
                if (gameState != null && gameState.agents != null)
                {
                    int winnerPlayerId = -1;
                    int highestScore = -1;
                    
                    for (int i = 0; i < gameState.agents.Count; i++)
                    {
                        var agent = gameState.agents[i];
                        if (agent != null && agent.score > highestScore)
                        {
                            highestScore = agent.score;
                            winnerPlayerId = i + 1; // 转换为1-based索引
                        }
                    }
                    
                    if (winnerPlayerId > 0)
                    {
                        if (enableDebugLog)
                            Debug.Log($"GameStateListener: 通知EndGameCanvas显示获胜者 Player {winnerPlayerId}");
                        
                        endGameCanvas.ShowEndGame(winnerPlayerId);
                    }
                    else
                    {
                        if (enableDebugLog)
                            Debug.LogWarning("GameStateListener: 无法确定获胜者");
                    }
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GameStateListener: gameState或agents为空，无法确定获胜者");
                }
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning("GameStateListener: 找不到EndGameCanvas组件，尝试了多种查找方式");
            }
        }
        
        /// <summary>
        /// 延迟一帧后更新所有管理器
        /// </summary>
        private IEnumerator UpdateAllManagersAfterFrame()
        {
            yield return null;
            
                
            UpdateAllManagers();
        }
        
        /// <summary>
        /// 更新所有管理器
        /// </summary>
        public void UpdateAllManagers()
        {
            if (enableDebugLog)
                Debug.Log("GameStateListener: UpdateAllManagers被调用");
                
            // 如果gameState为null，尝试刷新引用
            if (gameState == null)
            {
                if (enableDebugLog)
                    Debug.LogWarning("GameStateListener: gameState为null，尝试刷新引用");
                
                // 尝试从GameManager获取最新的gameState
                if (gameManager != null && gameManager.gameRule != null)
                {
                    gameState = gameManager.gameRule.currentGameState;
                    if (enableDebugLog)
                        Debug.Log($"GameStateListener: 从GameManager刷新gameState: {gameState != null}");
                }
                
                if (gameState == null)
                {
                    if (enableDebugLog)
                        Debug.LogError("GameStateListener: 无法获取gameState，跳过更新");
                    return;
                }
            }
            
            if (enableDebugLog)
                Debug.Log($"GameStateListener: 开始更新所有管理器，gameState: {gameState != null}, currentTurnNumber: {currentTurnNumber}");
                
            // 更新CardManager
            if (cardManager != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStateListener: 更新CardManager");
                cardManager.UpdateCardsFromGameState();
            }
            
            // 更新GemPileManager
            if (gemPileManager != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStateListener: 更新GemPileManager");
                gemPileManager.UpdateGemPilesFromGameState();
            }
            
            // 更新NobleManager
            if (nobleManager != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStateListener: 更新NobleManager");
                nobleManager.UpdateNoblesFromGameState();
            }
            
            // 更新TopInfoBar
            if (enableDebugLog)
                Debug.Log("GameStateListener: 更新TopInfoBar");
            UpdateTopInfoBar();
            
            // 更新PlayerInfoDisplay
            if (enableDebugLog)
                Debug.Log("GameStateListener: 更新PlayerInfoDisplay");
            UpdatePlayerInfoDisplays();
            
            if (enableDebugLog)
                Debug.Log("GameStateListener: 所有管理器更新完成");
        }
        
        /// <summary>
        /// 更新TopInfoBar显示
        /// </summary>
        private void UpdateTopInfoBar()
        {
            if (gameState == null)
            {
                
                if (gameState == null)
                {
                    return;
                }
            }
            
            if (topInfoBar != null)
            {
                topInfoBar.UpdateGameInfo(
                    gameState.agentToMove,
                    currentTurnNumber,
                    gameState.agents
                );
            }
        }
        
        /// <summary>
        /// 更新PlayerInfoDisplay显示
        /// </summary>
        private void UpdatePlayerInfoDisplays()
        {
            if (gameState == null)
            {
                if (enableDebugLog)
                    Debug.LogWarning("GameStateListener: gameState为null，无法更新PlayerInfoDisplay");
                return;
            }
            
            if (enableDebugLog)
                Debug.Log($"GameStateListener: 开始更新PlayerInfoDisplay，共{playerInfoDisplays.Count}个，agents数量: {gameState.agents.Count}");
                        
            foreach (var playerDisplay in playerInfoDisplays)
            {
                if (playerDisplay != null)
                {
                    int playerId = playerDisplay.playerId;
                    if (enableDebugLog)
                        Debug.Log($"GameStateListener: 更新PlayerInfoDisplay {playerId}");
                        
                    if (playerId < gameState.agents.Count)
                    {
                        var agentState = gameState.agents[playerId];
                        if (enableDebugLog)
                            Debug.Log($"GameStateListener: Player {playerId} 的gems: {GetGemCountString(agentState)}");
                        
                        playerDisplay.UpdateAgentState(agentState);
                    }
                    else
                    {
                        if (enableDebugLog)
                            Debug.LogWarning($"GameStateListener: Player {playerId} 超出agents范围 ({gameState.agents.Count})");
                    }
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GameStateListener: 发现null的PlayerInfoDisplay");
                }
            }
        }
        
        /// <summary>
        /// 获取宝石数量字符串（用于调试）
        /// </summary>
        private string GetGemCountString(SplendorGameState.AgentState agentState)
        {
            if (agentState == null || agentState.gems == null) return "null";
            
            var gemStrings = new List<string>();
            foreach (var kvp in agentState.gems)
            {
                gemStrings.Add($"{kvp.Key}:{kvp.Value}");
            }
            return string.Join(", ", gemStrings);
        }
        
                
        /// <summary>
        /// 设置TopInfoBar引用
        /// </summary>
        public void SetTopInfoBar(TopInfoBar infoBar)
        {
            topInfoBar = infoBar;
        }
        
        /// <summary>
        /// 添加PlayerInfoDisplay引用
        /// </summary>
        public void AddPlayerInfoDisplay(PlayerInfoDisplay playerDisplay)
        {
            if (playerDisplay != null && !playerInfoDisplays.Contains(playerDisplay))
            {
                playerInfoDisplays.Add(playerDisplay);
            }
        }
        
        /// <summary>
        /// 移除PlayerInfoDisplay引用
        /// </summary>
        public void RemovePlayerInfoDisplay(PlayerInfoDisplay playerDisplay)
        {
            if (playerInfoDisplays.Remove(playerDisplay))
            {
                if (enableDebugLog)
                    Debug.Log($"GameStateListener: 已移除PlayerInfoDisplay {playerDisplay.playerId}");
            }
        }
        
        /// <summary>
        /// 获取当前回合数
        /// </summary>
        public int GetCurrentTurnNumber()
        {
            return currentTurnNumber;
        }
        
        /// <summary>
        /// 重置回合数
        /// </summary>
        public void ResetTurnNumber()
        {
            currentTurnNumber = 0;
        }
    }
}
