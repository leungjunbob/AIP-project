using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using SplendorUnity.Models;
using SplendorUnity.Utils;

namespace SplendorUnity.Core
{
    /// <summary>
    /// Splendor游戏规则，替代原SplendorGameRule
    /// 处理动作验证、状态转换、分数计算、游戏结束判断
    /// </summary>
    public class SplendorGameRule : MonoBehaviour
    {
        [Header("游戏规则配置")]
        public int numOfAgent = 2;
        public int currentAgentIndex = 0;
        public int actionCounter = 0;
        public bool enableDebugLog = true;
        
        [Header("游戏状态")]
        public SplendorGameState currentGameState;
        
        [Header("私有信息配置")]
        public List<string> privateInformation;
        
        /// <summary>
        /// 获取当前代理索引
        /// </summary>
        public int GetCurrentAgentIndex()
        {
            return currentAgentIndex;
        }
        
        /// <summary>
        /// 获取代理数量
        /// </summary>
        public int NumOfAgent => numOfAgent;
        
        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public object CurrentGameState => currentGameState;
        
        /// <summary>
        /// 获取私有信息配置
        /// </summary>
        public List<string> PrivateInformation => privateInformation;
        
        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        public virtual bool GameEnds()
        {
            var splendorState = currentGameState;
            int deadlock = 0;
            
            // 添加调试日志
            if (enableDebugLog)
            {
                Debug.Log($"SplendorGameRule.GameEnds(): 检查游戏是否结束，agents数量: {splendorState.agents.Count}");
                for (int i = 0; i < splendorState.agents.Count; i++)
                {
                    var agent = splendorState.agents[i];
                    Debug.Log($"SplendorGameRule.GameEnds(): Agent {i} - Score: {agent.score}, Passed: {agent.passed}");
                }
            }
            
            foreach (var agent in splendorState.agents)
            {
                deadlock += agent.passed ? 1 : 0;
                // 任何玩家达到15分都应该结束游戏，不需要检查currentAgentIndex
                if (agent.score >= 15)
                {
                    if (enableDebugLog)
                        Debug.Log($"SplendorGameRule.GameEnds(): 检测到玩家达到15分，游戏结束！");
                    return true;
                }
            }
            
            if (deadlock == splendorState.agents.Count)
            {
                if (enableDebugLog)
                    Debug.Log($"SplendorGameRule.GameEnds(): 所有玩家都passed，游戏结束！");
                return true;
            }
            
            if (enableDebugLog)
                Debug.Log($"SplendorGameRule.GameEnds(): 游戏继续，deadlock: {deadlock}/{splendorState.agents.Count}");
            
            return false;
        }
        
