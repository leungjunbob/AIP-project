using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using SplendorUnity.Models;
using SplendorUnity.Utils;

namespace SplendorUnity.Core
{
    /// <summary>
    /// Splendor游戏状态，使用ScriptableObject便于序列化
    /// 替代原SplendorState
    /// </summary>
    [CreateAssetMenu(fileName = "SplendorGameState", menuName = "Splendor/Game State")]
    public class SplendorGameState : ScriptableObject
    {
        [Header("游戏状态")]
        public BoardState board;
        public List<AgentState> agents = new List<AgentState>();
        public int agentToMove = 0;
        
        /// <summary>
        /// 初始化游戏状态
        /// </summary>
        public void Initialize(int numAgents)
        {
            board = new BoardState(numAgents);
            agents.Clear();
            for (int i = 0; i < numAgents; i++)
            {
                agents.Add(new AgentState(i));
            }
            agentToMove = 0;
        }
        
        /// <summary>
        /// 深度复制游戏状态
        /// </summary>
        public SplendorGameState DeepCopy()
        {
            var newState = CreateInstance<SplendorGameState>();
            newState.agentToMove = agentToMove;
            newState.board = board.DeepCopy();
            newState.agents = agents.Select(agent => agent.DeepCopy()).ToList();
            return newState;
        }
        
        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            var output = "";
            output += board.ToString();
            foreach (var agentState in agents)
            {
                output += agentState.ToString();
            }
            return output;
        }
        
        /// <summary>
        /// 棋盘状态类
        /// </summary>
        [System.Serializable]
        public class BoardState
        {
            [Header("卡牌相关")]
            public List<List<Card>> decks = new List<List<Card>>();
            public Card[][] dealt = new Card[3][];
            public Dictionary<string, int> gems = new Dictionary<string, int>();
            public List<Noble> nobles = new List<Noble>();
            
            public BoardState(int numAgents)
            {
                // 初始化卡牌堆
                decks = new List<List<Card>> { new List<Card>(), new List<Card>(), new List<Card>() };
                dealt = new Card[3][];
                for (int i = 0; i < 3; i++)
                {
                    dealt[i] = new Card[4];
                }

                // 根据玩家数量设置宝石数量
                int n = numAgents == 2 ? 4 : numAgents == 3 ? 5 : 7;
                gems = new Dictionary<string, int>
                {
                    {"black", n}, {"red", n}, {"yellow", 5}, {"green", n}, {"blue", n}, {"white", n}
                };

                // 随机选择贵族
                var random = new System.Random();
                var allNobles = GameData.NOBLES.OrderBy(x => random.Next()).Take(numAgents + 1).ToList();
                nobles = allNobles.Select(nobleData => 
                    Noble.CreateNoble(nobleData.Item1, nobleData.Item2)).ToList();

                // 初始化卡牌
                foreach (var kvp in GameData.CARDS)
                {
                    var code = kvp.Key;
                    var (colour, cost, deckId, points) = kvp.Value;
                    var card = Card.CreateCard(code, colour, deckId - 1, points, cost);
                    decks[deckId - 1].Add(card);
                }

                // 洗牌并发牌
                foreach (var deck in decks)
                {
                    Shuffle(deck);
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        dealt[i][j] = Deal(i);
                    }
                }
            }
            
