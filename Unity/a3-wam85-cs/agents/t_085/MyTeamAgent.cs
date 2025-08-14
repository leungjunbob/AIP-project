using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Splendor;
using SplendorAction = Splendor.Action;

namespace agents.t_085
{
    public class PriorityQueue<T>
    {
        private List<(int priority, int count, T item)> heap;
        private int count;

        public PriorityQueue()
        {
            heap = new List<(int priority, int count, T item)>();
            count = 0;
        }

        public void Push(T item, int priority)
        {
            heap.Add((priority, count, item));
            count++;
            heap.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public T Pop()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            
            var item = heap[0];
            heap.RemoveAt(0);
            return item.item;
        }

        public bool IsEmpty()
        {
            return heap.Count == 0;
        }

        public void Update(T item, int priority)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(heap[i].item, item))
                {
                    if (heap[i].priority <= priority)
                        break;
                    
                    heap.RemoveAt(i);
                    Push(item, priority);
                    break;
                }
            }
        }

        public List<T> GetLowestPriorityItems(int n = 5)
        {
            if (IsEmpty())
                return new List<T>();

            var items = new List<T>();
            if (heap.Count == 0)
                return items;

            int lowestPriority = heap[0].priority;
            int threshold = lowestPriority + 4;

            foreach (var (priority, _, item) in heap)
            {
                if (priority <= threshold)
                {
                    items.Add(item);
                }
            }

            // Return top n if there are more than n actions
            if (items.Count > n)
                items = items.Take(n).ToList();

            return items;
        }
    }

    public class MyTeamAgent : Agent
    {
        private const double MAX_TIME = 0.9;
        private const int WIN_SCORE = 15;
        private const int MAX_DEPTH = 15;
        private const int MAX_GEM = 10;
        private const double NEG_INFINITY = double.NegativeInfinity;
        private const double POS_INFINITY = double.PositiveInfinity;
        private const double TIMELIMIT = 0.95;

        // Action types string
        private const string BUY_AVAILABLE = "buy_available";
        private const string BUY_RESERVE = "buy_reserve";
        private const string COLLECT_DIFF = "collect_diff";
        private const string COLLECT_SAME = "collect_same";
        private const string RESERVE = "reserve";

        private const string COLLECTED_GEMS = "collected_gems";
        private const string RETURNED_GEMS = "returned_gems";
        private static readonly string[] GEM_COLOURS = { "black", "red", "yellow", "green", "blue", "white" };

        private SplendorGameRule gameRule;
        private Stopwatch stopwatch;

        public MyTeamAgent(int id) : base(id)
        {
            gameRule = new SplendorGameRule(2);
            stopwatch = new Stopwatch();
        }

        private bool HaveTime()
        {
            return stopwatch.Elapsed.TotalSeconds < MAX_TIME;
        }

        private double EvaluateState(SplendorState state, int depth)
        {
            int opponentId = 1 - Id;

            double scoreSelf = EvaluatePlayerScore(state, Id, depth);
            double scoreOpponent = EvaluatePlayerScore(state, opponentId, depth);

            return scoreSelf - scoreOpponent;
        }

        private double EvaluatePlayerScore(SplendorState state, int agentId, int depth)
        {
            // 1 for a win, -1 for a loss, 0 otherwise
            double winLoss = CalculateWinLoss(state, agentId);

            // points is the score of an agent in the game in this turn, [0, 15, or higher]
            double points = gameRule.CalScore(state, agentId);

            // progress towards nobles this turn
            double nobleProgress = CalculateProgressTowardNobles(state, agentId);

            // number of cards this turn
            int prestige = state.Agents[agentId].Cards.Values.Sum(cards => cards.Count);

            // number of gems this turn
            int gems = state.Agents[agentId].Gems.Values.Sum();

            // Apply decay for each depth increment
            double decayFactor = Math.Pow(0.9, depth);

            return (100 * winLoss + 1.5 * points + 2.5 * nobleProgress + prestige + gems) * decayFactor;
        }

        private List<Dictionary<string, int>> GetAllAgentGemsState(SplendorState gameState)
        {
            var result = new List<Dictionary<string, int>>();
            
            foreach (var agent in gameState.Agents)
            {
                var gemState = new Dictionary<string, int>();
                foreach (var gem in GEM_COLOURS)
                {
                    gemState[gem] = agent.Gems[gem] + (gem != "yellow" ? agent.Cards[gem].Count : 0);
                }
                result.Add(gemState);
            }
            
            return result;
        }

        private List<Dictionary<string, int>> GetNoblesRequirement(SplendorState gameState)
        {
            var noblesRequirement = new List<Dictionary<string, int>>();
            foreach (var (_, req) in gameState.Board.Nobles)
            {
                noblesRequirement.Add(req);
            }
            return noblesRequirement;
        }

        private Dictionary<string, int> GetCardsCount(SplendorState gameState, int agentId)
        {
            var result = new Dictionary<string, int>();
            foreach (var kvp in gameState.Agents[agentId].Cards)
            {
                if (kvp.Key != "yellow")
                {
                    result[kvp.Key] = kvp.Value.Count;
                }
            }
            return result;
        }

        private int GetActionHeuristicValue(SplendorState gameState, List<Dictionary<string, int>> agentsGemState, List<Dictionary<string, int>> noblesState, SplendorAction action)
        {
            int maxScore = 160; // Maximum possible score: 100 + 3 * 20 = 160
            int actionScore = GetActionScore(gameState, agentsGemState, noblesState, action);
            return maxScore - actionScore;
        }

        private (bool hasNoble, int getPoints, Dictionary<string, int> getGems, Dictionary<string, int> getCard) AnalyzeActionEffect(SplendorAction action)
        {
            bool hasNoble = action.Noble != null;
            int getPoints = hasNoble ? 3 : 0;
            var getGems = new Dictionary<string, int>();
            var getCard = new Dictionary<string, int>();

            foreach (var color in GEM_COLOURS)
            {
                getGems[color] = 0;
            }

            // if buy card, get points, card, gems
            if (action.Type == BUY_AVAILABLE || action.Type == BUY_RESERVE)
            {
                getPoints += action.Card.Points;
                getCard[action.Card.Colour] = 1;
                foreach (var kvp in action.ReturnedGems)
                {
                    getGems[kvp.Key] -= kvp.Value;
                }
            }
            // COLLECT_DIFF, COLLECT_SAME, or RESERVE:
            else
            {
                foreach (var kvp in action.CollectedGems)
                {
                    getGems[kvp.Key] += kvp.Value;
                }
                foreach (var kvp in action.ReturnedGems)
                {
                    getGems[kvp.Key] -= kvp.Value;
                }
            }

            return (hasNoble, getPoints, getGems, getCard);
        }

        private int CalculateNobleScore(bool hasNoble)
        {
            return hasNoble ? 100 : 0;
        }

        private int CalculatePointScore(int points)
        {
            return points * 20;
        }

        private int CalculateCardScore(Dictionary<string, int> getCard, Dictionary<string, int> noblesPreferColours, string actionType)
        {
            if (getCard.Count == 0)
                return 0;

            string getCardColour = getCard.Keys.First();
            int cardScore = 20 * getCard.Count + (noblesPreferColours.ContainsKey(getCardColour) ? noblesPreferColours[getCardColour] * 2 : 0);
            if (actionType == BUY_RESERVE)
                cardScore += 2;
            return cardScore;
        }

        private int CalculateGemScore(Dictionary<string, int> getGems)
        {
            return getGems.Values.Sum(value => value * 2);
        }

        private int CalculateReserveScore(SplendorAction action, SplendorState gameState, List<Dictionary<string, int>> agentsGemState, Dictionary<string, int> noblesPreferColours, List<Dictionary<string, int>> noblesRequirement)
        {
            int opponentAgentId = 1 - Id;
            // check if opponent can afford this card given his current state
            bool opponentCanAfford = action.Card.Cost.All(kvp => agentsGemState[opponentAgentId][kvp.Key] >= kvp.Value);
            
            if (opponentCanAfford)
            {
                int reserveOppoNoble = 0;
                var agentCardCount = GetCardsCount(gameState, opponentAgentId);
                agentCardCount[action.Card.Colour] += 1;
                
                bool isEffort = noblesRequirement.Any(noble => 
                    noble.All(kvp => agentCardCount.ContainsKey(kvp.Key) && agentCardCount[kvp.Key] >= kvp.Value));
                
                reserveOppoNoble = isEffort ? 60 : 0;
                // 7 is the reserve points, 10 is the points for point for reserve card
                return 7 + action.Card.Points * 10 + (noblesPreferColours.ContainsKey(action.Card.Colour) ? noblesPreferColours[action.Card.Colour] : 0) + reserveOppoNoble;
            }
            return 0;
        }

        private int CalculateProgressToCardScore(SplendorAction action, SplendorState gameState, List<Dictionary<string, int>> agentsGemState, Dictionary<string, int> noblesPreferColours)
        {
            var operaGemState = new Dictionary<string, int>(agentsGemState[Id]);
            
            foreach (var kvp in action.CollectedGems)
            {
                operaGemState[kvp.Key] += kvp.Value;
            }
            foreach (var kvp in action.ReturnedGems)
            {
                operaGemState[kvp.Key] -= kvp.Value;
            }

            int closeToCardScore = 0;
            var boardCards = gameState.Board.DealtList();
            var reserveCards = gameState.Agents[Id].Cards["yellow"];
            var availableCards = boardCards.Concat(reserveCards).ToList();

            foreach (var card in availableCards)
            {
                bool canAfford = card.Cost.All(kvp => operaGemState[kvp.Key] >= kvp.Value);
                int gap = card.Cost.Sum(kvp => Math.Max(0, kvp.Value - (operaGemState.ContainsKey(kvp.Key) ? operaGemState[kvp.Key] : 0)));

                if (canAfford)
                {
                    closeToCardScore = Math.Max(closeToCardScore, 3 + (noblesPreferColours.ContainsKey(card.Colour) ? noblesPreferColours[card.Colour] : 0));
                }
                else if (gap < 3)
                {
                    closeToCardScore = Math.Max(closeToCardScore, (3 + (noblesPreferColours.ContainsKey(card.Colour) ? noblesPreferColours[card.Colour] : 0)) / 2);
                }
            }

            return closeToCardScore;
        }

        private int GetActionScore(SplendorState gameState, List<Dictionary<string, int>> agentsGemState, List<Dictionary<string, int>> noblesRequirement, SplendorAction action)
        {
            // Calculate nobles preferences
            var noblesPreferColours = new Dictionary<string, int>();
            string[] colors = { "red", "green", "blue", "black", "white" };
            foreach (var color in colors)
            {
                noblesPreferColours[color] = noblesRequirement.Sum(noble => noble.ContainsKey(color) ? noble[color] : 0);
            }

            // Analysis of action effect
            var (hasNoble, getPoints, getGems, getCard) = AnalyzeActionEffect(action);

            // calculate scores for each components
            int nobleScore = CalculateNobleScore(hasNoble);
            int pointScore = CalculatePointScore(getPoints);
            int cardScore = CalculateCardScore(getCard, noblesPreferColours, action.Type);
            int gemScore = CalculateGemScore(getGems);
            int reserveScore = action.Type == RESERVE ? CalculateReserveScore(action, gameState, agentsGemState, noblesPreferColours, noblesRequirement) : 0;
            int progressToCardScore = (action.Type == COLLECT_DIFF || action.Type == COLLECT_SAME) ? CalculateProgressToCardScore(action, gameState, agentsGemState, noblesPreferColours) : 0;

            return nobleScore + pointScore + cardScore + gemScore + reserveScore + progressToCardScore;
        }

        private List<SplendorAction> GetStrategicActions(SplendorState gameState, int agentId)
        {
            var legalActions = gameRule.GetLegalActions(gameState, agentId).Cast<SplendorAction>().ToList();
            var actionQueue = new PriorityQueue<SplendorAction>();
            
            // Pick up current gem state from current game_state
            var agentsGemState = GetAllAgentGemsState(gameState);
            
            // Pick up current nobles prices on board from game_state
            var noblesRequirement = GetNoblesRequirement(gameState);
            
            // evaluate each action
            foreach (var action in legalActions)
            {
                actionQueue.Push(action, GetActionHeuristicValue(gameState, agentsGemState, noblesRequirement, action));
            }

            return actionQueue.GetLowestPriorityItems();
        }

        private double CalculateWinLoss(SplendorState state, int agentId)
        {
            int opponentId = 1 - Id;
            double selfScore = gameRule.CalScore(state, Id);
            double opponentScore = gameRule.CalScore(state, opponentId);

            if (selfScore >= WIN_SCORE)
                return 1;
            else if (opponentScore >= WIN_SCORE)
                return -1;
            else
                return 0;
        }

        private double CalculateProgressTowardNobles(SplendorState state, int agentId)
        {
            double totalProgress = 0;
            var playerCards = state.Agents[agentId].Cards;
            var playerCardsCounts = new Dictionary<string, int>();
            
            foreach (var kvp in playerCards)
            {
                playerCardsCounts[kvp.Key] = kvp.Value.Count;
            }

            // nobles currently on the board
            var nobles = state.Board.Nobles;

            foreach (var (_, requirements) in nobles)
            {
                double progress = 0;
                foreach (var kvp in requirements)
                {
                    string color = kvp.Key;
                    int requiredAmount = kvp.Value;
                    int playerCardCount = playerCardsCounts.ContainsKey(color) ? playerCardsCounts[color] : 0;
                    
                    if (playerCardCount >= requiredAmount)
                        progress += 1;
                    else
                        progress += (double)playerCardCount / requiredAmount;
                }
                // Normalized progress: progressForEachColour / NumberOfDistinctColour
                totalProgress += progress / requirements.Count;
            }
            return totalProgress;
        }

        public override object SelectAction(List<object> actions, GameState gameState)
        {
            stopwatch.Restart();
            SplendorAction bestAction = null;
            int depthReached = 0;
            
            for (int depth = 1; depth <= MAX_DEPTH; depth++)
            {
                if (!HaveTime())
                    break;
                    
                var bestActionAtDepth = MinimaxDecision((SplendorState)gameState, depth);
                if (bestActionAtDepth != null)
                {
                    bestAction = bestActionAtDepth;
                    depthReached = depth;
                }
            }

            return bestAction ?? (SplendorAction)actions[0];
        }

        private SplendorAction MinimaxDecision(SplendorState gameState, int depth)
        {
            double MaxValue(SplendorState state, int agentId, int currentDepth, double alpha, double beta)
            {
                if (currentDepth == 0)
                    return EvaluateState(state, depth);

                double maxEval = NEG_INFINITY;
                var actions = GetStrategicActions(state, agentId);
                
                foreach (var action in actions)
                {
                    if (!HaveTime())
                        break;
                        
                    var successorState = (SplendorState)gameRule.GenerateSuccessor(state.DeepCopy(), action, agentId);
                    double evalScore = MinValue(successorState, 1 - agentId, currentDepth - 1, alpha, beta);
                    maxEval = Math.Max(maxEval, evalScore);
                    alpha = Math.Max(alpha, evalScore);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }

            double MinValue(SplendorState state, int agentId, int currentDepth, double alpha, double beta)
            {
                if (currentDepth == 0)
                    return EvaluateState(state, depth);
                    
                double minEval = POS_INFINITY;
                var actions = GetStrategicActions(state, agentId);
                
                foreach (var action in actions)
                {
                    if (!HaveTime())
                        break;
                        
                    var successorState = (SplendorState)gameRule.GenerateSuccessor(state.DeepCopy(), action, agentId);
                    double evalScore = MaxValue(successorState, Id, currentDepth - 1, alpha, beta);
                    minEval = Math.Min(minEval, evalScore);
                    beta = Math.Min(beta, evalScore);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }

            int agentId = Id;
            int opponentId = 1 - agentId;
            SplendorAction bestAction = null;
            double bestEval = NEG_INFINITY;
            var actions = GetStrategicActions(gameState, agentId);
            
            int numActions = actions.Count;
            int count = 0;
            
            foreach (var action in actions)
            {
                count++;
                
                if (!HaveTime())
                    break;
                    
                var successorState = (SplendorState)gameRule.GenerateSuccessor(gameState.DeepCopy(), action, agentId);
                double evalScore = MinValue(successorState, opponentId, depth - 1, NEG_INFINITY, POS_INFINITY);
                
                if (evalScore > bestEval)
                {
                    bestEval = evalScore;
                    bestAction = action;
                }
            }
            
            return bestAction ?? actions[0];
        }
    }
} 