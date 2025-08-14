using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.Models
{
    /// <summary>
    /// 卡牌模型，使用ScriptableObject便于序列化
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Splendor/Card")]
    public class Card : ScriptableObject
    {
        [Header("卡牌基本信息")]
        public string Code = "";
        public string Colour = "";
        public int DeckId = 0;
        public int Points = 0;
        
        [Header("卡牌成本")]
        public Dictionary<string, int> Cost = new Dictionary<string, int>();
        
        /// <summary>
        /// 创建卡牌
        /// </summary>
        public static Card CreateCard(string code, string colour, int deckId, int points, Dictionary<string, int> cost)
        {
            var card = CreateInstance<Card>();
            card.Code = code;
            card.Colour = colour;
            card.DeckId = deckId;
            card.Points = points;
            card.Cost = cost ?? new Dictionary<string, int>();
            return card;
        }
        
        /// <summary>
        /// 深度复制卡牌
        /// </summary>
        public Card DeepCopy()
        {
            var newCard = CreateInstance<Card>();
            newCard.Code = this.Code;
            newCard.Colour = this.Colour;
            newCard.DeckId = this.DeckId;
            newCard.Points = this.Points;
            
            // 复制成本
            foreach (var kvp in Cost)
            {
                newCard.Cost[kvp.Key] = kvp.Value;
            }
            
            return newCard;
        }
        
        /// <summary>
        /// 检查卡牌是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Card other)
            {
                return Code == other.Code &&
                       Colour == other.Colour &&
                       DeckId == other.DeckId &&
                       Points == other.Points &&
                       CostEqual(Cost, other.Cost);
            }
            return false;
        }
        
        /// <summary>
        /// 检查成本是否相等
        /// </summary>
        private bool CostEqual(Dictionary<string, int> cost1, Dictionary<string, int> cost2)
        {
            if (cost1.Count != cost2.Count)
                return false;
                
            foreach (var kvp in cost1)
            {
                if (!cost2.ContainsKey(kvp.Key) || cost2[kvp.Key] != kvp.Value)
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return Code.GetHashCode() ^ Colour.GetHashCode() ^ DeckId.GetHashCode();
        }
        
        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            return $"Card({Code}, {Colour}, Tier{DeckId + 1}, {Points}pts)";
        }
    }
}