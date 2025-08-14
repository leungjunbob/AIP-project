// INFORMATION ------------------------------------------------------------------------------------------------------- //
// Author:  Steven Spratley, extending code by Guang Ho and Michelle Blom
// Date:    04/01/2021
// Purpose: Implements "Splendor" for the COMP90054 competitive game environment

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Splendor
{
    // 明确定义Action类，解决原Python代码中Action未定义的问题
    public class Action
    {
        public string Type { get; set; } = "";
        public Dictionary<string, int> CollectedGems { get; set; }
        public Dictionary<string, int> ReturnedGems { get; set; }
        public Card? Card { get; set; }
        public Tuple<string, Dictionary<string, int>>? Noble { get; set; }

        public Action()
        {
            CollectedGems = new Dictionary<string, int>();
            ReturnedGems = new Dictionary<string, int>();
        }

        public Action(string type, Dictionary<string, int> collectedGems, Dictionary<string, int> returnedGems, Card? card = null, Tuple<string, Dictionary<string, int>>? noble = null)
        {
            Type = type;
            CollectedGems = collectedGems ?? new Dictionary<string, int>();
            ReturnedGems = returnedGems ?? new Dictionary<string, int>();
            Card = card;
            Noble = noble;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Action other)
            {
                if (Type != other.Type) return false;
                if (Card?.Code != other.Card?.Code) return false;
                if (Noble?.Item1 != other.Noble?.Item1) return false;
                
                // 比较CollectedGems
                if (CollectedGems.Count != other.CollectedGems.Count) return false;
                foreach (var kvp in CollectedGems)
                {
                    if (!other.CollectedGems.ContainsKey(kvp.Key) || other.CollectedGems[kvp.Key] != kvp.Value)
                        return false;
                }
                
                // 比较ReturnedGems
                if (ReturnedGems.Count != other.ReturnedGems.Count) return false;
                foreach (var kvp in ReturnedGems)
                {
                    if (!other.ReturnedGems.ContainsKey(kvp.Key) || other.ReturnedGems[kvp.Key] != kvp.Value)
                        return false;
                }
                
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ (Card?.Code?.GetHashCode() ?? 0) ^ (Noble?.Item1?.GetHashCode() ?? 0);
        }

        public Action DeepCopy()
        {
            var newCollectedGems = new Dictionary<string, int>();
            foreach (var kvp in CollectedGems)
            {
                newCollectedGems[kvp.Key] = kvp.Value;
            }

            var newReturnedGems = new Dictionary<string, int>();
            foreach (var kvp in ReturnedGems)
            {
                newReturnedGems[kvp.Key] = kvp.Value;
            }

            Card? newCard = Card?.DeepCopy();
            
            Tuple<string, Dictionary<string, int>>? newNoble = null;
            if (Noble != null)
            {
                var newNobleCosts = new Dictionary<string, int>();
                foreach (var kvp in Noble.Item2)
                {
                    newNobleCosts[kvp.Key] = kvp.Value;
                }
                newNoble = new Tuple<string, Dictionary<string, int>>(Noble.Item1, newNobleCosts);
            }

            return new Action(Type, newCollectedGems, newReturnedGems, newCard, newNoble);
        }
    }

    // 卡牌类
    public class Card
    {
        public string Colour { get; set; } = "";
        public string Code { get; set; } = "";
        public Dictionary<string, int> Cost { get; set; } = new Dictionary<string, int>();
        public int DeckId { get; set; }
        public int Points { get; set; }

        public Card(string colour, string code, Dictionary<string, int> cost, int deckId, int points)
        {
            Colour = colour;
            Code = code;
            Cost = cost;
            DeckId = deckId;
            Points = points;
        }

        public Card DeepCopy()
        {
            var newCost = new Dictionary<string, int>();
            foreach (var kvp in Cost)
            {
                newCost[kvp.Key] = kvp.Value;
            }
            return new Card(Colour, Code, newCost, DeckId, Points);
        }

        public override string ToString()
        {
            var gemString = "";
            foreach (var kvp in Cost)
            {
                gemString += (gemString != "" ? ", " : "") + $"{kvp.Value} {kvp.Key}";
            }
            return $"Tier {DeckId + 1} {Colour} card worth {Points} points and costing {gemString}";
        }

        public override bool Equals(object? obj)
        {
            // Equal in the ways that matter: code is identical, and points haven't been tampered with
            if (obj is Card other)
            {
                return other.Code == Code && Points == other.Points && 
                       Points == SplendorUtils.CARDS[Code].Item4; // Check against original card data
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }

    // 游戏状态类 - 继承自GameState基类
    public class SplendorState : GameState
    {
        public BoardState Board { get; set; }
        public List<SplendorState.AgentState> Agents { get; set; }
        public int AgentToMove { get; set; }

        public SplendorState(int numAgents)
        {
            Board = new BoardState(numAgents);
            Agents = new List<SplendorState.AgentState>();
            for (int i = 0; i < numAgents; i++)
            {
                Agents.Add(new SplendorState.AgentState(i));
            }
            AgentToMove = 0;
        }

        public override string ToString()
        {
            var output = "";
            output += Board.ToString();
            foreach (var agentState in Agents)
            {
                output += agentState.ToString();
            }
            return output;
        }

        public SplendorState DeepCopy()
        {
            var newState = new SplendorState(0); // 临时创建
            newState.AgentToMove = AgentToMove;
            
            // 复制Board - 手动复制所有属性
            newState.Board = new BoardState(0);
            newState.Board.Gems = new Dictionary<string, int>();
            foreach (var kvp in Board.Gems)
            {
                newState.Board.Gems[kvp.Key] = kvp.Value;
            }
            
            newState.Board.Nobles = new List<Tuple<string, Dictionary<string, int>>>();
            foreach (var noble in Board.Nobles)
            {
                var newNobleCosts = new Dictionary<string, int>();
                foreach (var kvp in noble.Item2)
                {
                    newNobleCosts[kvp.Key] = kvp.Value;
                }
                newState.Board.Nobles.Add(new Tuple<string, Dictionary<string, int>>(noble.Item1, newNobleCosts));
            }
            
            // 复制Decks
            newState.Board.Decks = new List<List<Card>>();
            foreach (var deck in Board.Decks)
            {
                var newDeck = new List<Card>();
                foreach (var card in deck)
                {
                    newDeck.Add(card.DeepCopy());
                }
                newState.Board.Decks.Add(newDeck);
            }
            
            // 复制Dealt
            newState.Board.Dealt = new Card[3][];
            for (int i = 0; i < 3; i++)
            {
                newState.Board.Dealt[i] = new Card[4];
                for (int j = 0; j < 4; j++)
                {
                    newState.Board.Dealt[i][j] = Board.Dealt[i][j]?.DeepCopy();
                }
            }
            
            // 复制Agents - 手动复制所有属性
            newState.Agents = new List<SplendorState.AgentState>();
            foreach (var agent in Agents)
            {
                var newAgent = new SplendorState.AgentState(agent.Id);
                newAgent.Score = agent.Score;
                newAgent.Passed = agent.Passed;
                
                // 复制Gems
                foreach (var kvp in agent.Gems)
                {
                    newAgent.Gems[kvp.Key] = kvp.Value;
                }
                
                // 复制Cards
                foreach (var kvp in agent.Cards)
                {
                    var newCardList = new List<Card>();
                    foreach (var card in kvp.Value)
                    {
                        newCardList.Add(card.DeepCopy());
                    }
                    newAgent.Cards[kvp.Key] = newCardList;
                }
                
                // 复制Nobles
                foreach (var noble in agent.Nobles)
                {
                    var newNobleCosts = new Dictionary<string, int>();
                    foreach (var kvp in noble.Item2)
                    {
                        newNobleCosts[kvp.Key] = kvp.Value;
                    }
                    newAgent.Nobles.Add(new Tuple<string, Dictionary<string, int>>(noble.Item1, newNobleCosts));
                }
                
                newState.Agents.Add(newAgent);
            }

            return newState;
        }



        // 棋盘状态类
        public class BoardState
        {
            public List<List<Card>> Decks { get; set; }
            public Card[][] Dealt { get; set; }
            public Dictionary<string, int> Gems { get; set; }
            public List<Tuple<string, Dictionary<string, int>>> Nobles { get; set; }

            public BoardState(int numAgents)
            {
                Decks = new List<List<Card>> { new List<Card>(), new List<Card>(), new List<Card>() };
                Dealt = new Card[3][];
                for (int i = 0; i < 3; i++)
                {
                    Dealt[i] = new Card[4];
                }

                // 根据玩家数量设置宝石数量
                int n = numAgents == 2 ? 4 : numAgents == 3 ? 5 : 7;
                Gems = new Dictionary<string, int>
                {
                    {"black", n}, {"red", n}, {"yellow", 5}, {"green", n}, {"blue", n}, {"white", n}
                };

                // 随机选择贵族
                var random = new Random();
                Nobles = SplendorUtils.NOBLES.OrderBy(x => random.Next()).Take(numAgents + 1).ToList();

                // 初始化卡牌
                foreach (var kvp in SplendorUtils.CARDS)
                {
                    var code = kvp.Key;
                    var (colour, cost, deckId, points) = kvp.Value;
                    var card = new Card(colour, code, cost, deckId - 1, points); // deckId从1开始，转换为0开始
                    Decks[deckId - 1].Add(card);
                }

                // 洗牌并发牌
                foreach (var deck in Decks)
                {
                    Shuffle(deck);
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Dealt[i][j] = Deal(i);
                    }
                }
            }

            private void Shuffle<T>(List<T> list)
            {
                var random = new Random();
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }

            public Card? Deal(int deckId)
            {
                if (Decks[deckId].Count > 0)
                {
                    Shuffle(Decks[deckId]);
                    var card = Decks[deckId][Decks[deckId].Count - 1];
                    Decks[deckId].RemoveAt(Decks[deckId].Count - 1);
                    return card;
                }
                return null;
            }

            public List<Card> DealtList()
            {
                var result = new List<Card>();
                foreach (var deck in Dealt)
                {
                    foreach (var card in deck)
                    {
                        if (card != null)
                        {
                            result.Add(card);
                        }
                    }
                }
                return result;
            }

            public override string ToString()
            {
                var output = "";
                output += "\nAvailable Gems:\n";
                output += "{";
                var gemStrings = new List<string>();
                foreach (var kvp in Gems)
                {
                    gemStrings.Add($"{kvp.Key}: {kvp.Value}");
                }
                output += string.Join(", ", gemStrings);
                output += "}";
                output += "\nDealt Card List: \n";
                foreach (var card in DealtList())
                {
                    output += "\t" + card.ToString() + "\n";
                }
                output += "\nNoble List \n";
                output += "[";
                var nobleStrings = new List<string>();
                foreach (var noble in Nobles)
                {
                    nobleStrings.Add(noble.Item1);
                }
                output += string.Join(", ", nobleStrings);
                output += "]";
                output += "\n";
                return output;
            }
        }

        // 代理状态类
        public class AgentState
        {
            public int Id { get; set; }
            public int Score { get; set; }
            public Dictionary<string, int> Gems { get; set; }
            public Dictionary<string, List<Card>> Cards { get; set; }
            public List<Tuple<string, Dictionary<string, int>>> Nobles { get; set; }
            public bool Passed { get; set; }
            public AgentTrace AgentTrace { get; set; }
            public Action? LastAction { get; set; }

            public AgentState(int id)
            {
                Id = id;
                Score = 0;
                Gems = new Dictionary<string, int>();
                Cards = new Dictionary<string, List<Card>>();
                Nobles = new List<Tuple<string, Dictionary<string, int>>>();
                Passed = false;
                AgentTrace = new AgentTrace(id);
                LastAction = null;

                // 初始化宝石和卡牌
                foreach (var colour in SplendorUtils.COLOURS.Values)
                {
                    Gems[colour] = 0;
                    Cards[colour] = new List<Card>();
                }
            }

            public AgentState DeepCopy()
            {
                var newAgent = new AgentState(Id);
                newAgent.Score = Score;
                newAgent.Passed = Passed;
                
                // 复制宝石
                foreach (var kvp in Gems)
                {
                    newAgent.Gems[kvp.Key] = kvp.Value;
                }
                
                // 复制卡牌
                foreach (var kvp in Cards)
                {
                    var newCardList = new List<Card>();
                    foreach (var card in kvp.Value)
                    {
                        newCardList.Add(card.DeepCopy());
                    }
                    newAgent.Cards[kvp.Key] = newCardList;
                }
                
                // 复制贵族
                foreach (var noble in Nobles)
                {
                    var newNobleCosts = new Dictionary<string, int>();
                    foreach (var kvp in noble.Item2)
                    {
                        newNobleCosts[kvp.Key] = kvp.Value;
                    }
                    newAgent.Nobles.Add(new Tuple<string, Dictionary<string, int>>(noble.Item1, newNobleCosts));
                }
                
                // 复制最后动作
                newAgent.LastAction = LastAction?.DeepCopy();
                
                return newAgent;
            }

            public override string ToString()
            {
                var output = "";
                output += $"Agent ({Id}): \n";
                output += $"\tscore: {Score},\n";
                
                // 格式化 gems
                output += "\tgems: {";
                var gemStrings = new List<string>();
                foreach (var kvp in Gems)
                {
                    gemStrings.Add($"{kvp.Key}: {kvp.Value}");
                }
                output += string.Join(", ", gemStrings);
                output += "}\n";
                
                // 格式化 cards
                output += "\tcards: {";
                var cardStrings = new List<string>();
                foreach (var kvp in Cards)
                {
                    cardStrings.Add($"{kvp.Key}: {kvp.Value.Count}");
                }
                output += string.Join(", ", cardStrings);
                output += "}\n";
                
                // 格式化 nobles
                output += "\tnobles: ";
                if (Nobles.Count == 0)
                {
                    output += "0.\n";
                }
                else
                {
                    var nobleStrings = new List<string>();
                    foreach (var noble in Nobles)
                    {
                        nobleStrings.Add(noble.Item1);
                    }
                    output += string.Join(", ", nobleStrings) + ".\n";
                }
                
                return output;
            }
        }
    }

    // 游戏规则类
    public class SplendorGameRule : GameRule
    {
        public SplendorGameRule(int numOfAgent) : base(numOfAgent)
        {
            // 没有私有信息：代理状态对其他代理可用
            PrivateInformation = null;
        }

        public override GameState InitialGameState()
        {
            return new SplendorState(NumOfAgent);
        }

        public override GameState GenerateSuccessor(GameState state, object actionObj, int agentId)
        {
            var splendorState = (SplendorState)state;
            var action = (Action)actionObj;
            var agent = splendorState.Agents[agentId];
            var board = splendorState.Board;

            agent.LastAction = action; // Record last action such that other agents can make use of this information.
            int score = 0;

            if (action.Card != null)
            {
                var card = action.Card;
            }

            if (action.Type.Contains("collect") || action.Type == "reserve")
            {
                // Decrement board gem stacks by collected_gems. Increment player gem stacks by collected_gems.
                foreach (var kvp in action.CollectedGems)
                {
                    board.Gems[kvp.Key] -= kvp.Value;
                    agent.Gems[kvp.Key] += kvp.Value;
                }
                // Decrement player gem stacks by returned_gems. Increment board gem stacks by returned_gems.
                foreach (var kvp in action.ReturnedGems)
                {
                    agent.Gems[kvp.Key] -= kvp.Value;
                    board.Gems[kvp.Key] += kvp.Value;
                }

                if (action.Type == "reserve")
                {
                    // Remove card from dealt cards by locating via unique code (cards aren't otherwise hashable).
                    // Since we want to retain the positioning of dealt cards, set removed card slot to new dealt card.
                    // Since the board may have None cards (empty slots that cannot be filled), check cards first.
                    // Add card to player's yellow stack.
                    for (int i = 0; i < board.Dealt[action.Card.DeckId].Length; i++)
                    {
                        if (board.Dealt[action.Card.DeckId][i] != null && board.Dealt[action.Card.DeckId][i].Code == action.Card.Code)
                        {
                            board.Dealt[action.Card.DeckId][i] = board.Deal(action.Card.DeckId);
                            agent.Cards["yellow"].Add(action.Card);
                            break;
                        }
                    }
                }
            }
            else if (action.Type.Contains("buy"))
            {
                // Decrement player gem stacks by returned_gems. Increment board gem stacks by returned_gems.
                foreach (var kvp in action.ReturnedGems)
                {
                    agent.Gems[kvp.Key] -= kvp.Value;
                    board.Gems[kvp.Key] += kvp.Value;
                }
                // If buying one of the available cards on the board, set removed card slot to new dealt card.
                // Since the board may have None cards (empty slots that cannot be filled), check cards first.
                if (action.Type.Contains("available"))
                {
                    for (int i = 0; i < board.Dealt[action.Card.DeckId].Length; i++)
                    {
                        if (board.Dealt[action.Card.DeckId][i] != null && board.Dealt[action.Card.DeckId][i].Code == action.Card.Code)
                        {
                            board.Dealt[action.Card.DeckId][i] = board.Deal(action.Card.DeckId);
                            break;
                        }
                    }
                }
                // Else, agent is buying a reserved card. Remove card from player's yellow stack.
                else
                {
                    for (int i = 0; i < agent.Cards["yellow"].Count; i++)
                    {
                        if (agent.Cards["yellow"][i].Code == action.Card.Code)
                        {
                            agent.Cards["yellow"].RemoveAt(i);
                            break;
                        }
                    }
                }

                // Add card to player's stack of matching colour, and increment agent's score accordingly.
                agent.Cards[action.Card.Colour].Add(action.Card);
                score += action.Card.Points;
            }

            if (action.Noble != null)
            {
                // Remove noble from board. Add noble to player's stack. Like cards, nobles aren't hashable due to possessing
                // dictionaries (i.e. resource costs). Therefore, locate and delete the noble via unique code.
                // Add noble's points to agent score.
                for (int i = 0; i < board.Nobles.Count; i++)
                {
                    if (board.Nobles[i].Item1 == action.Noble.Item1)
                    {
                        board.Nobles.RemoveAt(i);
                        agent.Nobles.Add(action.Noble);
                        score += 3;
                        break;
                    }
                }
            }

            // Log this turn's action and any resultant score. Return updated gamestate.
            agent.AgentTrace.ActionReward.Add(new Tuple<Action, int>(action, score));
            agent.Score += score;
            agent.Passed = action.Type == "pass";
            return splendorState;
        }

        public override bool GameEnds()
        {
            var splendorState = (SplendorState)this.CurrentGameState;
            int deadlock = 0;
            foreach (var agent in splendorState.Agents)
            {
                deadlock += agent.Passed ? 1 : 0;
                if (agent.Score >= 15 && this.CurrentAgentIndex == 0)
                {
                    return true;
                }
            }
            return deadlock == splendorState.Agents.Count;
        }

        public override double CalScore(GameState gameState, int agentId)
        {
            var splendorState = (SplendorState)gameState;
            double maxScore = 0;
            var details = new List<Tuple<int, int, double>>();

            Func<SplendorState.AgentState, int> boughtCards = a => a.Cards.Values.Where(cards => cards != a.Cards["yellow"]).Sum(cards => cards.Count);

            foreach (var agent in splendorState.Agents)
            {
                details.Add(new Tuple<int, int, double>(agent.Id, boughtCards(agent), agent.Score));
                maxScore = Math.Max(agent.Score, maxScore);
            }

            var victors = details.Where(d => d.Item3 == maxScore).ToList();
            if (victors.Count > 1 && victors.Any(d => d.Item1 == agentId))
            {
                int minCards = details.Min(d => d.Item2);
                if (boughtCards(splendorState.Agents[agentId]) == minCards)
                {
                    // Add a half point if this agent was a tied victor, and had the fewest cards.
                    return splendorState.Agents[agentId].Score + 0.5;
                }
            }

            return splendorState.Agents[agentId].Score;
        }

        // 生成可以返回的宝石组合
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
                    foreach (var colour in SplendorUtils.COLOURS.Values)
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

        // 获取组合的辅助方法
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

        // 检查资源是否足够
        public Dictionary<string, int>? ResourcesSufficient(SplendorState.AgentState agent, Dictionary<string, int> costs)
        {
            int wild = agent.Gems["yellow"];
            var returnCombo = new Dictionary<string, int>();
            foreach (var colour in SplendorUtils.COLOURS.Values)
            {
                returnCombo[colour] = 0;
            }

            foreach (var kvp in costs)
            {
                string colour = kvp.Key;
                int cost = kvp.Value;

                // If a shortfall is found, see if the difference can be made with wild/seal/yellow gems.
                int available = agent.Gems[colour] + agent.Cards[colour].Count;
                int shortfall = Math.Max(cost - available, 0); // Shortfall shouldn't be negative.
                wild -= shortfall;
                // If wilds are expended, the agent cannot make the purchase.
                if (wild < 0)
                {
                    return null;
                }
                // Else, increment return_combo accordingly. Note that the agent should never return gems if it can afford 
                // to pay using its card stacks, and should never return wilds if it can return coloured gems instead. 
                // Although there may be strategic instances where holding on to coloured gems is beneficial (by virtue of 
                // shorting players from resources), in this implementation, this edge case is not worth added complexity.
                int gemCost = Math.Max(cost - agent.Cards[colour].Count, 0); // Gems owed.
                int gemShortfall = Math.Max(gemCost - agent.Gems[colour], 0); // Wilds required.
                returnCombo[colour] = gemCost - gemShortfall; // Coloured gems to be returned.
                returnCombo["yellow"] += gemShortfall; // Wilds to be returned.
            }

            // Filter out unnecessary colours and return dict specifying combination of gems.
            var result = returnCombo.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result.Count > 0 ? result : null;
        }

        // 检查贵族是否可以访问
        public bool NobleVisit(SplendorState.AgentState agent, Tuple<string, Dictionary<string, int>> noble)
        {
            var costs = noble.Item2;
            foreach (var kvp in costs)
            {
                if (agent.Cards[kvp.Key].Count < kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public override List<object> GetLegalActions(GameState gameState, int agentId)
        {
            var actions = new List<object>();
            var splendorState = (SplendorState)gameState;
            var agent = splendorState.Agents[agentId];
            var board = splendorState.Board;

            // 检查是否有贵族等待访问
            var potentialNobles = new List<Tuple<string, Dictionary<string, int>>?>();
            foreach (var noble in board.Nobles)
            {
                if (NobleVisit(agent, noble))
                {
                    potentialNobles.Add(noble);
                }
            }
            if (potentialNobles.Count == 0)
            {
                potentialNobles.Add((Tuple<string, Dictionary<string, int>>?)null);
            }

            // 生成收集不同宝石的行动
            var availableColours = board.Gems.Where(kvp => kvp.Key != "yellow" && kvp.Value > 0).Select(kvp => kvp.Key).ToList();
            int numHoldingGem = agent.Gems.Values.Sum();
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
                        var returnCombos = GenerateReturnCombos(agent.Gems, collectedGems);
                        foreach (var returnedGems in returnCombos)
                        {
                            foreach (var noble in potentialNobles)
                            {
                                actions.Add(new Action("collect_diff", collectedGems, returnedGems, (Card?)null, noble));
                            }
                        }
                    }
                }
            }

            // 生成收集相同宝石的行动
            var sameColours = board.Gems.Where(kvp => kvp.Key != "yellow" && kvp.Value >= 4).Select(kvp => kvp.Key).ToList();
            foreach (var colour in sameColours)
            {
                var collectedGems = new Dictionary<string, int> { { colour, 2 } };
                var returnCombos = GenerateReturnCombos(agent.Gems, collectedGems);
                foreach (var returnedGems in returnCombos)
                {
                    foreach (var noble in potentialNobles)
                    {
                        actions.Add(new Action("collect_same", collectedGems, returnedGems, (Card?)null, noble));
                    }
                }
            }

            // 生成保留卡牌的行动
            if (agent.Cards["yellow"].Count < 3)
            {
                var collectedGems = board.Gems["yellow"] > 0 ? new Dictionary<string, int> { { "yellow", 1 } } : new Dictionary<string, int>();
                var returnCombos = GenerateReturnCombos(agent.Gems, collectedGems);
                foreach (var returnedGems in returnCombos)
                {
                    foreach (var card in board.DealtList())
                    {
                        if (card != null)
                        {
                            foreach (var noble in potentialNobles)
                            {
                                actions.Add(new Action("reserve", collectedGems, returnedGems, card, noble));
                            }
                        }
                    }
                }
            }

            // 生成购买卡牌的行动
            // 只能购买桌面上的卡牌或自己 reserve 的卡牌
            var availableCards = board.DealtList().ToList();
            var reservedCards = agent.Cards["yellow"].ToList();
            var allCards = availableCards.Concat(reservedCards).ToList();
            
            foreach (var card in allCards)
            {
                if (card == null || agent.Cards[card.Colour].Count == 7)
                    continue;

                var returnedGems = ResourcesSufficient(agent, card.Cost);
                if (returnedGems != null)
                {
                    // 检查是否有新的贵族可以访问
                    var newNobles = new List<Tuple<string, Dictionary<string, int>>?>();
                    foreach (var noble in board.Nobles)
                    {
                        var agentPostAction = CloneAgent(agent);
                        agentPostAction.Cards[card.Colour].Add(card);
                        if (NobleVisit(agentPostAction, noble))
                        {
                            newNobles.Add(noble);
                        }
                    }
                    if (newNobles.Count == 0)
                    {
                        newNobles.Add((Tuple<string, Dictionary<string, int>>?)null);
                    }

                    foreach (var noble in newNobles)
                    {
                        string actionType = reservedCards.Contains(card) ? "buy_reserve" : "buy_available";
                        actions.Add(new Action(actionType, new Dictionary<string, int>(), returnedGems, card, noble));
                    }
                }
            }

            // 如果没有行动，只能跳过
            if (actions.Count == 0)
            {
                foreach (var noble in potentialNobles)
                {
                    actions.Add(new Action("pass", new Dictionary<string, int>(), new Dictionary<string, int>(), (Card?)null, noble));
                }
            }

            return actions;
        }

        // 克隆代理的辅助方法
        private SplendorState.AgentState CloneAgent(SplendorState.AgentState original)
        {
            var clone = new SplendorState.AgentState(original.Id)
            {
                Score = original.Score,
                Passed = original.Passed
            };

            foreach (var kvp in original.Gems)
            {
                clone.Gems[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in original.Cards)
            {
                var newCardList = new List<Card>();
                foreach (var card in kvp.Value)
                {
                    newCardList.Add(card.DeepCopy());
                }
                clone.Cards[kvp.Key] = newCardList;
            }

            clone.Nobles = new List<Tuple<string, Dictionary<string, int>>>(original.Nobles);

            return clone;
        }
    }
} 