// INFORMATION ------------------------------------------------------------------------------------------------------- //
// Author:  Steven Spratley, extending code by Guang Ho and Michelle Blom
// Date:    04/01/2021
// Purpose: Implements "Splendor" for the COMP90054 competitive game environment

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; // Added for StreamWriter

namespace Splendor
{
    // 文本显示器类
    public class TextDisplayer : Displayer
    {
        private int roundNumber = 0;
        private Dictionary<int, SplendorState.AgentState> previousStates = new Dictionary<int, SplendorState.AgentState>();
        private StreamWriter logWriter;
        private string logFileName;

        public TextDisplayer()
        {
            Console.WriteLine("--------------------------------------------------------------------");
            
            // 创建日志文件
            logFileName = $"splendor_game_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            logWriter = new StreamWriter(logFileName, false, System.Text.Encoding.UTF8);
            LogMessage("--------------------------------------------------------------------");
            LogMessage($"游戏开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        private void LogMessage(string message)
        {
            Console.WriteLine(message);
            logWriter.WriteLine(message);
            logWriter.Flush(); // 立即写入文件
        }

        public override void InitDisplayer(Runner runner)
        {
            LogMessage("------------------------GAME STARTED--------------------------------");
            LogMessage("开始游戏...");
        }

        public override object UserInput(List<object> actions)
        {
            var actionDict = new Dictionary<int, object>();
            int counter = 0;
            
            foreach (var action in actions)
            {
                actionDict[counter] = action;
                LogMessage($"{counter}: {action}");
                counter++;
            }
            
            try
            {
                Console.Write($"Please input your choice between 0 and {counter - 1}: ");
                string userInput = Console.ReadLine();
                int userInt = int.Parse(userInput);
                return actionDict[userInt];
            }
            catch (FormatException)
            {
                LogMessage("That's not an integer. Please try again.");
                return actionDict[0]; // 默认返回第一个动作
            }
        }

        public override void StartRound(GameState gameState)
        {
            roundNumber++;
            LogMessage($"\n=== 回合 {roundNumber} ===");
        }

        public override void DisplayState(GameState gameState)
        {
            LogMessage("------------------------GAME STATE----------------------------------");
            LogMessage(gameState.ToString());
            LogMessage("--------------------------------------------------------------------");
        }

        public override void DisplayAvailableActions(int agentId, List<object> actions)
        {
            LogMessage($"\n--- Agent {agentId} 的可用动作 ---");
            
            // 优化显示：合并相同卡牌的多个 reserve 动作
            var groupedActions = new Dictionary<string, List<(int index, Action action)>>();
            
            for (int i = 0; i < actions.Count; i++)
            {
                var action = (Action)actions[i];
                
                if (action.Type == "reserve" && action.Card != null)
                {
                    // 为 reserve 动作创建唯一键
                    var key = $"reserve_{action.Card.Code}";
                    if (!groupedActions.ContainsKey(key))
                    {
                        groupedActions[key] = new List<(int index, Action action)>();
                    }
                    groupedActions[key].Add((i, action));
                }
                else
                {
                    // 非 reserve 动作直接显示
                    LogMessage($"{i}: {SplendorUtils.ActionToString(agentId, action)}");
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
                    LogMessage($"{index}: {SplendorUtils.ActionToString(agentId, action)}");
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
                            returnedGemsList.Add(SplendorUtils.GemsToString(action.ReturnedGems));
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

        public override void ExecuteAction(int agentId, object move, GameState gameState)
        {
            var splendorState = (SplendorState)gameState;
            var playerState = splendorState.Agents[agentId];
            
            LogMessage($"\n=== Agent {agentId} 的行动 ===");
            LogMessage($"回合: {roundNumber}");
            
            // 显示选择的动作
            LogMessage($"选择的动作: {SplendorUtils.ActionToString(agentId, (Action)move)}");
            
            // 显示动作的详细信息
            DisplayActionDetails((Action)move);
            
            // 显示状态变化
            DisplayStateChanges(agentId, playerState);
            
            LogMessage("\n------------------------State After Action----------------------------------");
            DisplayDetailedState(gameState);
        }

        private void DisplayActionDetails(Action action)
        {
            LogMessage("\n--- 动作详情 ---");
            LogMessage($"动作类型: {action.Type}");
            
            if (action.CollectedGems.Count > 0)
            {
                LogMessage($"收集的宝石: {SplendorUtils.GemsToString(action.CollectedGems)}");
            }
            
            if (action.ReturnedGems.Count > 0)
            {
                LogMessage($"归还的宝石: {SplendorUtils.GemsToString(action.ReturnedGems)}");
            }
            
            if (action.Card != null)
            {
                var card = action.Card;
                LogMessage($"相关卡牌: Tier {card.DeckId + 1} {card.Colour} 卡牌");
                LogMessage($"  代码: {card.Code}");
                LogMessage($"  分数: {card.Points}");
                LogMessage($"  成本: {SplendorUtils.GemsToString(card.Cost)}");
            }
            
            if (action.Noble != null)
            {
                LogMessage($"贵族访问: {action.Noble.Item1}");
            }
        }

        private void DisplayStateChanges(int agentId, SplendorState.AgentState currentState)
        {
            LogMessage("\n--- 状态变化 ---");
            
            if (previousStates.ContainsKey(agentId))
            {
                var previousState = previousStates[agentId];
                
                // 显示分数变化
                if (currentState.Score != previousState.Score)
                {
                    int scoreChange = currentState.Score - previousState.Score;
                    LogMessage($"分数变化: {previousState.Score} → {currentState.Score} ({scoreChange:+0;-0})");
                }
                
                // 显示宝石变化
                var gemChanges = new List<string>();
                foreach (var kvp in currentState.Gems)
                {
                    var color = kvp.Key;
                    var currentCount = kvp.Value;
                    var previousCount = previousState.Gems.ContainsKey(color) ? previousState.Gems[color] : 0;
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
                foreach (var kvp in currentState.Cards)
                {
                    var color = kvp.Key;
                    var currentCards = kvp.Value;
                    var previousCards = previousState.Cards.ContainsKey(color) ? previousState.Cards[color] : new List<Card>();
                    
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
                if (currentState.Nobles.Count != previousState.Nobles.Count)
                {
                    LogMessage($"贵族变化: {previousState.Nobles.Count} → {currentState.Nobles.Count} (+{currentState.Nobles.Count - previousState.Nobles.Count})");
                    var newNobles = currentState.Nobles.Where(n => !previousState.Nobles.Any(pn => pn.Item1 == n.Item1)).ToList();
                    if (newNobles.Count > 0)
                    {
                        LogMessage($"  新增贵族: {string.Join(", ", newNobles.Select(n => n.Item1))}");
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

        private void DisplayDetailedState(GameState gameState)
        {
            var splendorState = (SplendorState)gameState;
            
            LogMessage("------------------------GAME STATE----------------------------------");
            
            // 显示可用宝石
            LogMessage("Available Gems:");
            LogMessage(string.Join(", ", splendorState.Board.Gems.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
            
            // 显示已发卡牌
            LogMessage("Dealt Card List:");
            foreach (var card in splendorState.Board.DealtList())
            {
                if (card != null)
                {
                    LogMessage($"\tTier {card.DeckId + 1} {card.Colour} card worth {card.Points} points and costing {SplendorUtils.GemsToString(card.Cost)}");
                }
            }
            
            // 显示贵族
            LogMessage("Noble List:");
            LogMessage(string.Join(", ", splendorState.Board.Nobles.Select(n => n.Item1)));
            
            // 显示每个代理的详细状态
            foreach (var agent in splendorState.Agents)
            {
                LogMessage($"Agent ({agent.Id}):");
                LogMessage($"\tscore: {agent.Score},");
                LogMessage($"\tgems: {string.Join(", ", agent.Gems.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
                LogMessage($"\tcards: {string.Join(", ", agent.Cards.Select(kvp => $"{kvp.Key}: {kvp.Value.Count}"))}");
                LogMessage($"\tnobles: {agent.Nobles.Count}.");
            }
            
            LogMessage("--------------------------------------------------------------------");
        }

        public override void IllegalWarning(Runner runner, int agentId)
        {
            LogMessage($"⚠️  Agent {agentId} 选择了非法动作!\n");
        }

        public override void TimeOutWarning(Runner runner, int agentId)
        {
            LogMessage($"⏰  Agent {agentId} 超时, {runner.Warnings[agentId]} out of {runner.WarningLimit}.\n");
        }

        public override void EndGame(GameState gameState, List<double> scores)
        {
            LogMessage("\n🎉 GAME HAS ENDED 🎉");
            LogMessage("--------------------------------------------------------------------");
            var splendorState = (SplendorState)gameState;
            
            // 显示最终分数
            for (int i = 0; i < splendorState.Agents.Count; i++)
            {
                var playerState = splendorState.Agents[i];
                LogMessage($"Score for Agent {playerState.Id}: {playerState.Score}");
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
            LogMessage($"日志文件: {logFileName}");
            
            // 关闭日志文件
            logWriter.Close();
            Console.WriteLine($"\n游戏日志已保存到: {logFileName}");
        }
    }

    // 简化的GUI显示器类（不依赖Windows Forms）
    public class GUIDisplayer : Displayer
    {
        private bool halfScale;
        private double delay;
        private bool noHighlighting;

        public GUIDisplayer(bool halfScale = false, double delay = 0.1, bool noHighlighting = false)
        {
            this.halfScale = halfScale;
            this.delay = delay;
            this.noHighlighting = noHighlighting;
        }

        public override void InitDisplayer(Runner runner)
        {
            Console.WriteLine("GUI Displayer initialized (simplified version)");
        }

        public override object UserInput(List<object> actions)
        {
            // 简化的用户输入，使用控制台
            var actionDict = new Dictionary<int, object>();
            int counter = 0;
            
            Console.WriteLine("Available actions:");
            foreach (var action in actions)
            {
                actionDict[counter] = action;
                Console.WriteLine($"{counter}: {action}");
                counter++;
            }
            
            try
            {
                Console.Write($"Please input your choice between 0 and {counter - 1}: ");
                string userInput = Console.ReadLine();
                int userInt = int.Parse(userInput);
                return actionDict[userInt];
            }
            catch (FormatException)
            {
                Console.WriteLine("That's not an integer. Please try again.");
                return actionDict[0]; // 默认返回第一个动作
            }
        }

        public override void StartRound(GameState gameState)
        {
            Console.WriteLine("=== New Round Started ===");
        }

        public override void DisplayState(GameState gameState)
        {
            Console.WriteLine("=== Current Game State ===");
            Console.WriteLine(gameState);
        }

        public override void ExecuteAction(int agentId, object move, GameState gameState)
        {
            Console.WriteLine($"Agent {agentId} executed action: {move}");
        }

        public override void TimeOutWarning(Runner runner, int agentId)
        {
            Console.WriteLine($"Agent {agentId} Time Out, {runner.Warnings[agentId]} out of {runner.WarningLimit}.\n");
        }

        public override void EndGame(GameState gameState, List<double> scores)
        {
            Console.WriteLine("=== GAME ENDED ===");
            var splendorState = (SplendorState)gameState;
            foreach (var playerState in splendorState.Agents)
            {
                Console.WriteLine($"Score for Agent {playerState.Id}: {playerState.Score}");
            }
        }
    }

    // 显示辅助类
    public static class DisplayHelper
    {
        // 检查代理是否可以购买卡牌
        public static bool CanBuy(SplendorState.AgentState agent, Card card)
        {
            var gameRule = new SplendorGameRule(2);
            var returnedGems = gameRule.ResourcesSufficient(agent, card.Cost);
            return returnedGems != null;
        }

        // 格式化宝石显示
        public static string FormatGems(Dictionary<string, int> gems)
        {
            return string.Join(", ", gems.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }

        // 格式化卡牌显示
        public static string FormatCards(List<Card> cards)
        {
            return string.Join(", ", cards.Select(card => card.ToString()));
        }
    }
} 