        /// <summary>
        /// 获取合法动作列表
        /// </summary>
        public virtual List<object> GetLegalActions(object gameState, int agentId)
        {
            var actions = new List<object>();
            var splendorState = (SplendorGameState)gameState;
            var agent = splendorState.agents[agentId];
            var board = splendorState.board;

            // 检查是否有贵族等待访问
            var potentialNobles = new List<Noble>();
            foreach (var noble in board.nobles)
            {
                if (NobleVisit(agent, noble))
                {
                    potentialNobles.Add(noble);
                }
            }
            if (potentialNobles.Count == 0)
            {
                potentialNobles.Add(null);
            }

            // 生成收集不同宝石的行动
            var availableColours = board.gems.Where(kvp => kvp.Key != "yellow" && kvp.Value > 0).Select(kvp => kvp.Key).ToList();
            int numHoldingGem = agent.gems.Values.Sum();
            int minCombLen;

            if (numHoldingGem <= 7)
                minCombLen = Math.Min(3, availableColours.Count);
            else if (numHoldingGem == 8)
                minCombLen = Math.Min(2, availableColours.Count);
            else
                minCombLen = Math.Min(1, availableColours.Count);

            for (int comboLength = minCombLen; comboLength <= Math.Min(availableColours.Count, 3); comboLength++)
            {
                var combinations = GetCombinations(availableColours, comboLength);
                foreach (var combo in combinations)
                {
                    var collectedGems = combo.ToDictionary(colour => colour, colour => 1);
                    if (collectedGems.Count > 0)
                    {
                        var returnCombos = GenerateReturnCombos(agent.gems, collectedGems);
                        foreach (var returnedGems in returnCombos)
                        {
                            foreach (var noble in potentialNobles)
                            {
                                actions.Add(Action.CreateCollectAction(collectedGems, returnedGems));
                            }
                        }
                    }
                }
            }

            // 生成收集相同宝石的行动
            var sameColours = board.gems.Where(kvp => kvp.Key != "yellow" && kvp.Value >= 4).Select(kvp => kvp.Key).ToList();
            foreach (var colour in sameColours)
            {
                var collectedGems = new Dictionary<string, int> { { colour, 2 } };
                var returnCombos = GenerateReturnCombos(agent.gems, collectedGems);
                foreach (var returnedGems in returnCombos)
                {
                    foreach (var noble in potentialNobles)
                    {
                        actions.Add(Action.CreateCollectAction(collectedGems, returnedGems));
                    }
                }
            }

            // 生成保留卡牌的行动
            if (agent.cards["yellow"].Count < 3)
            {
                var collectedGems = board.gems["yellow"] > 0 ? new Dictionary<string, int> { { "yellow", 1 } } : new Dictionary<string, int>();
                var returnCombos = GenerateReturnCombos(agent.gems, collectedGems);
                foreach (var returnedGems in returnCombos)
                {
                    foreach (var card in board.DealtList())
                    {
                        if (card != null)
                        {
                                                    foreach (var noble in potentialNobles)
                        {
                            actions.Add(Action.CreateReserveAction(card, collectedGems, returnedGems));
                        }
                        }
                    }
                }
            }

            // 生成购买卡牌的行动
            var availableCards = board.DealtList().ToList();
            var reservedCards = agent.cards["yellow"].ToList();
            var allCards = availableCards.Concat(reservedCards).ToList();
            
            foreach (var card in allCards)
            {
                if (card == null || agent.cards[card.Colour].Count == 7)
                    continue;

                var returnedGems = ResourcesSufficient(agent, card.Cost);
                if (returnedGems != null)
                {
                    // 检查是否有新的贵族可以访问
                    var newNobles = new List<Noble>();
                    foreach (var noble in board.nobles)
                    {
                        var agentPostAction = CloneAgent(agent);
                        agentPostAction.cards[card.Colour].Add(card);
                        if (NobleVisit(agentPostAction, noble))
                        {
                            newNobles.Add(noble);
                        }
                    }
                    if (newNobles.Count == 0)
                    {
                        newNobles.Add(null);
                    }

                    foreach (var noble in newNobles)
                    {
                        bool isReserved = reservedCards.Contains(card);
                        actions.Add(Action.CreateBuyAction(card, isReserved, returnedGems, noble));
                    }
                }
            }

            // 如果没有行动，只能跳过
            if (actions.Count == 0)
            {
                foreach (var noble in potentialNobles)
                {
                    actions.Add(Action.CreatePassAction());
                }
            }

            return actions;
        }
        
        /// <summary>
        /// 更新游戏状态
        /// </summary>
        public virtual void ApplyAction(object actionObj)
        {
            var action = (Action)actionObj;
            var agent = currentGameState.agents[currentAgentIndex];
            var board = currentGameState.board;

            agent.lastAction = action;
            int score = 0;

            if (action.Type.Contains("collect") || action.Type == "reserve")
            {
                // 减少棋盘宝石堆，增加玩家宝石堆
                foreach (var kvp in action.CollectedGems)
                {
                    board.gems[kvp.Key] -= kvp.Value;
                    agent.gems[kvp.Key] += kvp.Value;
                }
                // 减少玩家宝石堆，增加棋盘宝石堆
                foreach (var kvp in action.ReturnedGems)
                {
                    agent.gems[kvp.Key] -= kvp.Value;
                    board.gems[kvp.Key] += kvp.Value;
                }

                if (action.Type == "reserve")
                {
                    // 从已发卡牌中移除卡牌
                    for (int i = 0; i < board.dealt[action.Card.DeckId].Length; i++)
                    {
                        if (board.dealt[action.Card.DeckId][i] != null && 
                            board.dealt[action.Card.DeckId][i].Code == action.Card.Code)
                        {
                            board.dealt[action.Card.DeckId][i] = board.Deal(action.Card.DeckId);
                            agent.cards["yellow"].Add(action.Card);
                            break;
                        }
                    }
                }
            }
            else if (action.Type.Contains("buy"))
            {
                // 减少玩家宝石堆，增加棋盘宝石堆
                foreach (var kvp in action.ReturnedGems)
                {
                    agent.gems[kvp.Key] -= kvp.Value;
                    board.gems[kvp.Key] += kvp.Value;
                }
                
                // 检查卡片是否来自桌面（需要刷新board）还是保留的卡片
                bool isFromBoard = false;
                for (int i = 0; i < board.dealt[action.Card.DeckId].Length; i++)
                {
                    if (board.dealt[action.Card.DeckId][i] != null && 
                        board.dealt[action.Card.DeckId][i].Code == action.Card.Code)
                    {
                        isFromBoard = true;
                        break;
                    }
                }
                
                // 如果购买桌面上的卡牌，设置移除的卡牌位置为新发卡牌
                if (isFromBoard)
                {
                    for (int i = 0; i < board.dealt[action.Card.DeckId].Length; i++)
                    {
                        if (board.dealt[action.Card.DeckId][i] != null && 
                            board.dealt[action.Card.DeckId][i].Code == action.Card.Code)
                        {
                            board.dealt[action.Card.DeckId][i] = board.Deal(action.Card.DeckId);
                            break;
                        }
                    }
                }
                // 否则，代理购买保留的卡牌，从玩家的黄色堆中移除卡牌
                else
                {
                    for (int i = 0; i < agent.cards["yellow"].Count; i++)
                    {
                        if (agent.cards["yellow"][i].Code == action.Card.Code)
                        {
                            agent.cards["yellow"].RemoveAt(i);
                            break;
                        }
                    }
                }

                // 将卡牌添加到玩家匹配颜色的堆中，并相应增加代理分数
                agent.cards[action.Card.Colour].Add(action.Card);
                score += action.Card.Points;
            }

            if (action.Noble != null)
            {
                // 从棋盘移除贵族，添加到玩家堆中
                for (int i = 0; i < board.nobles.Count; i++)
                {
                    if (board.nobles[i].Code == action.Noble.Code)
                    {
                        board.nobles.RemoveAt(i);
                        agent.nobles.Add(action.Noble);
                        score += 3;
                        break;
                    }
                }
            }

            // 记录这一轮的动作和任何结果分数
            agent.agentTrace.ActionReward.Add(new Tuple<Action, int>(action, score));
            agent.score += score;
            agent.passed = action.Type == "pass";
            
            currentAgentIndex = GetNextAgentIndex();
            // 同步更新currentGameState.agentToMove
            if (currentGameState != null)
            {
                currentGameState.agentToMove = currentAgentIndex;
            }
            actionCounter++;
        }
        
