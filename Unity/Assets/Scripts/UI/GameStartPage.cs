using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SplendorUnity.Core;
using System.Collections;
using System.Collections.Generic;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 游戏开始页面 - 显示游戏背景和开始按钮
    /// </summary>
    public class GameStartPage : MonoBehaviour
    {
        [Header("UI Components")]
        public Button startGameButton;             // 开始游戏按钮
        public TextMeshProUGUI startButtonText;    // 开始按钮文本
        
        // 新增的UI组件
        [Header("Agent Selection")]
        public TMP_Dropdown player1AgentDropdown;  // Player 1 代理选择下拉列表
        public TMP_Dropdown player2AgentDropdown;  // Player 2 代理选择下拉列表
        
        [Header("Game Settings")]
        public TMP_InputField timeLimitInputField; // 时间限制输入框
        [Header("Agent Settings")]
        [Tooltip("每个代理的操作延迟时间（秒）")]
        public float agentActionDelay = 0.1f; // 每个代理的操作延迟时间（秒）
        
        [Header("Settings")]
        public bool enableDebugLog = true;
        
        // 私有字段
        private GameManager gameManager;
        private GameLauncher gameLauncher;
        
        // 游戏设置
        private float timeLimit = 30.0f; // 默认30秒
        private string player1AgentType = "Human"; // 默认人类玩家
        private string player2AgentType = "Random"; // 默认AI对手
        
        // 防止重复初始化的标志
        private bool isInitialized = false;
        private bool isStarted = false;
        
        private void Awake()
        {
            // 防止重复初始化
            if (isInitialized)
            {
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已经初始化过，跳过重复初始化");
                return;
            }
            
            InitializePage();
            isInitialized = true;
        }
        
        private void Start()
        {
            // 防止重复初始化
            if (isStarted)
            {
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已经启动过，跳过重复启动");
                return;
            }
            
            // 自动查找GameManager
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameStartPage: Cannot find GameManager");
            }
            
            // 自动查找GameLauncher
            gameLauncher = FindObjectOfType<GameLauncher>();
            if (gameLauncher == null)
            {
                Debug.LogWarning("GameStartPage: Cannot find GameLauncher");
            }
            
            // 设置按钮事件（只在Start中设置，避免重复）
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveAllListeners();
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            
            // 设置下拉列表事件
            SetupDropdownEvents();
            
            isStarted = true;
            if (enableDebugLog)
                Debug.Log("GameStartPage: 初始化完成");
        }
        
        /// <summary>
        /// 初始化页面
        /// </summary>
        private void InitializePage()
        {
            // 只设置下拉列表事件，按钮事件在Start中设置，避免重复
            SetupDropdownEvents();
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 页面初始化完成");
        }
        
        
        

        


        
        /// <summary>
        /// 设置下拉列表事件
        /// </summary>
        private void SetupDropdownEvents()
        {
            if (player1AgentDropdown != null)
            {
                player1AgentDropdown.onValueChanged.AddListener(OnPlayer1AgentChanged);
            }
            
            if (player2AgentDropdown != null)
            {
                player2AgentDropdown.onValueChanged.AddListener(OnPlayer2AgentChanged);
            }
        }
        
        /// <summary>
        /// Player 1代理选择改变事件
        /// </summary>
        private void OnPlayer1AgentChanged(int value)
        {
            player1AgentType = GetAgentTypeFromDropdownValue(player1AgentDropdown, value);
            
            if (enableDebugLog)
                Debug.Log($"GameStartPage: Player 1 Agent changed to {player1AgentType}");
        }
        
        /// <summary>
        /// Player 2代理选择改变事件
        /// </summary>
        private void OnPlayer2AgentChanged(int value)
        {
            player2AgentType = GetAgentTypeFromDropdownValue(player2AgentDropdown, value);
            
            if (enableDebugLog)
                Debug.Log($"GameStartPage: Player 2 Agent changed to {player2AgentType}");
        }
        
        /// <summary>
        /// 显示页面
        /// </summary>
        public void ShowPage()
        {
            if (gameObject != null)
            {
                gameObject.SetActive(true);
                
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 页面已显示");
            }
        }
        
        /// <summary>
        /// 隐藏页面
        /// </summary>
        public void HidePage()
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
                
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 页面已隐藏");
            }
        }
        
        /// <summary>
        /// 开始游戏按钮点击事件
        /// </summary>
        private void OnStartGameClicked()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: Start Game按钮被点击");
            
            // 开始游戏（在隐藏页面之前）
            StartGame();
            
            // 隐藏页面
            HidePage();
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            if (gameManager != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 开始创建游戏组件...");
                
                // 验证用户选择
                ValidateUserSelections();
                
                // 重置游戏状态，确保新游戏从干净状态开始
                ResetGameState();
                
                // 创建所有必要的游戏组件
                CreateGameComponents();
                
                // 设置GameManager的代理操作延迟
                gameManager.agentActionDelay = agentActionDelay;
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 已设置GameManager.agentActionDelay = {agentActionDelay}");
                
                // 设置GameManager为交互模式（如果需要的话）
                // gameManager.interactive = true; // 保持交互模式
                
                // 调用GameManager开始游戏
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 调用gameManager.StartGame()前，agents数量: {gameManager.agents.Count}");
                
                gameManager.StartGame();
                
                if (enableDebugLog)
                {
                    Debug.Log("GameStartPage: 游戏已开始");
                    Debug.Log($"GameStartPage: 调用gameManager.StartGame()后，agents数量: {gameManager.agents.Count}");
                    
                    // 再次验证agents设置
                    for (int i = 0; i < gameManager.agents.Count; i++)
                    {
                        var agent = gameManager.agents[i];
                        Debug.Log($"GameStartPage: 游戏开始后Agent {i}: {agent.GetName()} ({agent.GetType().Name})");
                    }
                }
                
                // 验证agents设置是否保持正确
                if (enableDebugLog)
                {
                    Debug.Log($"GameStartPage: 游戏开始后验证agents设置，当前数量: {gameManager.agents.Count}");
                    for (int i = 0; i < gameManager.agents.Count; i++)
                    {
                        var agent = gameManager.agents[i];
                        Debug.Log($"GameStartPage: 游戏开始后Agent {i}: {agent.GetName()} ({agent.GetType().Name})");
                    }
                }
                
                // 不再需要强制重新应用agents，因为已经在CreateGameComponents中设置好了
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 跳过ForceReapplyAgentsAfterFrame，agents已在CreateGameComponents中设置");
                
                // 通知GameLauncher游戏已开始
                if (gameLauncher != null)
                {
                    // 通过反射设置GameLauncher的gameRunning字段
                    var gameRunningField = typeof(GameLauncher).GetField("gameRunning", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (gameRunningField != null)
                    {
                        gameRunningField.SetValue(gameLauncher, true);
                        if (enableDebugLog)
                            Debug.Log("GameStartPage: 已设置GameLauncher.gameRunning = true");
                    }
                }
            }
            else
            {
                Debug.LogError("GameStartPage: GameManager为空，无法开始游戏");
            }
        }
        
        /// <summary>
        /// 创建游戏组件
        /// </summary>
        private void CreateGameComponents()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 开始创建游戏组件...");
            
            // 1. 创建SplendorGameRule
            if (gameManager.gameRule == null)
            {
                gameManager.gameRule = gameManager.gameObject.AddComponent<SplendorUnity.Core.SplendorGameRule>();
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已创建SplendorGameRule");
            }
            
            // 2. 创建并设置AI代理
            CreateAIAgents();
            
            // 3. 强制重新初始化游戏规则，确保新游戏从干净状态开始
            if (gameManager.gameRule != null && gameManager.agents.Count > 0)
            {
                try
                {
                    // 由于我们已经在ResetGameState中清除了currentGameState，这里应该总是重新初始化
                    gameManager.gameRule.Initialize(gameManager.agents.Count);
                    if (enableDebugLog)
                        Debug.Log($"GameStartPage: 已重新初始化SplendorGameRule，agents数量: {gameManager.agents.Count}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"GameStartPage: 初始化SplendorGameRule失败: {e.Message}");
                }
            }
            
            // 4. 创建玩家信息显示
            CreatePlayerInfoDisplays();
            
            // 5. 创建顶部信息栏
            CreateTopInfoBar();
            
            // 6. 设置TopInfoBar的玩家名称
            SetTopInfoBarPlayerNames();
            
            // 7. 创建GameStateListener
            CreateGameStateListener();
            
            // 8. 创建GameDisplayer（如果不存在）
            CreateGameDisplayer();
            
            // 9. 创建EndGameCanvas（如果不存在）
            CreateEndGameCanvas();
            
            // 10. 创建其他必要的UI组件
            CreateOtherUIComponents();
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 游戏组件创建完成");
        }
        
        /// <summary>
        /// 创建AI代理
        /// </summary>
        private void CreateAIAgents()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 创建AI代理...");
            
            // 先读取用户选择，确保我们有正确的设置
            ReadUserSelections();
            if (enableDebugLog)
                Debug.Log($"GameStartPage: 用户选择 - Player1: {player1AgentType}, Player2: {player2AgentType}");
            
            // 禁用GameManager的自动查找agents功能，防止覆盖我们的设置
            gameManager.autoFindAgents = false;
            if (enableDebugLog)
                Debug.Log("GameStartPage: 已禁用GameManager的自动查找agents功能");
            
            // 强制清除场景中所有已存在的Agent组件，确保完全控制
            ClearAllExistingAgents();
            
            // 强制清除现有的agents列表，确保我们的设置不被覆盖
            if (gameManager.agents != null)
            {
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 清除前agents数量: {gameManager.agents.Count}");
                gameManager.agents.Clear();
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已清除现有的agents列表");
            }
            
            // 直接使用用户选择的设置创建代理，不再依赖TestSceneSetup
            CreateDefaultAgents();
            
            if (enableDebugLog)
                Debug.Log($"GameStartPage: AI代理创建完成，当前agents数量: {gameManager.agents.Count}");
            
            // 验证agents设置
            for (int i = 0; i < gameManager.agents.Count; i++)
            {
                var agent = gameManager.agents[i];
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: Agent {i}: {agent.GetName()} ({agent.GetType().Name})");
            }
        }
        
        /// <summary>
        /// 清除场景中所有已存在的Agent组件
        /// </summary>
        private void ClearAllExistingAgents()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 清除场景中所有已存在的Agent组件...");
            
            // 查找场景中所有的BaseAgent组件
            var allAgents = FindObjectsOfType<SplendorUnity.AI.BaseAgent>();
            if (enableDebugLog)
                Debug.Log($"GameStartPage: 发现场景中有 {allAgents.Length} 个已存在的Agent组件");
            
            // 删除所有已存在的Agent组件
            foreach (var agent in allAgents)
            {
                if (agent != null)
                {
                    if (enableDebugLog)
                        Debug.Log($"GameStartPage: 删除已存在的Agent: {agent.GetType().Name} ({agent.GetName()})");
                    
                    // 如果Agent有GameObject，删除整个GameObject
                    if (agent.gameObject != null && agent.gameObject != gameManager.gameObject)
                    {
                        DestroyImmediate(agent.gameObject);
                    }
                    else
                    {
                        // 如果Agent在GameManager上，只删除组件
                        DestroyImmediate(agent);
                    }
                }
            }
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 已清除所有已存在的Agent组件");
        }
        
        /// <summary>
        /// 创建默认代理
        /// </summary>
        private void CreateDefaultAgents()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 根据用户选择创建代理...");
            
            // 不再需要在这里调用ReadUserSelections()，因为已经在CreateAIAgents()中调用了
            
            if (gameManager.agents == null)
                gameManager.agents = new System.Collections.Generic.List<SplendorUnity.AI.BaseAgent>();
            
            // 不需要再次Clear，因为已经在CreateAIAgents()中Clear过了
            
            // 创建Player 1代理
            var agent1 = CreateAgentByType(player1AgentType, 0, "Player 1");
            if (agent1 != null)
            {
                gameManager.agents.Add(agent1);
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 已创建Player 1代理 ({player1AgentType})");
            }
            
            // 创建Player 2代理
            var agent2 = CreateAgentByType(player2AgentType, 1, "Player 2");
            if (agent2 != null)
            {
                gameManager.agents.Add(agent2);
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 已创建Player 2代理 ({player2AgentType})");
            }
            
            // 设置时间限制
            if (gameManager != null)
            {
                gameManager.timeLimit = (double)timeLimit;
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 已设置GameManager时间限制为 {timeLimit} 秒");
            }
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 代理创建完成");
        }
        
        /// <summary>
        /// 验证用户选择
        /// </summary>
        private void ValidateUserSelections()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 验证用户选择...");
            
            // 验证下拉列表引用
            if (player1AgentDropdown == null)
                Debug.LogWarning("GameStartPage: Player 1 Agent Dropdown 引用为空");
            if (player2AgentDropdown == null)
                Debug.LogWarning("GameStartPage: Player 2 Agent Dropdown 引用为空");
            if (timeLimitInputField == null)
                Debug.LogWarning("GameStartPage: Time Limit Input Field 引用为空");
            
            // 验证时间限制
            if (timeLimitInputField != null && !string.IsNullOrEmpty(timeLimitInputField.text))
            {
                if (float.TryParse(timeLimitInputField.text, out float inputTime))
                {
                    if (inputTime <= 0)
                    {
                        Debug.LogWarning($"GameStartPage: 时间限制无效 ({inputTime})，使用默认值30秒");
                        timeLimit = 30.0f;
                    }
                    else
                    {
                        timeLimit = inputTime;
                        if (enableDebugLog)
                            Debug.Log($"GameStartPage: 验证时间限制: {timeLimit} 秒");
                    }
                }
                else
                {
                    Debug.LogWarning("GameStartPage: 无法解析时间限制，使用默认值30秒");
                    timeLimit = 30.0f;
                }
            }
            else
            {
                Debug.LogWarning("GameStartPage: 时间限制输入框为空，使用默认值30秒");
                timeLimit = 30.0f;
            }
            
            if (enableDebugLog)
                Debug.Log($"GameStartPage: 最终设置 - Player 1: {player1AgentType}, Player 2: {player2AgentType}, Time: {timeLimit}s");
        }
        
        /// <summary>
        /// 强制重新应用agents设置，防止被其他地方覆盖
        /// </summary>
        private IEnumerator ForceReapplyAgentsAfterFrame()
        {
            // 等待一帧，确保所有初始化完成
            yield return null;
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: ForceReapplyAgentsAfterFrame被调用，但已不再需要重复创建agents");
            
            // 不再重复创建agents，因为已经在CreateGameComponents中设置好了
            yield break;
        }
        
        /// <summary>
        /// 检查下拉列表的选项内容
        /// </summary>
        private void CheckDropdownOptions()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 检查下拉列表选项内容...");
            
            if (player1AgentDropdown != null)
            {
                Debug.Log($"GameStartPage: Player 1 Dropdown选项数量: {player1AgentDropdown.options.Count}");
                for (int i = 0; i < player1AgentDropdown.options.Count; i++)
                {
                    var option = player1AgentDropdown.options[i];
                    Debug.Log($"GameStartPage: Player 1 Dropdown选项 {i}: {option.text}");
                }
            }
            
            if (player2AgentDropdown != null)
            {
                Debug.Log($"GameStartPage: Player 2 Dropdown选项数量: {player2AgentDropdown.options.Count}");
                for (int i = 0; i < player2AgentDropdown.options.Count; i++)
                {
                    var option = player2AgentDropdown.options[i];
                    Debug.Log($"GameStartPage: Player 2 Dropdown选项 {i}: {option.text}");
                }
            }
        }
        
        /// <summary>
        /// 根据下拉列表值获取代理类型
        /// </summary>
        private string GetAgentTypeFromDropdownValue(TMP_Dropdown dropdown, int value)
        {
            if (dropdown == null || value < 0 || value >= dropdown.options.Count)
            {
                Debug.LogWarning($"GameStartPage: Dropdown值 {value} 无效，使用默认Random");
                return "Random";
            }
            
            var option = dropdown.options[value];
            string optionText = option.text.ToLower();
            
            if (enableDebugLog)
                Debug.Log($"GameStartPage: Dropdown值 {value} 对应选项文本: {option.text}");
            
            // 根据选项文本确定代理类型
            if (optionText.Contains("human") || optionText.Contains("人"))
            {
                return "Human";
            }
            else if (optionText.Contains("first") || optionText.Contains("firstmove"))
            {
                return "FirstMove";
            }
            else if (optionText.Contains("random") || optionText.Contains("随机"))
            {
                return "Random";
            }
            else
            {
                Debug.LogWarning($"GameStartPage: 无法识别选项文本 '{option.text}'，使用默认Random");
                return "Random";
            }
        }
        
        /// <summary>
        /// 读取用户的最新选择
        /// </summary>
        private void ReadUserSelections()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 开始读取用户选择...");
            
            // 检查下拉列表的选项内容
            CheckDropdownOptions();
            
            // 读取时间限制
            if (timeLimitInputField != null && !string.IsNullOrEmpty(timeLimitInputField.text))
            {
                if (float.TryParse(timeLimitInputField.text, out float inputTime))
                {
                    timeLimit = inputTime;
                    if (enableDebugLog)
                        Debug.Log($"GameStartPage: 读取到时间限制: {timeLimit} 秒");
                }
            }
            
            // 读取Player 1代理类型
            if (player1AgentDropdown != null)
            {
                int dropdownValue = player1AgentDropdown.value;
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: Player 1 Dropdown当前值: {dropdownValue}");
                
                // 根据实际选项内容确定代理类型
                player1AgentType = GetAgentTypeFromDropdownValue(player1AgentDropdown, dropdownValue);
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 读取到Player 1代理类型: {player1AgentType}");
            }
            else
            {
                Debug.LogWarning("GameStartPage: Player 1 Dropdown引用为空");
            }
            
            // 读取Player 2代理类型
            if (player2AgentDropdown != null)
            {
                int dropdownValue = player2AgentDropdown.value;
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: Player 2 Dropdown当前值: {dropdownValue}");
                
                // 根据实际选项内容确定代理类型
                player2AgentType = GetAgentTypeFromDropdownValue(player2AgentDropdown, dropdownValue);
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 读取到Player 2代理类型: {player2AgentType}");
            }
            else
            {
                Debug.LogWarning("GameStartPage: Player 2 Dropdown引用为空");
            }
            
            if (enableDebugLog)
                Debug.Log($"GameStartPage: 最终用户选择 - Player1: {player1AgentType}, Player2: {player2AgentType}");
        }
        
        /// <summary>
        /// 根据类型创建代理
        /// </summary>
        private SplendorUnity.AI.BaseAgent CreateAgentByType(string agentType, int id, string name)
        {
            switch (agentType)
            {
                case "Human":
                    var humanAgent = gameManager.gameObject.AddComponent<SplendorUnity.AI.HumanAgent>();
                    humanAgent.SetId(id);
                    humanAgent.SetName(name);
                    return humanAgent;
                    
                case "FirstMove":
                    var firstMoveAgent = gameManager.gameObject.AddComponent<SplendorUnity.AI.FirstMoveAgent>();
                    firstMoveAgent.SetId(id);
                    firstMoveAgent.SetName(name);
                    return firstMoveAgent;
                    
                case "Random":
                    var randomAgent = gameManager.gameObject.AddComponent<SplendorUnity.AI.RandomAgent>();
                    randomAgent.SetId(id);
                    randomAgent.SetName(name);
                    return randomAgent;
                    
                default:
                    Debug.LogWarning($"GameStartPage: 未知的代理类型 {agentType}，使用默认Random代理");
                    var defaultAgent = gameManager.gameObject.AddComponent<SplendorUnity.AI.RandomAgent>();
                    defaultAgent.SetId(id);
                    defaultAgent.SetName(name);
                    return defaultAgent;
            }
        }
        
        /// <summary>
        /// 创建玩家信息显示
        /// </summary>
        private void CreatePlayerInfoDisplays()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 创建玩家信息显示...");
            
            // 检查是否已经存在PlayerInfoDisplay，避免重复创建
            var existingDisplays = FindObjectsOfType<PlayerInfoDisplay>();
            if (existingDisplays.Length > 0)
            {
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 发现已存在的PlayerInfoDisplay ({existingDisplays.Length}个)，跳过创建");
                return;
            }
            
            // 查找PlayerInfoDisplayExample并调用其设置方法
            var playerInfoExample = FindObjectOfType<SplendorUnity.Examples.PlayerInfoDisplayExample>();
            if (playerInfoExample != null)
            {
                var setupMethod = typeof(SplendorUnity.Examples.PlayerInfoDisplayExample).GetMethod("SetupPlayerInfoDisplays", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (setupMethod != null)
                {
                    setupMethod.Invoke(playerInfoExample, null);
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已通过PlayerInfoDisplayExample创建玩家信息显示");
                }
            }
        }
        
        /// <summary>
        /// 创建顶部信息栏
        /// </summary>
        private void CreateTopInfoBar()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 创建顶部信息栏...");
            
            // 检查是否已经存在TopInfoBar，避免重复创建
            var existingTopInfoBar = FindObjectOfType<TopInfoBar>();
            if (existingTopInfoBar != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 发现已存在的TopInfoBar，跳过创建");
                return;
            }
            
            // 查找TopInfoBarSetup并调用其创建方法
            var topInfoBarSetup = FindObjectOfType<SplendorUnity.UI.TopInfoBarSetup>();
            if (topInfoBarSetup != null)
            {
                var createMethod = typeof(SplendorUnity.UI.TopInfoBarSetup).GetMethod("CreateTopInfoBarUI", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (createMethod != null)
                {
                    createMethod.Invoke(topInfoBarSetup, null);
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已通过TopInfoBarSetup创建顶部信息栏");
                }
            }
        }
        
        /// <summary>
        /// 设置TopInfoBar的玩家名称
        /// </summary>
        private void SetTopInfoBarPlayerNames()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 设置TopInfoBar玩家名称...");
            
            // 查找TopInfoBar
            var topInfoBar = FindObjectOfType<TopInfoBar>();
            if (topInfoBar != null)
            {
                // 设置玩家名称格式为 "Player1-{AgentType}"
                string player1Name = $"Player1-{player1AgentType}";
                string player2Name = $"Player2-{player2AgentType}";
                
                topInfoBar.SetPlayerNames(player1Name, player2Name);
                
                if (enableDebugLog)
                    Debug.Log($"GameStartPage: 已设置TopInfoBar玩家名称 - {player1Name}, {player2Name}");
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning("GameStartPage: 未找到TopInfoBar，无法设置玩家名称");
            }
        }
        
        /// <summary>
        /// 创建GameStateListener
        /// </summary>
        private void CreateGameStateListener()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 创建GameStateListener...");
            
            // 检查是否已经存在GameStateListener，避免重复创建
            var existingGameStateListener = FindObjectOfType<GameStateListener>();
            if (existingGameStateListener != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 发现已存在的GameStateListener，跳过创建");
                return;
            }
            
            // 创建GameStateListener
            var gameStateListenerGO = new GameObject("GameStateListener");
            var gameStateListener = gameStateListenerGO.AddComponent<GameStateListener>();
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 已创建GameStateListener");
        }
        
        /// <summary>
        /// 检查EndGameCanvas是否存在
        /// </summary>
        private void CreateEndGameCanvas()
        {
            // 查找场景中是否已有EndGameCanvas
            var existingEndGameCanvas = FindObjectOfType<SplendorUnity.UI.EndGameCanvas>();
            if (existingEndGameCanvas != null)
            {
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已找到现有的EndGameCanvas，无需创建新的");
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning("GameStartPage: 场景中没有找到EndGameCanvas，请确保手动创建");
            }
        }
        
        /// <summary>
        /// 创建GameDisplayer（如果不存在）
        /// </summary>
        private void CreateGameDisplayer()
        {
            if (gameManager.displayer == null)
            {
                // 查找场景中是否已有GameDisplayer
                var existingDisplayer = FindObjectOfType<SplendorUnity.Display.GameDisplayer>();
                if (existingDisplayer != null)
                {
                    gameManager.displayer = existingDisplayer;
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已找到现有的GameDisplayer");
                }
                else
                {
                    // 创建新的GameDisplayer
                    var displayerObj = new GameObject("GameDisplayer");
                    displayerObj.transform.SetParent(gameManager.transform);
                    gameManager.displayer = displayerObj.AddComponent<SplendorUnity.Display.GameDisplayer>();
                    
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已创建新的GameDisplayer");
                }
                
                // 初始化GameDisplayer
                if (gameManager.displayer != null)
                {
                    gameManager.displayer.InitDisplayer(gameManager);
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已初始化GameDisplayer");
                }
            }
        }
        
        /// <summary>
        /// 创建其他必要的UI组件
        /// </summary>
        private void CreateOtherUIComponents()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 创建其他UI组件...");
            
            // 这里可以添加其他需要延迟创建的UI组件
            // 例如：CardManager, NobleManager, GemPileManager等
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 其他UI组件创建完成");
        }
        
        /// <summary>
        /// 重置游戏状态，确保新游戏从干净状态开始
        /// </summary>
        private void ResetGameState()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 开始重置游戏状态...");
            
            // 1. 重置GameManager的游戏状态
            if (gameManager != null)
            {
                // 停止当前游戏
                if (gameManager.gameRule != null)
                {
                    // 清除当前的GameState
                    if (gameManager.gameRule.currentGameState != null)
                    {
                        if (enableDebugLog)
                            Debug.Log("GameStartPage: 清除当前GameState");
                        
                        // 通过反射访问并清除currentGameState
                        var currentGameStateField = typeof(SplendorUnity.Core.SplendorGameRule).GetField("currentGameState", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (currentGameStateField != null)
                        {
                            currentGameStateField.SetValue(gameManager.gameRule, null);
                            if (enableDebugLog)
                                Debug.Log("GameStartPage: 已清除GameRule的currentGameState");
                        }
                    }
                }
                
                // 清除agents列表
                if (gameManager.agents != null)
                {
                    gameManager.agents.Clear();
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已清除GameManager的agents列表");
                }
                
                // 重置游戏历史（通过反射访问私有字段）
                var gameHistoryField = typeof(GameManager).GetField("gameHistory", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (gameHistoryField != null)
                {
                    var gameHistory = gameHistoryField.GetValue(gameManager) as Dictionary<string, object>;
                    if (gameHistory != null)
                    {
                        gameHistory.Clear();
                        
                        // 重新初始化必要的键，避免KeyNotFoundException
                        gameHistory["actions"] = new List<Dictionary<string, object>>();
                        
                        if (enableDebugLog)
                            Debug.Log("GameStartPage: 已清除GameManager的gameHistory并重新初始化actions键");
                    }
                }
            }
            
            // 2. 清除场景中可能存在的旧组件
            ClearOldGameComponents();
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 游戏状态重置完成");
        }
        
        /// <summary>
        /// 清除旧的游戏组件
        /// </summary>
        private void ClearOldGameComponents()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 清除旧的游戏组件...");
            
            // 清除旧的PlayerInfoDisplay
            var oldPlayerInfoDisplays = FindObjectsOfType<PlayerInfoDisplay>();
            foreach (var display in oldPlayerInfoDisplays)
            {
                if (display != null)
                {
                    DestroyImmediate(display.gameObject);
                    if (enableDebugLog)
                        Debug.Log("GameStartPage: 已清除旧的PlayerInfoDisplay");
                }
            }
            
            // 清除旧的TopInfoBar
            var oldTopInfoBar = FindObjectOfType<TopInfoBar>();
            if (oldTopInfoBar != null)
            {
                DestroyImmediate(oldTopInfoBar.gameObject);
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已清除旧的TopInfoBar");
            }
            
            // 清除旧的GameStateListener
            var oldGameStateListener = FindObjectOfType<GameStateListener>();
            if (oldGameStateListener != null)
            {
                DestroyImmediate(oldGameStateListener.gameObject);
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已清除旧的GameStateListener");
            }
            
            // 清除旧的EndGameCanvas（如果存在）
            var oldEndGameCanvas = FindObjectOfType<EndGameCanvas>();
            if (oldEndGameCanvas != null)
            {
                oldEndGameCanvas.HideEndGame();
                if (enableDebugLog)
                    Debug.Log("GameStartPage: 已隐藏旧的EndGameCanvas");
            }
            
            if (enableDebugLog)
                Debug.Log("GameStartPage: 旧的游戏组件清除完成");
        }
        
        /// <summary>
        /// 手动刷新页面
        /// </summary>
        [ContextMenu("Refresh Page")]
        public void RefreshPage()
        {
            if (enableDebugLog)
                Debug.Log("GameStartPage: 手动刷新页面");
            
            UpdatePage();
        }
        
        /// <summary>
        /// 更新页面
        /// </summary>
        private void UpdatePage()
        {
            // 这里可以添加页面更新逻辑
            if (enableDebugLog)
                Debug.Log("GameStartPage: 页面已更新");
        }
        
        /// <summary>
        /// 获取页面状态
        /// </summary>
        [ContextMenu("Get Page Status")]
        public string GetPageStatus()
        {
            string status = $"=== GameStartPage Status ===\n";
            status += $"Start Game Button: {(startGameButton != null ? "Found" : "Not Found")}\n";
            status += $"Start Button Text: {(startButtonText != null ? "Found" : "Not Found")}\n";
            status += $"Player 1 Dropdown: {(player1AgentDropdown != null ? "Found" : "Not Found")}\n";
            status += $"Player 2 Dropdown: {(player2AgentDropdown != null ? "Found" : "Not Found")}\n";
            status += $"Time Limit Input: {(timeLimitInputField != null ? "Found" : "Not Found")}\n";
            status += $"GameManager: {(gameManager != null ? "Connected" : "Not Found")}\n";
            status += $"Player 1 Agent Type: {player1AgentType}\n";
            status += $"Player 2 Agent Type: {player2AgentType}\n";
            status += $"Time Limit: {timeLimit} seconds\n";
            
            return status;
        }
    }
}
