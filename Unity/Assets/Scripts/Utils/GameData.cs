using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SplendorUnity.Utils
{
    /// <summary>
    /// 代理追踪类
    /// </summary>
    [System.Serializable]
    public class AgentTrace
    {
        public int Id { get; set; }
        public List<Tuple<SplendorUnity.Core.Action, int>> ActionReward { get; set; }

        public AgentTrace(int id)
        {
            Id = id;
            ActionReward = new List<Tuple<SplendorUnity.Core.Action, int>>();
        }

        public AgentTrace DeepCopy()
        {
            var newTrace = new AgentTrace(Id);
            foreach (var actionReward in ActionReward)
            {
                newTrace.ActionReward.Add(new Tuple<SplendorUnity.Core.Action, int>(actionReward.Item1.DeepCopy(), actionReward.Item2));
            }
            return newTrace;
        }
    }

    /// <summary>
    /// Splendor游戏数据工具类，替代原SplendorUtils
    /// </summary>
    public static class GameData
    {
        // 卡牌数据 - 格式：code : (colour, cost, deck_id, points)
        public static readonly Dictionary<string, (string colour, Dictionary<string, int> cost, int deckId, int points)> CARDS = new Dictionary<string, (string, Dictionary<string, int>, int, int)>
        {
            {"1g1w1r1b", ("black", new Dictionary<string, int>{{"green", 1}, {"white", 1}, {"red", 1}, {"blue", 1}}, 1, 0)},
            {"1g1w1r2b", ("black", new Dictionary<string, int>{{"green", 1}, {"white", 1}, {"red", 1}, {"blue", 2}}, 1, 0)},
            {"2b1r2w", ("black", new Dictionary<string, int>{{"blue", 2}, {"red", 1}, {"white", 2}}, 1, 0)},
            {"2b2g3w", ("black", new Dictionary<string, int>{{"blue", 2}, {"green", 2}, {"white", 3}}, 2, 1)},
            {"2g1r", ("black", new Dictionary<string, int>{{"green", 2}, {"red", 1}}, 1, 0)},
            {"2w2g", ("black", new Dictionary<string, int>{{"white", 2}, {"green", 2}}, 1, 0)},
            {"3g", ("black", new Dictionary<string, int>{{"green", 3}}, 1, 0)},
            {"3g2B3w", ("black", new Dictionary<string, int>{{"green", 3}, {"black", 2}, {"white", 3}}, 2, 1)},
            {"3r1B1g", ("black", new Dictionary<string, int>{{"red", 3}, {"black", 1}, {"green", 1}}, 1, 0)},
            {"4b", ("black", new Dictionary<string, int>{{"blue", 4}}, 1, 1)},
            {"4g2r1b", ("black", new Dictionary<string, int>{{"green", 4}, {"red", 2}, {"blue", 1}}, 2, 2)},
            {"5g3r", ("black", new Dictionary<string, int>{{"green", 5}, {"red", 3}}, 2, 2)},
            {"5g3w3r3b", ("black", new Dictionary<string, int>{{"green", 5}, {"white", 3}, {"red", 3}, {"blue", 3}}, 3, 3)},
            {"5w", ("black", new Dictionary<string, int>{{"white", 5}}, 2, 2)},
            {"6B", ("black", new Dictionary<string, int>{{"black", 6}}, 2, 3)},
            {"6r3B3g", ("black", new Dictionary<string, int>{{"red", 6}, {"black", 3}, {"green", 3}}, 3, 4)},
            {"7r", ("black", new Dictionary<string, int>{{"red", 7}}, 3, 4)},
            {"7r3B", ("black", new Dictionary<string, int>{{"red", 7}, {"black", 3}}, 3, 5)},
            {"1r1w1B1g", ("blue", new Dictionary<string, int>{{"red", 1}, {"white", 1}, {"black", 1}, {"green", 1}}, 1, 0)},
            {"1r4B2w", ("blue", new Dictionary<string, int>{{"red", 1}, {"black", 4}, {"white", 2}}, 2, 2)},
            {"1w2B", ("blue", new Dictionary<string, int>{{"white", 1}, {"black", 2}}, 1, 0)},
            {"2g2B", ("blue", new Dictionary<string, int>{{"green", 2}, {"black", 2}}, 1, 0)},
            {"2g2r1w", ("blue", new Dictionary<string, int>{{"green", 2}, {"red", 2}, {"white", 1}}, 1, 0)},
            {"2g3r2b", ("blue", new Dictionary<string, int>{{"green", 2}, {"red", 3}, {"blue", 2}}, 2, 1)},
            {"2r1w1B1g", ("blue", new Dictionary<string, int>{{"red", 2}, {"white", 1}, {"black", 1}, {"green", 1}}, 1, 0)},
            {"3B", ("blue", new Dictionary<string, int>{{"black", 3}}, 1, 0)},
            {"3b3B6w", ("blue", new Dictionary<string, int>{{"blue", 3}, {"black", 3}, {"white", 6}}, 3, 4)},
            {"3g1r1b", ("blue", new Dictionary<string, int>{{"green", 3}, {"red", 1}, {"blue", 1}}, 1, 0)},
            {"3g3B2b", ("blue", new Dictionary<string, int>{{"green", 3}, {"black", 3}, {"blue", 2}}, 2, 1)},
            {"3r3w5B3g", ("blue", new Dictionary<string, int>{{"red", 3}, {"white", 3}, {"black", 5}, {"green", 3}}, 3, 3)},
            {"4r", ("blue", new Dictionary<string, int>{{"red", 4}}, 1, 1)},
            {"5b", ("blue", new Dictionary<string, int>{{"blue", 5}}, 2, 2)},
            {"5w3b", ("blue", new Dictionary<string, int>{{"white", 5}, {"blue", 3}}, 2, 2)},
            {"6b", ("blue", new Dictionary<string, int>{{"blue", 6}}, 2, 3)},
            {"7w", ("blue", new Dictionary<string, int>{{"white", 7}}, 3, 4)},
            {"7w3b", ("blue", new Dictionary<string, int>{{"white", 7}, {"blue", 3}}, 3, 5)},
            {"1r1w1B1b", ("green", new Dictionary<string, int>{{"red", 1}, {"white", 1}, {"black", 1}, {"blue", 1}}, 1, 0)},
            {"1r1w2B1b", ("green", new Dictionary<string, int>{{"red", 1}, {"white", 1}, {"black", 2}, {"blue", 1}}, 1, 0)},
            {"2b1B4w", ("green", new Dictionary<string, int>{{"blue", 2}, {"black", 1}, {"white", 4}}, 2, 2)},
            {"2b2r", ("green", new Dictionary<string, int>{{"blue", 2}, {"red", 2}}, 1, 0)},
            {"2g3r3w", ("green", new Dictionary<string, int>{{"green", 2}, {"red", 3}, {"white", 3}}, 2, 1)},
            {"2r2B1b", ("green", new Dictionary<string, int>{{"red", 2}, {"black", 2}, {"blue", 1}}, 1, 0)},
            {"2w1b", ("green", new Dictionary<string, int>{{"white", 2}, {"blue", 1}}, 1, 0)},
            {"3b1g1w", ("green", new Dictionary<string, int>{{"blue", 3}, {"green", 1}, {"white", 1}}, 1, 0)},
            {"3b2B2w", ("green", new Dictionary<string, int>{{"blue", 3}, {"black", 2}, {"white", 2}}, 2, 1)},
            {"3r", ("green", new Dictionary<string, int>{{"red", 3}}, 1, 0)},
            {"3r5w3B3b", ("green", new Dictionary<string, int>{{"red", 3}, {"white", 5}, {"black", 3}, {"blue", 3}}, 3, 3)},
            {"4B", ("green", new Dictionary<string, int>{{"black", 4}}, 1, 1)},
            {"5b3g", ("green", new Dictionary<string, int>{{"blue", 5}, {"green", 3}}, 2, 2)},
            {"5g", ("green", new Dictionary<string, int>{{"green", 5}}, 2, 2)},
            {"6b3g3w", ("green", new Dictionary<string, int>{{"blue", 6}, {"green", 3}, {"white", 3}}, 3, 4)},
            {"6g", ("green", new Dictionary<string, int>{{"green", 6}}, 2, 3)},
            {"7b", ("green", new Dictionary<string, int>{{"blue", 7}}, 3, 4)},
            {"7b3g", ("green", new Dictionary<string, int>{{"blue", 7}, {"green", 3}}, 3, 5)},
            {"1g1w1B1b", ("red", new Dictionary<string, int>{{"green", 1}, {"white", 1}, {"black", 1}, {"blue", 1}}, 1, 0)},
            {"1g2B2w", ("red", new Dictionary<string, int>{{"green", 1}, {"black", 2}, {"white", 2}}, 1, 0)},
            {"1g2w1B1b", ("red", new Dictionary<string, int>{{"green", 1}, {"white", 2}, {"black", 1}, {"blue", 1}}, 1, 0)},
            {"1r3B1w", ("red", new Dictionary<string, int>{{"red", 1}, {"black", 3}, {"white", 1}}, 1, 0)},
            {"2b1g", ("red", new Dictionary<string, int>{{"blue", 2}, {"green", 1}}, 1, 0)},
            {"2r3B2w", ("red", new Dictionary<string, int>{{"red", 2}, {"black", 3}, {"white", 2}}, 2, 1)},
            {"2r3B3b", ("red", new Dictionary<string, int>{{"red", 2}, {"black", 3}, {"blue", 3}}, 2, 1)},
            {"2w2r", ("red", new Dictionary<string, int>{{"white", 2}, {"red", 2}}, 1, 0)},
            {"3g3w3B5b", ("red", new Dictionary<string, int>{{"green", 3}, {"white", 3}, {"black", 3}, {"blue", 5}}, 3, 3)},
            {"3w", ("red", new Dictionary<string, int>{{"white", 3}}, 1, 0)},
            {"3w5B", ("red", new Dictionary<string, int>{{"white", 3}, {"black", 5}}, 2, 2)},
            {"4b2g1w", ("red", new Dictionary<string, int>{{"blue", 4}, {"green", 2}, {"white", 1}}, 2, 2)},
            {"4w", ("red", new Dictionary<string, int>{{"white", 4}}, 1, 1)},
            {"5B", ("red", new Dictionary<string, int>{{"black", 5}}, 2, 2)},
            {"6g3r3b", ("red", new Dictionary<string, int>{{"green", 6}, {"red", 3}, {"blue", 3}}, 3, 4)},
            {"6r", ("red", new Dictionary<string, int>{{"red", 6}}, 2, 3)},
            {"7g", ("red", new Dictionary<string, int>{{"green", 7}}, 3, 4)},
            {"7g3r", ("red", new Dictionary<string, int>{{"green", 7}, {"red", 3}}, 3, 5)},
            {"1b1B3w", ("white", new Dictionary<string, int>{{"blue", 1}, {"black", 1}, {"white", 3}}, 1, 0)},
            {"1r1b1B1g", ("white", new Dictionary<string, int>{{"red", 1}, {"blue", 1}, {"black", 1}, {"green", 1}}, 1, 0)},
            {"1r1b1B2g", ("white", new Dictionary<string, int>{{"red", 1}, {"blue", 1}, {"black", 1}, {"green", 2}}, 1, 0)},
            {"2b2B", ("white", new Dictionary<string, int>{{"blue", 2}, {"black", 2}}, 1, 0)},
            {"2g1B2b", ("white", new Dictionary<string, int>{{"green", 2}, {"black", 1}, {"blue", 2}}, 1, 0)},
            {"2r1B", ("white", new Dictionary<string, int>{{"red", 2}, {"black", 1}}, 1, 0)},
            {"2r2B3g", ("white", new Dictionary<string, int>{{"red", 2}, {"black", 2}, {"green", 3}}, 2, 1)},
            {"3b", ("white", new Dictionary<string, int>{{"blue", 3}}, 1, 0)},
            {"3b3r2w", ("white", new Dictionary<string, int>{{"blue", 3}, {"red", 3}, {"white", 2}}, 2, 1)},
            {"3r6B3w", ("white", new Dictionary<string, int>{{"red", 3}, {"black", 6}, {"white", 3}}, 3, 4)},
            {"3w7B", ("white", new Dictionary<string, int>{{"white", 3}, {"black", 7}}, 3, 5)},
            {"4g", ("white", new Dictionary<string, int>{{"green", 4}}, 1, 1)},
            {"4r2B1g", ("white", new Dictionary<string, int>{{"red", 4}, {"black", 2}, {"green", 1}}, 2, 2)},
            {"5r", ("white", new Dictionary<string, int>{{"red", 5}}, 2, 2)},
            {"5r3B", ("white", new Dictionary<string, int>{{"red", 5}, {"black", 3}}, 2, 2)},
            {"5r3b3B3g", ("white", new Dictionary<string, int>{{"red", 5}, {"blue", 3}, {"black", 3}, {"green", 3}}, 3, 3)},
            {"6w", ("white", new Dictionary<string, int>{{"white", 6}}, 2, 3)},
            {"7B", ("white", new Dictionary<string, int>{{"black", 7}}, 3, 4)}
        };

        // 贵族数据 - 格式：(code, cost)
        public static readonly List<Tuple<string, Dictionary<string, int>>> NOBLES = new List<Tuple<string, Dictionary<string, int>>>
        {
            new Tuple<string, Dictionary<string, int>>("4g4r", new Dictionary<string, int>{{"green", 4}, {"red", 4}}),
            new Tuple<string, Dictionary<string, int>>("3w3r3B", new Dictionary<string, int>{{"white", 3}, {"red", 3}, {"black", 3}}),
            new Tuple<string, Dictionary<string, int>>("3b3g3r", new Dictionary<string, int>{{"blue", 3}, {"green", 3}, {"red", 3}}),
            new Tuple<string, Dictionary<string, int>>("3w3b3g", new Dictionary<string, int>{{"white", 3}, {"blue", 3}, {"green", 3}}),
            new Tuple<string, Dictionary<string, int>>("4w4b", new Dictionary<string, int>{{"white", 4}, {"blue", 4}}),
            new Tuple<string, Dictionary<string, int>>("4w4B", new Dictionary<string, int>{{"white", 4}, {"black", 4}}),
            new Tuple<string, Dictionary<string, int>>("3w3b3B", new Dictionary<string, int>{{"white", 3}, {"blue", 3}, {"black", 3}}),
            new Tuple<string, Dictionary<string, int>>("4r4B", new Dictionary<string, int>{{"red", 4}, {"black", 4}}),
            new Tuple<string, Dictionary<string, int>>("4b4g", new Dictionary<string, int>{{"blue", 4}, {"green", 4}}),
            new Tuple<string, Dictionary<string, int>>("3g3r3B", new Dictionary<string, int>{{"green", 3}, {"red", 3}, {"black", 3}})
        };

        // 颜色映射
        public static readonly Dictionary<string, string> COLOURS = new Dictionary<string, string>
        {
            {"B", "black"},
            {"r", "red"},
            {"y", "yellow"},
            {"g", "green"},
            {"b", "blue"},
            {"w", "white"}
        };

        /// <summary>
        /// 转换文件名并返回游戏信息
        /// </summary>
        public static (string colour, string code, Dictionary<string, int> cost) ConvertFilename(string filename)
        {
            string f = filename.Substring(0, filename.Length - 4); // 移除扩展名
            
            if (char.IsDigit(f[f.Length - 1])) // 如果最后一个字符是数字，这是宝石资源
            {
                string[] parts = f.Split('_');
                if (parts.Length == 2)
                {
                    return (parts[0], int.Parse(parts[1]).ToString(), new Dictionary<string, int>());
                }
                else
                {
                    return (parts[0], int.Parse(parts[2]).ToString(), new Dictionary<string, int>());
                }
            }
            else // 否则这是卡牌资源
            {
                string colour = null;
                string code = f;
                
                if (char.IsLetter(f[0]))
                {
                    string[] parts = f.Split('_');
                    if (parts.Length >= 2)
                    {
                        colour = parts[0];
                        code = parts[1];
                    }
                }
                
                code = code.Replace("blu", "b").Replace("bla", "B");
                var cost = new Dictionary<string, int>();
                
                for (int i = 0; i < code.Length; i += 2)
                {
                    if (i + 1 < code.Length)
                    {
                        string colourKey = code[i + 1].ToString();
                        if (COLOURS.ContainsKey(colourKey))
                        {
                            cost[COLOURS[colourKey]] = int.Parse(code[i].ToString());
                        }
                    }
                }
                
                return (colour, code, cost);
            }
        }

        /// <summary>
        /// 将宝石字典转换为字符串
        /// </summary>
        public static string GemsToString(Dictionary<string, int> gemDict)
        {
            var gemCounts = gemDict.Where(kvp => kvp.Value > 0).ToList();
            
            if (gemCounts.Count == 0)
            {
                return "";
            }
            else if (gemCounts.Count == 1)
            {
                var kvp = gemCounts[0];
                return $"{kvp.Value} {kvp.Key} gem{(kvp.Value > 1 ? "s" : "")}";
            }
            else if (gemCounts.Count == 2)
            {
                return $"{gemCounts[0].Value} {gemCounts[0].Key} and {gemCounts[1].Value} {gemCounts[1].Key} gems";
            }
            else
            {
                var parts = new List<string>();
                for (int i = 0; i < gemCounts.Count - 1; i++)
                {
                    parts.Add($"{gemCounts[i].Value} {gemCounts[i].Key}");
                }
                parts.Add($"and {gemCounts[gemCounts.Count - 1].Value} {gemCounts[gemCounts.Count - 1].Key} gems");
                return string.Join(", ", parts);
            }
        }

        /// <summary>
        /// 将行动转换为字符串
        /// </summary>
        public static string ActionToString(int agentId, SplendorUnity.Core.Action action)
        {
            var desc = new StringBuilder();
            
            if (action.Type.Contains("collect"))
            {
                if (action.CollectedGems.Count > 0)
                {
                    if (action.ReturnedGems.Count > 0)
                    {
                        desc.Append($"Agent {agentId} collected {GemsToString(action.CollectedGems)}, exceeded the limit, and returned {GemsToString(action.ReturnedGems)}.");
                    }
                    else
                    {
                        desc.Append($"Agent {agentId} collected {GemsToString(action.CollectedGems)}.");
                    }
                }
            }
            else if (action.Card != null)
            {
                var card = action.Card;
                
                if (action.Type == "reserve")
                {
                    desc.Append($"Agent {agentId} reserved a Tier {card.DeckId + 1} {card.Colour} card ({card.Code}).");
                }
                else if (action.Type.Contains("buy"))
                {
                    if (card.Points > 0)
                    {
                        string prefix = action.Type.Contains("reserved") ? "previously reserved " : "";
                        string pointsText = card.Points > 1 ? "points" : "point";
                        desc.Append($"Agent {agentId} bought a {prefix}Tier {card.DeckId + 1} {card.Colour} card ({card.Code}), earning {card.Points} {pointsText}!");
                    }
                    else
                    {
                        string prefix = action.Type.Contains("reserved") ? "previously reserved " : "";
                        desc.Append($"Agent {agentId} bought a {prefix}Tier {card.DeckId + 1} {card.Colour} card ({card.Code}).");
                    }
                }
            }
            else if (action.Type == "pass")
            {
                desc.Append($"Agent {agentId} has no gems to take, and nothing to buy.");
            }
            
            if (action.Noble != null)
            {
                desc.Append(" A noble has also taken interest, earning 3 points!");
            }
            
            return desc.ToString();
        }

        /// <summary>
        /// 将代理状态转换为字符串
        /// </summary>
        public static string AgentToString(int agentId, object agentState)
        {
            // TODO: 实现具体的代理状态转换
            return $"Agent #{agentId} has scored points thus far.\n";
        }

        /// <summary>
        /// 将棋盘状态转换为字符串
        /// </summary>
        public static string BoardToString(object gameState)
        {
            // TODO: 实现具体的棋盘状态转换
            return "";
        }
    }
}