        /// <summary>
        /// 计算分数
        /// </summary>
        public virtual double CalScore(object gameState, int agentId)
        {
            var splendorState = (SplendorGameState)gameState;
            double maxScore = 0;
            var details = new List<Tuple<int, int, double>>();

            Func<SplendorGameState.AgentState, int> boughtCards = a => 
                a.cards.Values.Where(cards => cards != a.cards["yellow"]).Sum(cards => cards.Count);

            foreach (var agent in splendorState.agents)
            {
                details.Add(new Tuple<int, int, double>(agent.id, boughtCards(agent), agent.score));
                maxScore = Math.Max(agent.score, maxScore);
            }

            var victors = details.Where(d => d.Item3 == maxScore).ToList();
            if (victors.Count > 1 && victors.Any(d => d.Item1 == agentId))
            {
                int minCards = details.Min(d => d.Item2);
                if (boughtCards(splendorState.agents[agentId]) == minCards)
                {
                    // 如果这个代理是并列胜利者，并且拥有最少的卡牌，加半分
                    return splendorState.agents[agentId].score + 0.5;
                }
            }

            return splendorState.agents[agentId].score;
        }
        
        /// <summary>
        /// 验证动作
        /// </summary>
        public virtual bool ValidAction(object action, List<object> actions)
        {
            return actions.Contains(action);
        }
        
        /// <summary>
        /// 获取下一个代理索引
        /// </summary>
        private int GetNextAgentIndex()
        {
            return (currentAgentIndex + 1) % numOfAgent;
        }
        
        /// <summary>
        /// 初始化游戏规则
        /// </summary>
        public virtual void Initialize(int agentCount)
        {
            numOfAgent = agentCount;
            currentAgentIndex = 0;
            actionCounter = 0;
            
            if (currentGameState == null)
            {
                currentGameState = ScriptableObject.CreateInstance<SplendorGameState>();
            }
            
            currentGameState.Initialize(agentCount);
        }
        
        /// <summary>
        /// 生成可以返回的宝石组合
        /// </summary>
        public List<Dictionary<string, int>> GenerateReturnCombos(Dictionary<string, int> currentGems, Dictionary<string, int> collectedGems)
        {
            int totalGemCount = currentGems.Values.Sum() + collectedGems.Values.Sum();
            if (totalGemCount > 10)
            {
                var returnCombos = new List<Dictionary<string, int>>();
                int numReturn = totalGemCount - 10;

                // 合并当前和收集的宝石，排除刚收集的宝石颜色
                var totalGems = new Dictionary<string, int>();
                foreach (var kvp in currentGems)
                {
                    totalGems[kvp.Key] = kvp.Value;
                }
                foreach (var kvp in collectedGems)
                {
                    if (totalGems.ContainsKey(kvp.Key))
                        totalGems[kvp.Key] += kvp.Value;
                    else
                        totalGems[kvp.Key] = kvp.Value;
                }

                // 过滤掉刚收集的颜色
                var availableGems = totalGems.Where(kvp => !collectedGems.ContainsKey(kvp.Key)).ToList();

                // 形成宝石列表
                var totalGemsList = new List<string>();
                foreach (var kvp in availableGems)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        totalGemsList.Add(kvp.Key);
                    }
                }

