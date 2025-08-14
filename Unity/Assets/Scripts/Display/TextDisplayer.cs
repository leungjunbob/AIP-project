using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SplendorUnity.Core;
using SplendorUnity.Utils;

namespace SplendorUnity.Display
{
    /// <summary>
    /// 文本显示器，替代原TextDisplayer
    /// 处理控制台输出和文件日志
    /// </summary>
    public class TextDisplayer : MonoBehaviour
    {
        [Header("日志配置")]
        public bool enableFileLogging = true;
        public string logFileName;
        
        [Header("显示配置")]
        public bool enableConsoleOutput = true;
        public bool enableDetailedLogging = true;
        
        // 私有字段
        private int roundNumber = 0;
        private Dictionary<int, SplendorGameState.AgentState> previousStates = new Dictionary<int, SplendorGameState.AgentState>();
        private StreamWriter logWriter;
        private List<string> gameLog = new List<string>();
        
        private void Awake()
        {
            InitializeLogging();
        }
        
        private void OnDestroy()
        {
            CloseLogging();
        }
        
        /// <summary>
        /// 初始化日志系统
        /// </summary>
        private void InitializeLogging()
        {
            if (enableFileLogging)
            {
                logFileName = $"splendor_game_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logPath = Path.Combine(Application.persistentDataPath, logFileName);
                logWriter = new StreamWriter(logPath, false, System.Text.Encoding.UTF8);
                
                LogMessage("--------------------------------------------------------------------");
                LogMessage($"游戏开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
        }
        
        /// <summary>
        /// 关闭日志系统
        /// </summary>
        private void CloseLogging()
        {
            if (logWriter != null)
            {
                logWriter.Close();
                logWriter = null;
            }
        }
        
        /// <summary>
        /// 记录消息
        /// </summary>
        private void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}";
            
            gameLog.Add(logMessage);
            
            if (enableFileLogging && logWriter != null)
            {
                logWriter.WriteLine(logMessage);
                logWriter.Flush();
            }
        }
        
        /// <summary>
        /// 初始化显示器
        /// </summary>
        public virtual void InitDisplayer(object runner)
        {
            LogMessage("------------------------GAME STARTED--------------------------------");
            LogMessage("开始游戏...");
        }
        
        /// <summary>
        /// 用户输入
        /// </summary>
        public virtual object UserInput(List<object> actions)
        {
            var actionDict = new Dictionary<int, object>();
            int counter = 0;
            
            LogMessage("\n--- 可用动作 ---");
            foreach (var action in actions)
            {
                actionDict[counter] = action;
                LogMessage($"{counter}: {action}");
                counter++;
            }
            
            // 在Unity中，这里应该通过UI获取用户输入
            // 目前返回第一个动作作为默认值
            LogMessage("用户输入功能需要UI实现");
            return actionDict[0];
        }
        
        /// <summary>
        /// 检查是否有用户输入
        /// </summary>
        public virtual bool HasUserInput()
        {
            // TextDisplayer总是有默认输入（返回第一个动作）
            return true;
        }
        
        /// <summary>
        /// 开始回合
        /// </summary>
        public virtual void StartRound(object gameState)
        {
            roundNumber++;
            LogMessage($"\n=== 回合 {roundNumber} ===");
        }
        
        /// <summary>
        /// 显示游戏状态
        /// </summary>
        public virtual void DisplayState(object gameState)
        {
            LogMessage("------------------------GAME STATE----------------------------------");
            LogMessage(gameState.ToString());
            LogMessage("--------------------------------------------------------------------");
        }
        