            /// <summary>
            /// 深度复制棋盘状态
            /// </summary>
            public BoardState DeepCopy()
            {
                var newBoard = new BoardState(0);
                
                // 复制宝石
                newBoard.gems.Clear();
                foreach (var kvp in gems)
                {
                    newBoard.gems[kvp.Key] = kvp.Value;
                }
                
                // 复制贵族
                newBoard.nobles.Clear();
                foreach (var noble in nobles)
                {
                    newBoard.nobles.Add(noble.DeepCopy());
                }
                
                // 复制卡牌堆
                newBoard.decks.Clear();
                foreach (var deck in decks)
                {
                    var newDeck = new List<Card>();
                    foreach (var card in deck)
                    {
                        newDeck.Add(card.DeepCopy());
                    }
                    newBoard.decks.Add(newDeck);
                }
                
                // 复制已发卡牌
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        newBoard.dealt[i][j] = dealt[i][j]?.DeepCopy();
                    }
                }
                
                return newBoard;
            }

            /// <summary>
            /// 洗牌
            /// </summary>
            private void Shuffle<T>(List<T> list)
            {
                var random = new System.Random();
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

            /// <summary>
            /// 发牌
            /// </summary>
            public Card Deal(int deckId)
            {
                if (decks[deckId].Count > 0)
                {
                    Shuffle(decks[deckId]);
                    var card = decks[deckId][decks[deckId].Count - 1];
                    decks[deckId].RemoveAt(decks[deckId].Count - 1);
                    return card;
                }
                return null;
            }

            /// <summary>
            /// 获取已发卡牌列表
            /// </summary>
            public List<Card> DealtList()
            {
                var result = new List<Card>();
                foreach (var deck in dealt)
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

            /// <summary>
            /// 转换为字符串
            /// </summary>
            public override string ToString()
            {
                var output = "";
                output += "\nAvailable Gems:\n";
                output += "{";
                var gemStrings = new List<string>();
                foreach (var kvp in gems)
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
                foreach (var noble in nobles)
                {
                    nobleStrings.Add(noble.Code);
                }
                output += string.Join(", ", nobleStrings);
                output += "]";
                output += "\n";
                return output;
            }
        }

        /// <summary>
        /// 代理状态类
        /// </summary>
        [System.Serializable]
        public class AgentState
        {
            [Header("基本信息")]
            public int id;
            public int score;
            public bool passed;
            
            [Header("资源")]
            public Dictionary<string, int> gems = new Dictionary<string, int>();
            public Dictionary<string, List<Card>> cards = new Dictionary<string, List<Card>>();
            public List<Noble> nobles = new List<Noble>();
            
            [Header("追踪")]
            public AgentTrace agentTrace;
            public Action lastAction;

            public AgentState(int agentId)
            {
                id = agentId;
                score = 0;
                passed = false;
                gems = new Dictionary<string, int>();
                cards = new Dictionary<string, List<Card>>();
                nobles = new List<Noble>();
                agentTrace = new AgentTrace(id);
                lastAction = null;

                // 初始化宝石和卡牌
                foreach (var colour in GameData.COLOURS.Values)
                {
                    gems[colour] = 0;
                    cards[colour] = new List<Card>();
                }
            }

            /// <summary>
            /// 深度复制代理状态
            /// </summary>
            public AgentState DeepCopy()
            {
                var newAgent = new AgentState(id);
                newAgent.score = score;
                newAgent.passed = passed;
                
                // 复制宝石
                foreach (var kvp in gems)
                {
                    newAgent.gems[kvp.Key] = kvp.Value;
                }
                
                // 复制卡牌
                foreach (var kvp in cards)
                {
                    var newCardList = new List<Card>();
                    foreach (var card in kvp.Value)
                    {
                        newCardList.Add(card.DeepCopy());
                    }
                    newAgent.cards[kvp.Key] = newCardList;
                }
                
                // 复制贵族
                foreach (var noble in nobles)
                {
                    newAgent.nobles.Add(noble.DeepCopy());
                }
                
                // 复制最后动作
                newAgent.lastAction = lastAction?.DeepCopy();
                
                return newAgent;
            }

            /// <summary>
            /// 转换为字符串
            /// </summary>
            public override string ToString()
            {
                var output = "";
                output += $"Agent ({id}): \n";
                output += $"\tscore: {score},\n";
                
                // 格式化 gems
                output += "\tgems: {";
                var gemStrings = new List<string>();
                foreach (var kvp in gems)
                {
                    gemStrings.Add($"{kvp.Key}: {kvp.Value}");
                }
                output += string.Join(", ", gemStrings);
                output += "}\n";
                
                // 格式化 cards
                output += "\tcards: {";
                var cardStrings = new List<string>();
                foreach (var kvp in cards)
                {
                    cardStrings.Add($"{kvp.Key}: {kvp.Value.Count}");
                }
                output += string.Join(", ", cardStrings);
                output += "}\n";
                
                // 格式化 nobles
                output += "\tnobles: ";
                if (nobles.Count == 0)
                {
                    output += "0.\n";
                }
                else
                {
                    var nobleStrings = new List<string>();
                    foreach (var noble in nobles)
                    {
                        nobleStrings.Add(noble.Code);
                    }
                    output += string.Join(", ", nobleStrings) + ".\n";
                }
                
                return output;
            }
        }
    }
}