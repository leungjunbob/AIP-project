using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SplendorUnity.UI
{
    /// <summary>
    /// PlayerInfoDisplay设置助手 - 帮助快速创建和配置PlayerInfoDisplay的UI结构
    /// </summary>
    public class PlayerInfoDisplaySetup : MonoBehaviour
    {
        [Header("UI Settings")]
        public bool createUIOnStart = false; // 改为false，避免自动创建
        public Vector2 displaySize = new Vector2(300, 205);
        public Vector2 displayPosition = new Vector2(0, 0);
        
        [Header("Player Settings")]
        public string playerName = "Player 1";
        public int playerId = 0;
        public Color playerNameColor = Color.white;
        public Color playerScoreColor = Color.yellow;
        
        [Header("Gem Display Settings")]
        public Vector2 gemSize = new Vector2(40, 40);
        public float gemSpacing = 49f; // 计算：(280 - 35) / 5 = 49，确保6个宝石完全显示在300像素内
        
        [Header("Text Settings")]
        public int playerNameFontSize = 20;
        public int playerScoreFontSize = 18;
        
        
        private Canvas canvas;
        private PlayerInfoDisplay playerInfoDisplay;
        
        private void Start()
        {
        }
        
        /// <summary>
        /// 创建PlayerInfoDisplay的UI结构
        /// </summary>
        [ContextMenu("Create PlayerInfoDisplay UI")]
        public void CreatePlayerInfoDisplayUI()
        {
            // Find or create Canvas
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            // Create PlayerInfoDisplay container
            GameObject displayGO = new GameObject($"PlayerInfoDisplay_{playerId}");
            displayGO.transform.SetParent(canvas.transform, false);
            
            // Add PlayerInfoDisplay component
            playerInfoDisplay = displayGO.AddComponent<PlayerInfoDisplay>();
            
            // Set RectTransform
            RectTransform rectTransform = displayGO.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = displayGO.AddComponent<RectTransform>();
            }
            
            rectTransform.sizeDelta = displaySize;
            rectTransform.anchoredPosition = displayPosition;
            rectTransform.anchorMin = new Vector2(0f, 1f); // Top Left
            rectTransform.anchorMax = new Vector2(0f, 1f); // Top Left
            rectTransform.pivot = new Vector2(0f, 1f); // Top Left
            
            // Create UI components
            CreatePlayerInfoComponents(displayGO);
            CreateGemDisplayComponents(displayGO);
            CreateCardDisplayComponents(displayGO);
            
            // 创建贵族显示组件（第六行）
            CreateNobleDisplayComponents(displayGO);
            
            // Configure PlayerInfoDisplay
            playerInfoDisplay.playerName = playerName;
            playerInfoDisplay.playerId = playerId;
            
            
            // 验证组件分配
            ValidateComponentAssignment();
        }
        
        /// <summary>
        /// 验证组件是否正确分配
        /// </summary>
        private void ValidateComponentAssignment()
        {
            if (playerInfoDisplay == null) return;
            
            
            // 验证宝石图片组件
            var gemImages = new Dictionary<string, Image>
            {
                {"black", playerInfoDisplay.blackGemImage},
                {"red", playerInfoDisplay.redGemImage},
                {"yellow", playerInfoDisplay.yellowGemImage},
                {"green", playerInfoDisplay.greenGemImage},
                {"blue", playerInfoDisplay.blueGemImage},
                {"white", playerInfoDisplay.whiteGemImage}
            };
            
            
        }
        
        /// <summary>
        /// 创建玩家信息组件（第一行）
        /// </summary>
        private void CreatePlayerInfoComponents(GameObject parent)
        {
            
            // Create player name text
            CreateTextComponent(parent, "PlayerNameText", playerName, 
                new Vector2(70, -20), new Vector2(150, 30), playerNameFontSize, playerNameColor);
            
            // Create player score text
            CreateTextComponent(parent, "PlayerScoreText", $"Score: 0", 
                new Vector2(220, -20), new Vector2(120, 30), playerScoreFontSize, playerScoreColor);
            
        }
        
        /// <summary>
        /// 创建宝石显示组件（第二行）
        /// </summary>
        private void CreateGemDisplayComponents(GameObject parent)
        {
            
            string[] gemTypes = { "black", "red", "yellow", "green", "blue", "white" };
            float startX = 25f; // 从框内15像素开始显示6个宝石，留出合适的左边距
            
            for (int i = 0; i < gemTypes.Length; i++)
            {
                string gemType = gemTypes[i];
                float xPos = startX + i * gemSpacing;
                
                
                // Create gem image
                CreateGemImage(parent, $"{gemType}GemImage", gemType, 
                    new Vector2(xPos, -60), gemSize);
                
                // 不再创建gem count text，只使用图片表示数量
            }
            
        }
        
        /// <summary>
        /// 创建卡牌显示组件（第三、四、五行）
        /// </summary>
        private void CreateCardDisplayComponents(GameObject parent)
        {
            
            // 创建已购买卡牌容器（第三、四行）
            CreatePurchasedCardsContainer(parent);
            
            // 创建保留卡牌容器（第五行）
            CreateReservedCardsContainer(parent);
        }
        
        /// <summary>
        /// 创建已购买卡牌容器
        /// </summary>
        private void CreatePurchasedCardsContainer(GameObject parent)
        {
            GameObject containerGO = new GameObject("PurchasedCardsContainer");
            containerGO.transform.SetParent(parent.transform, false);
            
            // 添加RectTransform组件
            RectTransform rectTransform = containerGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280, 108); // 宽度280，高度72（适应两行，减少间距）
            rectTransform.anchoredPosition = new Vector2(10, -90); // 向下移动，为卡牌3行布局留出空间
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            
            // 分配给PlayerInfoDisplay
            if (playerInfoDisplay != null)
            {
                playerInfoDisplay.purchasedCardsContainer = containerGO.transform;
            }
        }
        
        /// <summary>
        /// 创建保留卡牌容器
        /// </summary>
        private void CreateReservedCardsContainer(GameObject parent)
        {
            GameObject containerGO = new GameObject("ReservedCardsContainer");
            containerGO.transform.SetParent(parent.transform, false);
            
            // 添加RectTransform组件
            RectTransform rectTransform = containerGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280, 50); // 宽度280，高度50
            rectTransform.anchoredPosition = new Vector2(110, -270); // 向下移动，为卡牌3行布局留出空间
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            
            // 创建"Reserved Card"标签
            CreateReservedCardsLabel(containerGO);
            
            // 分配给PlayerInfoDisplay
            if (playerInfoDisplay != null)
            {
                playerInfoDisplay.reservedCardsContainer = containerGO.transform;
            }
        }
        
        /// <summary>
        /// 创建保留卡牌标签
        /// </summary>
        private void CreateReservedCardsLabel(GameObject parent)
        {
            GameObject labelGO = new GameObject("ReservedCardsLabel");
            labelGO.transform.SetParent(parent.transform, false);
            
            // 添加RectTransform组件
            RectTransform rectTransform = labelGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 20); // 标签宽度
            rectTransform.anchoredPosition = new Vector2(-100, -10); // 标签位置，向下移动10像素
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            
            // 添加TextMeshProUGUI组件
            TextMeshProUGUI tmpText = labelGO.AddComponent<TextMeshProUGUI>();
            tmpText.text = "Reserved Card:";
            tmpText.fontSize = 14;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.fontStyle = FontStyles.Bold;
            
            // 确保字体资源可用
            if (TMP_Settings.defaultFontAsset != null)
            {
                tmpText.font = TMP_Settings.defaultFontAsset;
            }
            
            // 分配给PlayerInfoDisplay
            if (playerInfoDisplay != null)
            {
                playerInfoDisplay.reservedCardsLabel = tmpText;

            }
        }
        
        /// <summary>
        /// 创建贵族显示组件（第六行）
        /// </summary>
        private void CreateNobleDisplayComponents(GameObject parent)
        {
            
            // 创建贵族容器
            CreateNoblesContainer(parent);
        }
        
        /// <summary>
        /// 创建贵族容器
        /// </summary>
        private void CreateNoblesContainer(GameObject parent)
        {
            GameObject containerGO = new GameObject("NoblesContainer");
            containerGO.transform.SetParent(parent.transform, false);
            
            // 添加RectTransform组件
            RectTransform rectTransform = containerGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280, 40); // 宽度280，高度40
            rectTransform.anchoredPosition = new Vector2(10, -200); // 向下移动，为卡牌3行布局留出空间
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            
            // 创建"Nobles"标签
            CreateNoblesLabel(containerGO);
            
            // 分配给PlayerInfoDisplay
            if (playerInfoDisplay != null)
            {
                playerInfoDisplay.noblesContainer = containerGO.transform;
            }
        }
        
        /// <summary>
        /// 创建贵族标签（已移除文字显示）
        /// </summary>
        private void CreateNoblesLabel(GameObject parent)
        {
            // 不再创建标签文字，只保留方法以维持接口兼容性
        }
        
        /// <summary>
        /// 创建宝石图片组件
        /// </summary>
        private void CreateGemImage(GameObject parent, string name, string gemType, 
            Vector2 position, Vector2 size)
        {
            GameObject imageGO = new GameObject(name);
            imageGO.transform.SetParent(parent.transform, false);
            
            // Add Image component
            Image image = imageGO.AddComponent<Image>();
            
            // 设置Image为完全透明，避免底色
            image.color = new Color(1f, 1f, 1f, 0f); // 完全透明
            
            // 加载正确的gem sprite - 初始显示none sprite
            Sprite gemSprite = LoadGemSprite(gemType, 0); // 初始显示0个gem
            if (gemSprite != null)
            {
                image.sprite = gemSprite;
                // 设置sprite后，再次确保颜色为透明
                image.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                // 如果加载失败，尝试加载none sprite
                string nonePath = $"gems_large/{gemType}_none";
                Sprite noneSprite = Resources.Load<Sprite>(nonePath);
                if (noneSprite != null)
                {
                    image.sprite = noneSprite;
                }
                else
                {
                    // 如果还是失败，使用颜色作为fallback，但保持透明
                    Color gemColor = GetGemColor(gemType);
                    image.color = new Color(gemColor.r, gemColor.g, gemColor.b, 0f); // 保持透明
                }
            }
            
            // Set RectTransform
            RectTransform rectTransform = imageGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
            rectTransform.anchorMin = new Vector2(0f, 1f); // Top Left
            rectTransform.anchorMax = new Vector2(0f, 1f); // Top Left
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Assign to PlayerInfoDisplay
            AssignGemImageToPlayerInfoDisplay(gemType, image);
            
        }
        
        /// <summary>
        /// 创建文本组件
        /// </summary>
        private void CreateTextComponent(GameObject parent, string name, string text, 
            Vector2 position, Vector2 size, int fontSize, Color color)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);
            
            // Add TextMeshProUGUI component
            TextMeshProUGUI tmpText = textGO.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = color;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;
            
            // Add shadow effect
            Shadow shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(1, -1);
            
            // Set RectTransform
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
            rectTransform.anchorMin = new Vector2(0f, 1f); // Top Left
            rectTransform.anchorMax = new Vector2(0f, 1f); // Top Left
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Assign to PlayerInfoDisplay
            AssignTextComponentToPlayerInfoDisplay(name, tmpText);
        }
        
        /// <summary>
        /// 将宝石图片分配给PlayerInfoDisplay
        /// </summary>
        private void AssignGemImageToPlayerInfoDisplay(string gemType, Image image)
        {
            if (playerInfoDisplay == null) 
            {
                return;
            }
            
            // 根据gem类型分配对应的Image引用
            switch (gemType)
            {
                case "black":
                    playerInfoDisplay.blackGemImage = image;
                    break;
                case "red":
                    playerInfoDisplay.redGemImage = image;
                    break;
                case "yellow":
                    playerInfoDisplay.yellowGemImage = image;
                    break;
                case "green":
                    playerInfoDisplay.greenGemImage = image;
                    break;
                case "blue":
                    playerInfoDisplay.blueGemImage = image;
                    break;
                case "white":
                    playerInfoDisplay.whiteGemImage = image;
                    break;
                default:

                    break;
            }

        }
        
        /// <summary>
        /// 将文本组件分配给PlayerInfoDisplay
        /// </summary>
        private void AssignTextComponentToPlayerInfoDisplay(string componentName, TextMeshProUGUI text)
        {
            if (playerInfoDisplay == null) return;
            
            
            switch (componentName)
            {
                case "PlayerNameText":
                    playerInfoDisplay.playerNameText = text;
                    break;
                case "PlayerScoreText":
                    playerInfoDisplay.playerScoreText = text;
                    break;
                // 不再分配gem count text组件
                default:
                    break;
            }
        }
        
        /// <summary>
        /// 加载宝石sprite
        /// </summary>
        private Sprite LoadGemSprite(string gemType, int count)
        {
            try
            {
                Sprite sprite = null;
                
                if (count == 0)
                {
                    // 如果数量为0，直接加载none sprite
                    string spritePath = $"gems_large/{gemType}_none";
                    sprite = Resources.Load<Sprite>(spritePath);
                }
                else
                {
                    // 根据gem类型和数量加载对应的sprite
                    string spritePath = $"gems_large/{gemType}_{count}";
                    sprite = Resources.Load<Sprite>(spritePath);
                    
                    if (sprite == null)
                    {
                        // 如果找不到对应数量的sprite，尝试加载none sprite
                        spritePath = $"gems_large/{gemType}_none";
                        sprite = Resources.Load<Sprite>(spritePath);
                    }
                }
                
                
                return sprite;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
        
        /// <summary>
        /// 获取宝石颜色
        /// </summary>
        private Color GetGemColor(string gemType)
        {
            switch (gemType)
            {
                case "black":
                    return new Color(0.1f, 0.1f, 0.1f);
                case "red":
                    return new Color(0.9f, 0.1f, 0.1f);
                case "yellow":
                    return new Color(1f, 0.9f, 0.1f);
                case "green":
                    return new Color(0.1f, 0.8f, 0.1f);
                case "blue":
                    return new Color(0.1f, 0.1f, 0.9f);
                case "white":
                    return new Color(0.95f, 0.95f, 0.95f);
                default:
                    return Color.gray;
            }
        }
        
        /// <summary>
        /// Destroy PlayerInfoDisplay UI
        /// </summary>
        [ContextMenu("Destroy PlayerInfoDisplay UI")]
        public void DestroyPlayerInfoDisplayUI()
        {
            if (playerInfoDisplay != null)
            {
                DestroyImmediate(playerInfoDisplay.gameObject);
                playerInfoDisplay = null;
            }
        }
        
        /// <summary>
        /// Recreate UI
        /// </summary>
        [ContextMenu("Recreate UI")]
        public void RecreateUI()
        {
            DestroyPlayerInfoDisplayUI();
            CreatePlayerInfoDisplayUI();
        }
        
        /// <summary>
        /// Get setup status
        /// </summary>
        [ContextMenu("Get Setup Status")]
        public string GetSetupStatus()
        {
            string status = $"=== PlayerInfoDisplaySetup Status ===\n";
            status += $"Canvas: {(canvas != null ? "Found" : "Not Found")}\n";
            status += $"PlayerInfoDisplay: {(playerInfoDisplay != null ? "Created" : "Not Created")}\n";
            status += $"Player Name: {playerName}\n";
            status += $"Player ID: {playerId}\n";
            status += $"Display Size: {displaySize}\n";
            status += $"Display Position: {displayPosition}\n";
            status += $"Gem Size: {gemSize}\n";
            status += $"Gem Spacing: {gemSpacing}";
            
            return status;
        }
    }
}