        /// <summary>
        /// 显示可用动作
        /// </summary>
        public virtual void DisplayAvailableActions(int agentId, List<object> actions)
        {
            LogMessage($"\n--- Agent {agentId} 的可用动作 ---");
            
            // 优化显示：合并相同卡牌的多个 reserve 动作
            var groupedActions = new Dictionary<string, List<(int index, SplendorUnity.Core.Action action)>>();
            
            for (int i = 0; i < actions.Count; i++)
            {
                var action = (SplendorUnity.Core.Action)actions[i];
                
                if (action.Type == "reserve" && action.Card != null)
                {
                    // 为 reserve 动作创建唯一键
                    var key = $"reserve_{action.Card.Code}";
                    if (!groupedActions.ContainsKey(key))
                    {
                        groupedActions[key] = new List<(int index, SplendorUnity.Core.Action action)>();
                    }
                    groupedActions[key].Add((i, action));
                }
                else
                {
                    // 非 reserve 动作直接显示
                    LogMessage($"{i}: {GameData.ActionToString(agentId, action)}");
                }
            }
            
            // 显示合并后的 reserve 动作
            foreach (var group in groupedActions)
            {
                var actionsInGroup = group.Value;
                if (actionsInGroup.Count == 1)
                {
                    // 只有一个动作，直接显示
                    var (index, action) = actionsInGroup[0];
                    LogMessage($"{index}: {GameData.ActionToString(agentId, action)}");
                }
                else
                {
                    // 多个动作，合并显示
                    var firstAction = actionsInGroup[0].action;
                    var card = firstAction.Card;
                    var returnedGemsList = new List<string>();
                    
                    foreach (var (index, action) in actionsInGroup)
                    {
                        if (action.ReturnedGems.Count > 0)
                        {
                            returnedGemsList.Add(GameData.GemsToString(action.ReturnedGems));
                        }
                    }
                    
                    if (returnedGemsList.Count > 0)
                    {
                        // 去重并排序
                        returnedGemsList = returnedGemsList.Distinct().OrderBy(x => x).ToList();
                        LogMessage($"{actionsInGroup[0].index}-{actionsInGroup[actionsInGroup.Count - 1].index}: Agent {agentId} reserved a Tier {card.DeckId + 1} {card.Colour} card ({card.Code}) with return options: {string.Join(", ", returnedGemsList)}");
                    }
                    else
                    {
                        LogMessage($"{actionsInGroup[0].index}-{actionsInGroup[actionsInGroup.Count - 1].index}: Agent {agentId} reserved a Tier {card.DeckId + 1} {card.Colour} card ({card.Code})");
                    }
                }
            }
            
            LogMessage("--- 可用动作结束 ---\n");
        }
        
        /// <summary>
        /// 执行动作
        /// </summary>
        public virtual void ExecuteAction(int agentId, object move, object gameState)
        {
            var splendorState = (SplendorGameState)gameState;
            var playerState = splendorState.agents[agentId];
            
            LogMessage($"\n=== Agent {agentId} 的行动 ===");
            LogMessage($"回合: {roundNumber}");
            
            // 显示选择的动作
            LogMessage($"选择的动作: {GameData.ActionToString(agentId, (SplendorUnity.Core.Action)move)}");
            
            // 显示动作的详细信息
            DisplayActionDetails((SplendorUnity.Core.Action)move);
            
            // 显示状态变化
            DisplayStateChanges(agentId, playerState);
            
            LogMessage("\n------------------------State After Action----------------------------------");
            DisplayDetailedState(gameState);
        }
        
        /// <summary>
        /// 显示动作详情
        /// </summary>
        private void DisplayActionDetails(SplendorUnity.Core.Action action)
        {
            LogMessage("\n--- 动作详情 ---");
            LogMessage($"动作类型: {action.Type}");
            
            if (action.CollectedGems.Count > 0)
            {
                LogMessage($"收集的宝石: {GameData.GemsToString(action.CollectedGems)}");
            }
            
            if (action.ReturnedGems.Count > 0)
            {
                LogMessage($"归还的宝石: {GameData.GemsToString(action.ReturnedGems)}");
            }
            
            if (action.Card != null)
            {
                var card = action.Card;
                LogMessage($"相关卡牌: Tier {card.DeckId + 1} {card.Colour} 卡牌");
                LogMessage($"  代码: {card.Code}");
                LogMessage($"  分数: {card.Points}");
                LogMessage($"  成本: {GameData.GemsToString(card.Cost)}");
            }
            
            if (action.Noble != null)
            {
                LogMessage($"贵族访问: {action.Noble.Code}");
            }
        }
        
        /// <summary>
        /// 显示状态变化
        /// </summary>
        private void DisplayStateChanges(int agentId, SplendorGameState.AgentState currentState)
        {
            LogMessage("\n--- 状态变化 ---");
            
            if (previousStates.ContainsKey(agentId))
            {
                var previousState = previousStates[agentId];
                
                // 显示分数变化
                if (currentState.score != previousState.score)
                {
                    int scoreChange = currentState.score - previousState.score;
                    LogMessage($"分数变化: {previousState.score} → {currentState.score} ({scoreChange:+0;-0})");
                }
                
                // 显示宝石变化
                var gemChanges = new List<string>();
                foreach (var kvp in currentState.gems)
                {
                    var color = kvp.Key;
                    var currentCount = kvp.Value;
                    var previousCount = previousState.gems.ContainsKey(color) ? previousState.gems[color] : 0;
                    if (currentCount != previousCount)
                    {
                        int change = currentCount - previousCount;
                        gemChanges.Add($"{color}: {previousCount} → {currentCount} ({change:+0;-0})");
                    }
                }
                if (gemChanges.Count > 0)
                {
                    LogMessage($"宝石变化: {string.Join(", ", gemChanges)}");
                }
                
                // 显示卡牌变化
                foreach (var kvp in currentState.cards)
                {
                    var color = kvp.Key;
                    var currentCards = kvp.Value;
                    var previousCards = previousState.cards.ContainsKey(color) ? previousState.cards[color] : new List<SplendorUnity.Models.Card>();
                    
                    if (currentCards.Count != previousCards.Count)
                    {
                        LogMessage($"卡牌变化 ({color}): {previousCards.Count} → {currentCards.Count} (+{currentCards.Count - previousCards.Count})");
                        
                        // 显示新增的卡牌
                        var newCards = currentCards.Where(c => !previousCards.Any(pc => pc.Code == c.Code)).ToList();
                        if (newCards.Count > 0)
                        {
                            LogMessage($"  新增卡牌: {string.Join(", ", newCards.Select(c => $"{c.Code}({c.Points}分)"))}");
                        }
                    }
                }
                
                // 显示贵族变化
                if (currentState.nobles.Count != previousState.nobles.Count)
                {
                    LogMessage($"贵族变化: {previousState.nobles.Count} → {currentState.nobles.Count} (+{currentState.nobles.Count - previousState.nobles.Count})");
                    var newNobles = currentState.nobles.Where(n => !previousState.nobles.Any(pn => pn.Code == n.Code)).ToList();
                    if (newNobles.Count > 0)
                    {
                        LogMessage($"  新增贵族: {string.Join(", ", newNobles.Select(n => n.Code))}");
                    }
                }
            }
            else
            {
                LogMessage("首次行动，无状态变化");
            }
            
            // 保存当前状态作为下次比较的基准
            previousStates[agentId] = currentState.DeepCopy();
        }
        
