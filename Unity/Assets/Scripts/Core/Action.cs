using UnityEngine;
using System.Collections.Generic;
using SplendorUnity.Models;

namespace SplendorUnity.Core
{
    /// <summary>
    /// 动作类，定义收集宝石、购买卡牌、保留卡牌、贵族访问等动作
    /// </summary>
    [System.Serializable]
    public class Action
    {
        // 动作基本信息
        public string Type { get; set; } = "";
        public string Code { get; set; } = "";
        
        // 卡牌相关
        public Card Card { get; set; }
        
        // 宝石相关
        public Dictionary<string, int> CollectedGems { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ReturnedGems { get; set; } = new Dictionary<string, int>();
        
        // 贵族相关
        public Noble Noble { get; set; }
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Action()
        {
            CollectedGems = new Dictionary<string, int>();
            ReturnedGems = new Dictionary<string, int>();
        }
        
        /// <summary>
        /// 创建收集宝石动作
        /// </summary>
        public static Action CreateCollectAction(Dictionary<string, int> collectedGems, Dictionary<string, int> returnedGems = null)
        {
            var action = new Action
            {
                Type = "collect",
                CollectedGems = collectedGems ?? new Dictionary<string, int>(),
                ReturnedGems = returnedGems ?? new Dictionary<string, int>()
            };
            return action;
        }
        
        /// <summary>
        /// 创建购买卡牌动作
        /// </summary>
        public static Action CreateBuyAction(Card card, bool isReserved = false, Dictionary<string, int> returnedGems = null, Noble noble = null)
        {
            var action = new Action
            {
                Type = isReserved ? "buy_reserved" : "buy",
                Card = card,
                ReturnedGems = returnedGems ?? new Dictionary<string, int>(),
                Noble = noble
            };
            return action;
        }
        
        /// <summary>
        /// 创建保留卡牌动作
        /// </summary>
        public static Action CreateReserveAction(Card card, Dictionary<string, int> collectedGems = null, Dictionary<string, int> returnedGems = null)
        {
            var action = new Action
            {
                Type = "reserve",
                Card = card,
                CollectedGems = collectedGems ?? new Dictionary<string, int>(),
                ReturnedGems = returnedGems ?? new Dictionary<string, int>()
            };
            return action;
        }
        
        /// <summary>
        /// 创建贵族访问动作
        /// </summary>
        public static Action CreateNobleAction(Noble noble)
        {
            var action = new Action
            {
                Type = "noble",
                Noble = noble
            };
            return action;
        }
        
        /// <summary>
        /// 创建跳过动作
        /// </summary>
        public static Action CreatePassAction()
        {
            var action = new Action
            {
                Type = "pass"
            };
            return action;
        }
        
        /// <summary>
        /// 深度复制动作
        /// </summary>
        public Action DeepCopy()
        {
            var newAction = new Action
            {
                Type = this.Type,
                Code = this.Code,
                Card = this.Card?.DeepCopy(),
                Noble = this.Noble?.DeepCopy()
            };
            
            // 复制收集的宝石
            foreach (var kvp in CollectedGems)
            {
                newAction.CollectedGems[kvp.Key] = kvp.Value;
            }
            
            // 复制归还的宝石
            foreach (var kvp in ReturnedGems)
            {
                newAction.ReturnedGems[kvp.Key] = kvp.Value;
            }
            
            return newAction;
        }
        
        /// <summary>
        /// 检查动作是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Action other)
            {
                return Type == other.Type &&
                       Code == other.Code &&
                       Equals(Card, other.Card) &&
                       Equals(Noble, other.Noble) &&
                       GemsEqual(CollectedGems, other.CollectedGems) &&
                       GemsEqual(ReturnedGems, other.ReturnedGems);
            }
            return false;
        }
        
        /// <summary>
        /// 检查宝石字典是否相等
        /// </summary>
        private bool GemsEqual(Dictionary<string, int> gems1, Dictionary<string, int> gems2)
        {
            if (gems1.Count != gems2.Count)
                return false;
                
            foreach (var kvp in gems1)
            {
                if (!gems2.ContainsKey(kvp.Key) || gems2[kvp.Key] != kvp.Value)
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Code.GetHashCode();
        }
        
        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            switch (Type)
            {
                case "collect":
                    return $"Collect: {GemsToString(CollectedGems)}";
                case "buy":
                case "buy_reserved":
                    return $"Buy: {Card?.Code ?? "Unknown"}";
                case "reserve":
                    return $"Reserve: {Card?.Code ?? "Unknown"}";
                case "noble":
                    return $"Noble: {Noble?.Code ?? "Unknown"}";
                case "pass":
                    return "Pass";
                default:
                    return $"Action: {Type}";
            }
        }
        
        /// <summary>
        /// 将宝石字典转换为字符串
        /// </summary>
        private string GemsToString(Dictionary<string, int> gems)
        {
            if (gems.Count == 0)
                return "None";
                
            var parts = new List<string>();
            foreach (var kvp in gems)
            {
                parts.Add($"{kvp.Value} {kvp.Key}");
            }
            return string.Join(", ", parts);
        }
    }
}