using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SplendorUnity.AI;
using SplendorUnity.Display;
using SplendorUnity.Events;
using SplendorUnity.Utils;

namespace SplendorUnity.Core
{
    /// <summary>
    /// 游戏主管理器，替代原Game.cs
    /// 负责游戏状态管理、AI代理管理、回合控制、事件分发、游戏配置
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("游戏配置")]
        public int seed = Constants.DEFAULT_SEED;
        public double timeLimit = Constants.DEFAULT_TIME_LIMIT;
        public int warningLimit = Constants.DEFAULT_WARNING_LIMIT;
        [Tooltip("启用交互模式：玩家 vs AI对战")]
        public bool interactive = true; // 默认启用交互模式，玩家 vs AI
        public float agentActionDelay = 0.1f; // 每个代理的操作延迟时间（秒）
        public bool enableDebugLog = true; // 启用调试日志
        
        [Header("玩家输入状态")]
        public bool playerHasInput = false; // 玩家是否已经输入
        public object playerSelectedAction = null; // 玩家选择的action
        
        [Header("组件引用")]
        public SplendorGameRule gameRule;
        public GameDisplayer displayer;
        
        [Header("AI代理")]
        public List<BaseAgent> agents = new List<BaseAgent>();
        public List<string> agentsNamelist = new List<string>();
        public bool autoFindAgents = true; // 是否自动查找场景中的agents
        
        // 私有字段
        private List<int> seedList = new List<int>();
        private int seedIdx = 0;
        private Dictionary<int, int> warnings = new Dictionary<int, int>();
        private List<(int, int)> warningPositions = new List<(int, int)>();
        private Dictionary<string, object> gameHistory = new Dictionary<string, object>();
        public int actionCounter = 0; // 改为public，供NobleManager访问
        
        // 事件
        public static event Action<GameManager> OnGameStarted;
        public static event Action<GameManager> OnGameEnded;
        public static event Action<int, object> OnActionExecuted;
        public static event Action<int> OnTimeoutWarning;
        public static event Action<int> OnIllegalWarning;
        
        private void Awake()
        {
            // 不再自动初始化游戏，等待GameStartPage来管理
            // InitializeGame();
            
            if (enableDebugLog)
                Debug.Log("GameManager: Awake完成，等待GameStartPage初始化游戏");
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            if (enableDebugLog)
                Debug.Log("GameManager: InitializeGame被调用");
            
            // 只有在启用自动查找时才自动查找AI代理
            if (autoFindAgents && agents.Count == 0)
            {
                var allAgents = FindObjectsOfType<BaseAgent>();
                if (allAgents.Length >= 2)
                {
                    agents.AddRange(allAgents);
                    if (enableDebugLog)
                        Debug.Log($"GameManager: 自动查找到 {allAgents.Length} 个agents");
                }
            }
            
            // 初始化游戏规则 - 修复版本
            if (gameRule == null)
            {
                try
                {
                    gameRule = gameObject.AddComponent<SplendorGameRule>();
                    if (enableDebugLog)
                        Debug.Log("GameManager: 已创建SplendorGameRule组件");
                }
                catch (System.Exception e)
                {
                    if (enableDebugLog)
                        Debug.LogError($"GameManager: 创建SplendorGameRule失败: {e.Message}");
                    return;
                }
            }
            
            // 只在必要时初始化游戏规则，避免覆盖GameStartPage的设置
            if (gameRule.currentGameState == null)
            {
                try
                {
                    gameRule.Initialize(agents.Count);
                    if (enableDebugLog)
                        Debug.Log($"GameManager: 已初始化SplendorGameRule，agents数量: {agents.Count}");
                }
                catch (System.Exception e)
                {
                    if (enableDebugLog)
                        Debug.LogError($"GameManager: 初始化SplendorGameRule失败: {e.Message}");
                    return;
                }
            }
            else
            {
                if (enableDebugLog)
                    Debug.Log("GameManager: GameState已存在，跳过重新初始化以避免board刷新");
            }
            
            // 初始化随机种子
            InitializeSeedList();
            
            // 初始化警告系统
            InitializeWarnings();
            
            // 初始化显示器
            if (displayer == null)
            {
                displayer = FindObjectOfType<GameDisplayer>();
            }
            
            if (displayer != null)
            {
                displayer.InitDisplayer(this);
            }
            
            // 初始化游戏历史
            gameHistory["actions"] = new List<Dictionary<string, object>>();
            
            // 触发游戏开始事件
            OnGameStarted?.Invoke(this);
        }
        
        /// <summary>
        /// 初始化随机种子列表
        /// </summary>
        private void InitializeSeedList()
        {
            var rand = new System.Random(seed);
            seedList.Clear();
            for (int i = 0; i < Constants.MAX_SEED_COUNT; i++)
            {
                seedList.Add(rand.Next(0, unchecked((int)1e10)));
            }
            seedIdx = 0;
        }
        
        /// <summary>
        /// 初始化警告系统
        /// </summary>
        private void InitializeWarnings()
        {
            warnings.Clear();
            for (int i = 0; i < agents.Count; i++)
            {
                warnings[i] = 0;
            }
        }
        
        /// <summary>
        /// 显示游戏开始页面
        /// </summary>
        public void ShowGameStartPage()
        {
            
            // 查找GameStartPage组件
            var gameStartPage = FindObjectOfType<UI.GameStartPage>();
            if (gameStartPage != null)
            {
                gameStartPage.ShowPage();
            }
            else
            {
                StartGame();
            }
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            // 调试：检查agents列表
            if (enableDebugLog)
            {
                Debug.Log($"GameManager.StartGame(): 开始游戏，agents数量: {agents.Count}");
                for (int i = 0; i < agents.Count; i++)
                {
                    var agent = agents[i];
                    if (agent != null)
                    {
                        Debug.Log($"GameManager.StartGame(): Agent {i}: {agent.GetName()} ({agent.GetType().Name})");
                    }
                    else
                    {
                        Debug.LogWarning($"GameManager.StartGame(): Agent {i} 为null");
                    }
                }
            }
            
            // 初始化必要的系统
            InitializeWarnings();
            
            if (enableDebugLog)
                Debug.Log("GameManager.StartGame(): 已初始化警告系统");
            
            // 触发游戏开始事件，通知所有监听器
            OnGameStarted?.Invoke(this);
            
            if (enableDebugLog)
                Debug.Log("GameManager.StartGame(): 已触发OnGameStarted事件");
            
            StartCoroutine(RunGameCoroutine());
        }
        
        /// <summary>
        /// 游戏主循环协程
        /// </summary>
        private System.Collections.IEnumerator RunGameCoroutine()
        {
            if (enableDebugLog)
                Debug.Log("GameManager.RunGameCoroutine(): 游戏主循环开始");
            
            // 添加游戏结束检查的调试日志
            if (enableDebugLog)
                Debug.Log($"GameManager.RunGameCoroutine(): 初始游戏结束检查: {gameRule.GameEnds()}");
            
            while (!gameRule.GameEnds())
            {
                // 每回合检查游戏是否结束
                if (enableDebugLog)
                    Debug.Log($"GameManager.RunGameCoroutine(): 回合开始，游戏结束检查: {gameRule.GameEnds()}");
                
                // 开始回合
                if (displayer != null)
                {
                    displayer.StartRound(gameRule.CurrentGameState);
                }
                
                int agentIndex = gameRule.GetCurrentAgentIndex();
                BaseAgent agent = agentIndex < agents.Count ? agents[agentIndex] : GetDummyAgent();
                
                if (enableDebugLog)
                    Debug.Log($"GameManager.RunGameCoroutine(): 当前回合 - AgentIndex: {agentIndex}, Agent: {(agent != null ? $"{agent.GetName()} ({agent.GetType().Name})" : "null")}");
                
                var gameState = gameRule.CurrentGameState;
                
                // 获取合法动作
                var actions = gameRule.GetLegalActions(gameState, agentIndex);
                var actionsCopy = DeepCopy(actions);
                var gameStateCopy = DeepCopy(gameState);
                
                // 处理私有信息（非完全信息游戏）
                if (gameRule.PrivateInformation != null)
                {
                    ProcessPrivateInformation(gameStateCopy, agentIndex);
                }
                
                // 首次行动动画
                if (actionCounter == 0 && displayer != null)
                {
                    displayer.DisplayState(gameRule.CurrentGameState);
                }
                
                
                // 根据agent类型决定是否需要玩家输入
                if (interactive && IsHumanAgent(agentIndex))
                {
                    // 人类玩家回合，等待玩家输入或超时
                    float waitTime = 0f;
                    float maxWaitTime = (float)timeLimit; // 使用设置的时间限制
                    
                    while (!playerHasInput && waitTime < maxWaitTime)
                    {
                        waitTime += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    // AI代理回合，正常延迟
                    yield return new WaitForSeconds(agentActionDelay);
                }
                
                // 选择动作
                object selectedAction = SelectActionSync(agent, (List<object>)actionsCopy, gameStateCopy, agentIndex);
                
                // 额外安全检查：确保selectedAction不为null
                if (selectedAction == null)
                {
                    var actionsList = actionsCopy as List<object>;
                    selectedAction = (actionsList != null && actionsList.Count > 0) ? actionsList[0] : Constants.TIMEOUT_ACTION;
                }
                
                // 验证动作
                if (!ValidateAction(selectedAction, actions, agentIndex))
                {
                    selectedAction = HandleInvalidAction(actions, agentIndex);
                }
                
                // 记录动作
                RecordAction(agentIndex, selectedAction);
                
                // 更新游戏状态
                gameRule.ApplyAction(selectedAction);
                
                // 显示动作执行
                if (displayer != null)
                {
                    displayer.DisplayAvailableActions(agentIndex, (List<object>)actionsCopy);
                    displayer.ExecuteAction(agentIndex, selectedAction, gameRule.CurrentGameState);
                }
                
                // 触发动作执行事件，通知UI更新
                OnActionExecuted?.Invoke(agentIndex, selectedAction);
                
                // 检查警告限制
                if (agentIndex != gameRule.NumOfAgent && warnings.ContainsKey(agentIndex) && warnings[agentIndex] >= warningLimit)
                {
                    EndGame(true, agentIndex);
                    yield break;
                }
                
                actionCounter++;
                yield return null; // 等待一帧
            }
            
            // 游戏正常结束
            EndGame(false);
        }
        
        /// <summary>
        /// 设置玩家输入标志（供外部调用）
        /// </summary>
        public void SetPlayerInputFlag(bool hasInput)
        {            
            playerHasInput = hasInput;
        }
        
        /// <summary>
        /// 设置玩家选择的action（供外部调用）
        /// </summary>
        public void SetPlayerAction(object action)
        {            
            playerSelectedAction = action;
            playerHasInput = true;
        }
        
        /// <summary>
        /// 判断指定索引的agent是否为人类玩家
        /// </summary>
        private bool IsHumanAgent(int agentIndex)
        {
            if (agentIndex < 0 || agentIndex >= agents.Count)
                return false;
                
            var agent = agents[agentIndex];
            if (agent == null)
                return false;
                
            // 检查agent类型是否为HumanAgent
            return agent.GetType().Name == "HumanAgent";
        }
        
        /// <summary>
        /// 同步选择动作 - 修复版本，确保永不返回null
        /// </summary>
        private object SelectActionSync(BaseAgent agent, List<object> actions, object gameState, int agentIndex)
        {
            // 安全检查
            if (agent == null)
            {
                return Constants.TIMEOUT_ACTION;
            }
            
            if (actions == null || actions.Count == 0)
            {
                return Constants.TIMEOUT_ACTION;
            }
            
            if (interactive && IsHumanAgent(agentIndex))
            {
                // 检查玩家是否已经输入
                if (playerHasInput && playerSelectedAction != null)
                {                    
                    // 重置玩家输入状态
                    playerHasInput = false;
                    var action = playerSelectedAction;
                    playerSelectedAction = null;
                    
                    return action;
                }
                
                // 如果玩家还没有输入，尝试通过displayer获取
                if (displayer != null)
                {
                    try
                    {
                        displayer.DisplayState(gameRule.CurrentGameState);
                        object userInput = displayer.UserInput(actions);
                        
                        // 检查用户输入是否有效
                        if (userInput == null)
                        {
                            return actions[0]; // 返回第一个可用动作
                        }
                        
                        return userInput;
                    }
                    catch (System.Exception e)
                    {
                        return actions[0]; // 返回第一个可用动作
                    }
                }
                else
                {
                    // 如果displayer为null，使用默认动作
                    return actions[0];
                }
            }
            else
            {
                try
                {
                    // 直接调用代理的选择方法，不使用异步
                    object selectedAction = agent.SelectAction(actions, gameState);
                    
                    // 检查AI代理的选择是否有效
                    if (selectedAction == null)
                    {
                        return actions[0]; // 返回第一个可用动作
                    }
                    
                    return selectedAction;
                }
                catch (System.Exception e)
                {
                    return Constants.TIMEOUT_ACTION;
                }
            }
        }
        
        /// <summary>
        /// 验证动作有效性 - 修复版本
        /// </summary>
        private bool ValidateAction(object selectedAction, List<object> actions, int agentIndex)
        {
            // 安全检查
            if (gameRule == null)
            {
                return false;
            }
            
            if (selectedAction == null)
            {
                return false;
            }
            
            if (actions == null || actions.Count == 0)
            {
                return false;
            }
            
            if (selectedAction.Equals(Constants.TIMEOUT_ACTION))
                return false;
                
            try
            {
                return gameRule.ValidAction(selectedAction, actions);
            }
            catch (System.Exception e)
            {
                return false;
            }
        }
        
        /// <summary>
        /// 处理无效动作
        /// </summary>
        private object HandleInvalidAction(List<object> actions, int agentIndex)
        {
            // 确保warnings字典中有这个agentIndex的键
            if (!warnings.ContainsKey(agentIndex))
            {
                warnings[agentIndex] = 0;
            }
            warnings[agentIndex]++;
            warningPositions.Add((agentIndex, actionCounter));
            
            if (displayer != null)
            {
                displayer.IllegalWarning(this, agentIndex);
                OnIllegalWarning?.Invoke(agentIndex);
            }
            
            // 随机选择一个合法动作
            var rand = new System.Random();
            return actions[rand.Next(actions.Count)];
        }
        
        /// <summary>
        /// 记录动作
        /// </summary>
        private void RecordAction(int agentIndex, object action)
        {
            var actionRecord = new Dictionary<string, object>
            {
                ["agent_id"] = agentIndex,
                ["action"] = action
            };
            
            ((List<Dictionary<string, object>>)gameHistory["actions"]).Add(
                new Dictionary<string, object> { [actionCounter.ToString()] = actionRecord }
            );
        }
        
        /// <summary>
        /// 处理私有信息
        /// </summary>
        private void ProcessPrivateInformation(object gameState, int currentAgentIndex)
        {
            // 对于Splendor游戏，暂时跳过私有信息处理
            // 这里可以根据具体游戏需求实现
        }
        
        /// <summary>
        /// 获取虚拟代理
        /// </summary>
        private BaseAgent GetDummyAgent()
        {
            // 返回一个虚拟代理，用于处理超出范围的代理索引
            return new DummyAgent(gameRule.NumOfAgent);
        }
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        private void EndGame(bool isTimeout = false, int? timeoutAgentId = null)
        {
            if (enableDebugLog)
                Debug.Log($"GameManager.EndGame(): 游戏结束被调用，isTimeout: {isTimeout}, timeoutAgentId: {timeoutAgentId}");
            
            gameHistory["seed"] = seed;
            gameHistory["num_of_agent"] = gameRule.NumOfAgent;
            gameHistory["agents_namelist"] = agentsNamelist;
            gameHistory["warning_positions"] = warningPositions;
            gameHistory["warning_limit"] = warningLimit;
            
            var scores = new Dictionary<int, double>();
            int winnerPlayerId = -1;
            double highestScore = -1;
            
            for (int i = 0; i < gameRule.NumOfAgent; i++)
            {
                if (isTimeout && timeoutAgentId.HasValue && i == timeoutAgentId.Value)
                {
                    scores[i] = -1;
                }
                else
                {
                    scores[i] = gameRule.CalScore(gameRule.CurrentGameState, i);
                    if (scores[i] > highestScore)
                    {
                        highestScore = scores[i];
                        winnerPlayerId = i + 1; // 转换为1-based索引
                    }
                }
            }
            gameHistory["scores"] = scores;
            
            if (displayer != null)
            {
                displayer.EndGame(gameRule.CurrentGameState, scores.Values.ToList());
            }
            
            // 触发游戏结束事件，让GameStateListener处理EndGameCanvas显示
            if (enableDebugLog)
                Debug.Log($"GameManager.EndGame(): 准备触发OnGameEnded事件");
            
            OnGameEnded?.Invoke(this);
            
            if (enableDebugLog)
                Debug.Log($"GameManager.EndGame(): OnGameEnded事件已触发");
        }
        

        
        /// <summary>
        /// 深度复制对象
        /// </summary>
        private object DeepCopy(object obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();

            // 基本类型和字符串直接返回
            if (type.IsPrimitive || type == typeof(string))
                return obj;

            // List
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var newList = (System.Collections.IList)Activator.CreateInstance(type);
                foreach (var item in (System.Collections.IEnumerable)obj)
                {
                    newList.Add(DeepCopy(item));
                }
                return newList;
            }

            // Dictionary
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var newDict = (System.Collections.IDictionary)Activator.CreateInstance(type);
                foreach (System.Collections.DictionaryEntry kvp in (System.Collections.IDictionary)obj)
                {
                    newDict.Add(DeepCopy(kvp.Key), DeepCopy(kvp.Value));
                }
                return newDict;
            }

            // Array
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType != null)
                {
                    var array = (Array)obj;
                    var newArray = Array.CreateInstance(elementType, array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        var value = array.GetValue(i);
                        if (value != null)
                        {
                            newArray.SetValue(DeepCopy(value), i);
                        }
                    }
                    return newArray;
                }
            }

            // 对于特定类型，使用其DeepCopy方法
            if (obj is IDeepCopyable deepCopyable)
            {
                return deepCopyable.DeepCopy();
            }

            // 对于其他复杂对象，暂时返回原对象（浅复制）
            return obj;
        }
    }
    
    /// <summary>
    /// 深度复制接口
    /// </summary>
    public interface IDeepCopyable
    {
        object DeepCopy();
    }
    
    /// <summary>
    /// 虚拟代理类
    /// </summary>
    public class DummyAgent : BaseAgent
    {
        public DummyAgent(int agentId)
        {
            id = agentId;
        }

        public override object SelectAction(List<object> actions, object gameState)
        {
            var rand = new System.Random();
            return actions[rand.Next(actions.Count)];
        }
    }
}