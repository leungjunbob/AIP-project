using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.Models
{
    /// <summary>
    /// 贵族模型
    /// </summary>
    [CreateAssetMenu(fileName = "Noble", menuName = "Splendor/Noble")]
    public class Noble : ScriptableObject
    {
        [Header("贵族基本信息")]
        public string Code = "";
        public int Points = 3; // 贵族总是给3分
        
        [Header("贵族要求")]
        public Dictionary<string, int> Requirements = new Dictionary<string, int>();
        
        /// <summary>
        /// 创建贵族
        /// </summary>
        public static Noble CreateNoble(string code, Dictionary<string, int> requirements)
        {
            var noble = CreateInstance<Noble>();
            noble.Code = code;
            noble.Requirements = requirements ?? new Dictionary<string, int>();
            return noble;
        }
        
        /// <summary>
        /// 深度复制贵族
        /// </summary>
        public Noble DeepCopy()
        {
            var newNoble = CreateInstance<Noble>();
            newNoble.Code = this.Code;
            newNoble.Points = this.Points;
            
            // 复制要求
            foreach (var kvp in Requirements)
            {
                newNoble.Requirements[kvp.Key] = kvp.Value;
            }
            
            return newNoble;
        }
        
        /// <summary>
        /// 检查贵族是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Noble other)
            {
                return Code == other.Code &&
                       Points == other.Points &&
                       RequirementsEqual(Requirements, other.Requirements);
            }
            return false;
        }
        
        /// <summary>
        /// 检查要求是否相等
        /// </summary>
        private bool RequirementsEqual(Dictionary<string, int> req1, Dictionary<string, int> req2)
        {
            if (req1.Count != req2.Count)
                return false;
                
            foreach (var kvp in req1)
            {
                if (!req2.ContainsKey(kvp.Key) || req2[kvp.Key] != kvp.Value)
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return Code.GetHashCode() ^ Points.GetHashCode();
        }
        
        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            return $"Noble({Code}, {Points}pts)";
        }
        
        /// <summary>
        /// 检查玩家是否满足贵族要求
        /// </summary>
        public bool CheckRequirements(Dictionary<string, int> playerCards)
        {
            foreach (var requirement in Requirements)
            {
                if (!playerCards.ContainsKey(requirement.Key) || 
                    playerCards[requirement.Key] < requirement.Value)
                {
                    return false;
                }
            }
            return true;
        }
    }
}