using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SplendorUnity.Core;
using SplendorUnity.Utils;

namespace SplendorUnity.Display
{
    /// <summary>
    /// GemArea管理器，专门用于Scene1中的GemArea
    /// 根据gamestate中银行的gem数量自动更新显示
    /// </summary>
    public class GemAreaManager : MonoBehaviour
    {
        [Header("GemArea引用")]
        public Transform gemArea; // Scene1中的GemArea对象
        
        [Header("Gem UI组件")]
        public Image blackGemImage;
        public Image redGemImage;
        public Image yellowGemImage;
        public Image greenGemImage;
        public Image blueGemImage;
        public Image whiteGemImage;
        
        [Header("配置")]
        public bool autoLoadSprites = true;
        public string spriteSize = "small"; // "small" 或 "large"
        
        private Dictionary<string, Image> gemImages;
        private Dictionary<string, Sprite[]> gemSprites;
        private SplendorGameState currentGameState;
        
        private void Awake()
        {
            InitializeGemArea();
        }
        
        /// <summary>
        /// 初始化GemArea
        /// </summary>
        private void InitializeGemArea()
        {
            // 自动查找GemArea
            if (gemArea == null)
            {
                gemArea = GameObject.Find("GemArea")?.transform;
                if (gemArea == null)
                {
                    return;
                }
            }
            
            // 初始化gem图片引用
            InitializeGemImageReferences();
            
            // 自动加载宝石图片
            if (autoLoadSprites)
            {
                LoadAllGemSprites();
            }
        }
        
        /// <summary>
        /// 初始化gem图片引用
        /// </summary>
        private void InitializeGemImageReferences()
        {
            gemImages = new Dictionary<string, Image>();
            
            // 如果手动指定了图片组件，使用指定的
            if (blackGemImage != null) gemImages["black"] = blackGemImage;
            if (redGemImage != null) gemImages["red"] = redGemImage;
            if (yellowGemImage != null) gemImages["yellow"] = yellowGemImage;
            if (greenGemImage != null) gemImages["green"] = greenGemImage;
            if (blueGemImage != null) gemImages["blue"] = blueGemImage;
            if (whiteGemImage != null) gemImages["white"] = whiteGemImage;
            
            // 如果没有手动指定，尝试自动查找
            if (gemArea != null)
            {
                AutoFindGemImages();
            }
        }
        
        /// <summary>
        /// 自动查找gem图片组件
        /// </summary>
        private void AutoFindGemImages()
        {
            string[] gemTypes = { "black", "red", "yellow", "green", "blue", "white" };
            
            foreach (string gemType in gemTypes)
            {
                if (!gemImages.ContainsKey(gemType) || gemImages[gemType] == null)
                {
                    // 尝试查找包含gem类型名称的Image组件
                    Image[] images = gemArea.GetComponentsInChildren<Image>();
                    foreach (Image img in images)
                    {
                        if (img.name.ToLower().Contains(gemType))
                        {
                            gemImages[gemType] = img;
                            break;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 自动加载所有宝石图片
        /// </summary>
        private void LoadAllGemSprites()
        {
                
            var loadedSprites = GemSpriteLoader.LoadAllGemSprites(spriteSize);
            gemSprites = new Dictionary<string, Sprite[]>();
            
            foreach (var kvp in loadedSprites)
            {
                string gemType = kvp.Key;
                Sprite[] sprites = kvp.Value;
                
                if (GemSpriteLoader.ValidateGemSprites(sprites, gemType))
                {
                    gemSprites[gemType] = sprites;
                }
            }
        }
        
        /// <summary>
        /// 更新GemArea显示
        /// </summary>
        /// <param name="gameState">游戏状态</param>
        public void UpdateGemArea(SplendorGameState gameState)
        {
            if (gameState?.board?.gems == null)
            {
                return;
            }
            
            currentGameState = gameState;
            
            foreach (var gem in gameState.board.gems)
            {
                UpdateGemDisplay(gem.Key, gem.Value);
            }
            
        }
        
        /// <summary>
        /// 更新单个gem的显示
        /// </summary>
        /// <param name="gemType">宝石类型</param>
        /// <param name="count">数量</param>
        public void UpdateGemDisplay(string gemType, int count)
        {
            if (!gemImages.ContainsKey(gemType) || !gemSprites.ContainsKey(gemType))
            {
                return;
            }
            
            var image = gemImages[gemType];
            var sprites = gemSprites[gemType];
            
            if (image == null)
            {
                return;
            }
            
            // 计算正确的数组索引（数量1-7对应索引0-6）
            int spriteIndex = Mathf.Clamp(count - 1, 0, sprites.Length - 1);
            
            if (spriteIndex < sprites.Length && sprites[spriteIndex] != null)
            {
                image.sprite = sprites[spriteIndex];
            }
        }
        
        
        /// <summary>
        /// 获取所有gem Image组件的引用
        /// </summary>
        /// <returns>gem类型到Image组件的字典</returns>
        public Dictionary<string, Image> GetGemImages()
        {
            return gemImages;
        }
        
    }
}