        /// <summary>
        /// 显示详细状态
        /// </summary>
        private void DisplayDetailedState(object gameState)
        {
            var splendorState = (SplendorGameState)gameState;
            
            LogMessage("------------------------GAME STATE----------------------------------");
            
            // 显示可用宝石
            LogMessage("Available Gems:");
            LogMessage(string.Join(", ", splendorState.board.gems.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
            
            // 显示已发卡牌
            LogMessage("Dealt Card List:");
            foreach (var card in splendorState.board.DealtList())
            {
                if (card != null)
                {
                    LogMessage($"\tTier {card.DeckId + 1} {card.Colour} card worth {card.Points} points and costing {GameData.GemsToString(card.Cost)}");
                }
            }
            
            // 显示贵族
            LogMessage("Noble List:");
            LogMessage(string.Join(", ", splendorState.board.nobles.Select(n => n.Code)));
            
            // 显示每个代理的详细状态
            foreach (var agent in splendorState.agents)
            {
                LogMessage($"Agent ({agent.id}):");
                LogMessage($"\tscore: {agent.score},");
                LogMessage($"\tgems: {string.Join(", ", agent.gems.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
                LogMessage($"\tcards: {string.Join(", ", agent.cards.Select(kvp => $"{kvp.Key}: {kvp.Value.Count}"))}");
                LogMessage($"\tnobles: {agent.nobles.Count}.");
            }
            
            LogMessage("--------------------------------------------------------------------");
        }
        
        /// <summary>
        /// 非法动作警告
        /// </summary>
        public virtual void IllegalWarning(object runner, int agentId)
        {
            LogMessage($"⚠️  Agent {agentId} 选择了非法动作!\n");
        }
        
        /// <summary>
        /// 超时警告
        /// </summary>
        public virtual void TimeOutWarning(object runner, int agentId)
        {
            LogMessage($"⏰  Agent {agentId} 超时!\n");
        }
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        public virtual void EndGame(object gameState, List<double> scores)
        {
            LogMessage("\n🎉 GAME HAS ENDED 🎉");
            LogMessage("--------------------------------------------------------------------");
            var splendorState = (SplendorGameState)gameState;
            
            // 显示最终分数
            for (int i = 0; i < splendorState.agents.Count; i++)
            {
                var playerState = splendorState.agents[i];
                LogMessage($"Score for Agent {playerState.id}: {playerState.score}");
            }
            
            // 显示获胜者
            var maxScore = scores.Max();
            var winners = scores.Select((score, index) => new { score, index }).Where(x => x.score == maxScore).ToList();
            
            if (winners.Count == 1)
            {
                LogMessage($"🏆 获胜者: Agent {winners[0].index} (分数: {winners[0].score})");
            }
            else
            {
                LogMessage($"🤝 平局! 获胜者: {string.Join(", ", winners.Select(w => $"Agent {w.index}"))} (分数: {winners[0].score})");
            }
            
            LogMessage($"总回合数: {roundNumber}");
            LogMessage($"游戏结束时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            if (enableFileLogging)
            {
                LogMessage($"日志文件: {logFileName}");
                LogMessage($"日志路径: {Path.Combine(Application.persistentDataPath, logFileName)}");
            }
            
            CloseLogging();
        }
        
        /// <summary>
        /// 获取游戏日志
        /// </summary>
        public List<string> GetGameLog()
        {
            return new List<string>(gameLog);
        }
        
        /// <summary>
        /// 清空日志
        /// </summary>
        public void ClearLog()
        {
            gameLog.Clear();
        }
    }
}