                if (totalGemsList.Count < numReturn)
                {
                    return new List<Dictionary<string, int>>();
                }

                // 生成所有有效的返回组合
                var combinations = GetCombinations(totalGemsList, numReturn);
                foreach (var combo in combinations)
                {
                    var returnedGems = new Dictionary<string, int>();
                    foreach (var colour in GameData.COLOURS.Values)
                    {
                        returnedGems[colour] = 0;
                    }

                    foreach (var colour in combo)
                    {
                        returnedGems[colour]++;
                    }

                    // 过滤掉零值的颜色
                    var filteredGems = returnedGems.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    returnCombos.Add(filteredGems);
                }

                return returnCombos;
            }

            return new List<Dictionary<string, int>> { new Dictionary<string, int>() };
        }

        /// <summary>
        /// 获取组合的辅助方法
        /// </summary>
        private List<List<T>> GetCombinations<T>(List<T> list, int k)
        {
            var result = new List<List<T>>();
            GetCombinationsHelper(list, k, 0, new List<T>(), result);
            return result;
        }

        private void GetCombinationsHelper<T>(List<T> list, int k, int start, List<T> current, List<List<T>> result)
        {
            if (current.Count == k)
            {
                result.Add(new List<T>(current));
                return;
            }

            for (int i = start; i < list.Count; i++)
            {
                current.Add(list[i]);
                GetCombinationsHelper(list, k, i + 1, current, result);
                current.RemoveAt(current.Count - 1);
            }
        }

        /// <summary>
        /// 检查资源是否足够
        /// </summary>
        public Dictionary<string, int> ResourcesSufficient(SplendorGameState.AgentState agent, Dictionary<string, int> costs)
        {
            int wild = agent.gems["yellow"];
            var returnCombo = new Dictionary<string, int>();
            foreach (var colour in GameData.COLOURS.Values)
            {
                returnCombo[colour] = 0;
            }

            foreach (var kvp in costs)
            {
                string colour = kvp.Key;
                int cost = kvp.Value;

                // 如果发现短缺，看看差异是否可以用万能/印章/黄色宝石弥补
                int available = agent.gems[colour] + agent.cards[colour].Count;
                int shortfall = Math.Max(cost - available, 0); // 短缺不应该是负数
                wild -= shortfall;
                // 如果万能宝石用完，代理无法购买
                if (wild < 0)
                {
                    return null;
                }
                // 否则，相应地增加return_combo
                int gemCost = Math.Max(cost - agent.cards[colour].Count, 0); // 欠的宝石
                int gemShortfall = Math.Max(gemCost - agent.gems[colour], 0); // 需要的万能宝石
                returnCombo[colour] = gemCost - gemShortfall; // 要归还的彩色宝石
                returnCombo["yellow"] += gemShortfall; // 要归还的万能宝石
            }

            // 过滤掉不必要的颜色并返回指定宝石组合的字典
            var result = returnCombo.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            // 即使没有宝石需要支付，也应该返回空字典而不是null
            // 这样当玩家卡片完全满足需求时，仍然会生成buy action
            return result;
        }

        /// <summary>
        /// 检查贵族是否可以访问
        /// </summary>
        public bool NobleVisit(SplendorGameState.AgentState agent, Noble noble)
        {
            var costs = noble.Requirements;
            foreach (var kvp in costs)
            {
                if (agent.cards[kvp.Key].Count < kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 克隆代理的辅助方法
        /// </summary>
        private SplendorGameState.AgentState CloneAgent(SplendorGameState.AgentState original)
        {
            var clone = new SplendorGameState.AgentState(original.id)
            {
                score = original.score,
                passed = original.passed
            };

            foreach (var kvp in original.gems)
            {
                clone.gems[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in original.cards)
            {
                var newCardList = new List<Card>();
                foreach (var card in kvp.Value)
                {
                    newCardList.Add(card.DeepCopy());
                }
                clone.cards[kvp.Key] = newCardList;
            }

            clone.nobles = new List<Noble>(original.nobles);

            return clone;
        }
    }
}