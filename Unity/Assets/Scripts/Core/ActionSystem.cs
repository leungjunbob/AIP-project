using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SplendorUnity.Models;
using SplendorUnity.Utils;

namespace SplendorUnity.Core
{
    /// <summary>
    /// 动作系统，管理游戏中的各种动作
    /// </summary>
    public class ActionSystem : MonoBehaviour
    {
        [Header("动作系统配置")]
        public SplendorGameRule gameRule;
        
        /// <summary>
        /// 执行动作
        /// </summary>
        public void ExecuteAction(Action action, SplendorGameState gameState, int agentId)
        {
            var agent = gameState.agents[agentId];
            var board = gameState.board;
            
            // 记录动作
            agent.lastAction = action;
            int score = 0;

            // 处理收集宝石或保留卡牌
            if (action.Type.Contains("collect") || action.Type == "reserve")
            {
                ProcessGemCollection(action, agent, board);
                
                if (action.Type == "reserve")
                {
                    ProcessCardReservation(action, agent, board);
                }
            }
            // 处理购买卡牌
            else if (action.Type.Contains("buy"))
            {
                ProcessCardPurchase(action, agent, board, ref score);
            }

            // 处理贵族访问
            if (action.Noble != null)
            {
                ProcessNobleVisit(action, agent, board, ref score);
            }

            // 更新分数和状态
            agent.agentTrace.ActionReward.Add(new System.Tuple<SplendorUnity.Core.Action, int>(action, score));
            agent.score += score;
            agent.passed = action.Type == "pass";
            
            // 更新游戏状态
            gameState.agentToMove = (gameState.agentToMove + 1) % gameState.agents.Count;
        }
        
        /// <summary>
        /// 处理宝石收集
        /// </summary>
        private void ProcessGemCollection(Action action, SplendorGameState.AgentState agent, SplendorGameState.BoardState board)
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
        }
        
        /// <summary>
        /// 处理卡牌保留
        /// </summary>
        private void ProcessCardReservation(Action action, SplendorGameState.AgentState agent, SplendorGameState.BoardState board)
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
        
        /// <summary>
        /// 处理卡牌购买
        /// </summary>
        private void ProcessCardPurchase(Action action, SplendorGameState.AgentState agent, SplendorGameState.BoardState board, ref int score)
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
        
        /// <summary>
        /// 处理贵族访问
        /// </summary>
        private void ProcessNobleVisit(Action action, SplendorGameState.AgentState agent, SplendorGameState.BoardState board, ref int score)
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
        
        /// <summary>
        /// 验证动作是否合法
        /// </summary>
        public bool ValidateAction(SplendorUnity.Core.Action action, List<object> legalActions)
        {
            return legalActions.Contains(action);
        }
        
        /// <summary>
        /// 获取动作描述
        /// </summary>
        public string GetActionDescription(SplendorUnity.Core.Action action, int agentId)
        {
            return GameData.ActionToString(agentId, action);
        }
        
        /// <summary>
        /// 检查动作是否会导致游戏结束
        /// </summary>
        public bool WillEndGame(SplendorUnity.Core.Action action, SplendorGameState gameState, int agentId)
        {
            // 检查是否有玩家达到15分
            var agent = gameState.agents[agentId];
            int newScore = agent.score;
            
            if (action.Card != null)
            {
                newScore += action.Card.Points;
            }
            
            if (action.Noble != null)
            {
                newScore += 3;
            }
            
            return newScore >= 15;
        }
        
        /// <summary>
        /// 获取动作的预期分数
        /// </summary>
        public int GetActionScore(SplendorUnity.Core.Action action)
        {
            int score = 0;
            
            if (action.Card != null)
            {
                score += action.Card.Points;
            }
            
            if (action.Noble != null)
            {
                score += 3;
            }
            
            return score;
        }
    }
}