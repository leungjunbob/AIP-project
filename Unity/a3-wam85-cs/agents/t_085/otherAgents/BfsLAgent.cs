// BfsLAgent.cs
// 由 bfs-l.py 完整翻译
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Splendor;
using SplendorAction = Splendor.Action;

namespace agents.t_085.otherAgents
{
    public class BfsLAgent : Agent
    {
        private const double MAX_TIME = 1; // 更激进的时间限制
        private SplendorGameRule gameRule;
        private Stopwatch stopwatch;

        public BfsLAgent(int id) : base(id)
        {
            gameRule = new SplendorGameRule(2);
            stopwatch = new Stopwatch();
        }

        public override object? SelectAction(List<object> actions, GameState gameState)
        {
            if (actions == null || actions.Count == 0)
                return null;

            stopwatch.Restart();
            var bestAction = actions[0]; // 默认选择第一个动作
            var bestScore = double.MinValue;
            
            // 使用启发式BFS搜索
            var queue = new Queue<(SplendorState state, List<SplendorAction> path, double heuristic)>();
            var visited = new HashSet<string>(); // 避免重复状态
            
            queue.Enqueue(((SplendorState)gameState, new List<SplendorAction>(), 0));
            double originScore = gameRule.CalScore((SplendorState)gameState, this.Id);

            int maxIterations = 1000;
            int maxDepth = 10;
            int iterations = 0;
            
            while (queue.Count > 0 && 
                   stopwatch.Elapsed.TotalSeconds < MAX_TIME && 
                   iterations < maxIterations)
            {
                iterations++;
                
                // 每10次迭代检查一次时间
                if (iterations % 10 == 0 && stopwatch.Elapsed.TotalSeconds >= MAX_TIME * 0.8)
                {
                    break;
                }
                
                var (state, currentPath, currentHeuristic) = queue.Dequeue();
                
                // 限制搜索深度
                if (currentPath.Count >= maxDepth)
                {
                    continue;
                }
                
                double score = gameRule.CalScore(state, this.Id);
                double scoreImprovement = score - originScore;

                // 评估当前状态
                if (score >= 15 || scoreImprovement >= 3)
                {
                    if (currentPath.Count > 0)
                    {
                        return currentPath[0];
                    }
                }

                // 启发式评估：优先选择能带来更好分数的动作
                if (scoreImprovement > bestScore)
                {
                    bestScore = scoreImprovement;
                    if (currentPath.Count > 0)
                    {
                        bestAction = currentPath[0];
                    }
                }

                // 获取所有合法动作并按启发式排序
                var legalActions = gameRule.GetLegalActions(state, this.Id);
                var sortedActions = SortActionsByHeuristic(state, legalActions.Cast<SplendorAction>().ToList());
                
                // 只处理前几个最有希望的动作
                foreach (var action in sortedActions.Take(3))
                {
                    var nextState = (SplendorState)gameRule.GenerateSuccessor(state, action, this.Id);
                    var nextPath = new List<SplendorAction>(currentPath) { action };
                    
                    // 生成状态哈希以避免重复
                    var stateHash = GenerateStateHash(nextState);
                    if (!visited.Contains(stateHash))
                    {
                        visited.Add(stateHash);
                        var nextHeuristic = CalculateHeuristic(nextState, action);
                        queue.Enqueue((nextState, nextPath, nextHeuristic));
                    }
                }
            }

            return bestAction;
        }

        private bool IfCanBuy(SplendorState gameState)
        {
            var actions = gameRule.GetLegalActions(gameState, this.Id);
            return actions.Any(action => ((SplendorAction)action).Type == "buy_available");
        }

        private (List<SplendorAction> actions, string actionType) GemAction(SplendorState gameState)
        {
            var actions = gameRule.GetLegalActions(gameState, this.Id);
            var gemActions = actions.Where(action => 
                ((SplendorAction)action).Type == "collect_diff" || ((SplendorAction)action).Type == "collect_same").Cast<SplendorAction>().ToList();

            return (gemActions, "gem");
        }

        private (List<SplendorAction> actions, string actionType) GreedyAction(SplendorState gameState, List<SplendorAction> actions)
        {
            var reserveActions = actions.Where(action => action.Type == "reserve").ToList();
            int totalGem = gameState.Agents[this.Id].Gems.Values.Sum();
            var buyActions = actions.Where(action => 
                action.Type == "buy_available" || action.Type == "buy_reserve").ToList();

            if (buyActions.Count > 0)
                return (buyActions, "buy");

            if (totalGem <= 8)
                return GemAction(gameState);
            else if (reserveActions.Count > 0)
                return (reserveActions, "reserve");
            else
                return GemAction(gameState);
        }

        private List<SplendorAction> SortActionsByHeuristic(SplendorState state, List<SplendorAction> actions)
        {
            var actionScores = new List<(SplendorAction action, double score)>();
            
            foreach (var action in actions)
            {
                double score = CalculateHeuristic(state, action);
                actionScores.Add((action, score));
            }
            
            return actionScores.OrderByDescending(x => x.score).Select(x => x.action).ToList();
        }

        private double CalculateHeuristic(SplendorState state, SplendorAction action)
        {
            double score = 0;
            
            switch (action.Type)
            {
                case "buy_available":
                case "buy_reserve":
                    if (action.Card != null)
                    {
                        score += action.Card.Points * 10; // 购买卡片的点数权重很高
                        score += action.Card.Points * 2; // 额外奖励
                    }
                    break;
                    
                case "reserve":
                    if (action.Card != null)
                    {
                        score += action.Card.Points * 1; // 保留卡片的点数权重
                        score += 2; // 保留动作的基础分数
                    }
                    break;
                    
                case "collect_diff":
                    score += 3; // 收集不同颜色宝石的基础分数
                    break;
                    
                case "collect_same":
                    score += 2; // 收集相同颜色宝石的基础分数
                    break;
            }
            
            return score;
        }

        private string GenerateStateHash(SplendorState state)
        {
            // 简单的状态哈希，用于避免重复状态
            var agent = state.Agents[this.Id];
            var gems = string.Join(",", agent.Gems.Values);
            var cards = string.Join(",", agent.Cards.Values.Select(cards => cards.Count));
            return $"{gems}|{cards}";
        }
    }
} 