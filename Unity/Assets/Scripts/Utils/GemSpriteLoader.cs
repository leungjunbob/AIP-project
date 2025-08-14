using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.Utils
{
    /// <summary>
    /// 宝石图片加载器，自动从Resources目录加载宝石图片
    /// </summary>
    public static class GemSpriteLoader
    {
        private static readonly string[] gemTypes = { "black", "red", "yellow", "green", "blue", "white" };
        private static readonly string[] sizeTypes = { "small", "large" };
        
        /// <summary>
        /// 获取宝石类型的最大数量
        /// </summary>
        private static int GetMaxGemCount(string gemType)
        {
            // yellow宝石最多5个，其他宝石最多7个
            return gemType == "yellow" ? 5 : 7;
        }
        
        /// <summary>
        /// 加载指定类型和尺寸的宝石图片数组
        /// </summary>
        /// <param name="gemType">宝石类型</param>
        /// <param name="size">尺寸类型 ("small" 或 "large")</param>
        /// <returns>宝石图片数组，索引0对应数量0，索引N对应数量N</returns>
        public static Sprite[] LoadGemSprites(string gemType, string size = "large")
        {
            int maxCount = GetMaxGemCount(gemType);
            var sprites = new Sprite[maxCount + 1]; // 支持数量0到maxCount
            
            for (int i = 0; i <= maxCount; i++)
            {
                string spriteName = $"{gemType}_{i}";
                string resourcePath = $"gems_{size}/{spriteName}";
                
                Sprite sprite = Resources.Load<Sprite>(resourcePath);
                if (sprite != null)
                {
                    sprites[i] = sprite; // 数量i对应索引i
                }
                else
                {
                    Debug.LogWarning($"GemSpriteLoader: 无法加载图片 {resourcePath}");
                }
            }
            
            return sprites;
        }
        
        /// <summary>
        /// 加载所有宝石类型的图片
        /// </summary>
        /// <param name="size">尺寸类型</param>
        /// <returns>宝石类型到图片数组的字典</returns>
        public static Dictionary<string, Sprite[]> LoadAllGemSprites(string size = "large")
        {
            var allSprites = new Dictionary<string, Sprite[]>();
            
            foreach (string gemType in gemTypes)
            {
                allSprites[gemType] = LoadGemSprites(gemType, size);
            }
            
            return allSprites;
        }
        
        /// <summary>
        /// 验证宝石图片是否完整
        /// </summary>
        /// <param name="sprites">图片数组</param>
        /// <param name="gemType">宝石类型</param>
        /// <returns>是否完整</returns>
        public static bool ValidateGemSprites(Sprite[] sprites, string gemType)
        {
            int expectedLength = GetMaxGemCount(gemType) + 1;
            
            if (sprites == null || sprites.Length < expectedLength)
            {
                Debug.LogError($"GemSpriteLoader: {gemType} 宝石图片数组不完整，期望长度 {expectedLength}，实际长度 {sprites?.Length ?? 0}");
                return false;
            }
            
            for (int i = 0; i < expectedLength; i++)
            {
                if (sprites[i] == null)
                {
                    Debug.LogWarning($"GemSpriteLoader: {gemType} 宝石数量 {i} 的图片缺失");
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取宝石类型的最大数量（供外部使用）
        /// </summary>
        public static int GetGemTypeMaxCount(string gemType)
        {
            return GetMaxGemCount(gemType);
        }
    }
}