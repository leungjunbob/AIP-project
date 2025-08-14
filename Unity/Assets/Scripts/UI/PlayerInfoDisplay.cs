using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplendorUnity.Core;
using SplendorUnity.Models;
using SplendorUnity.Utils;
using SplendorUnity.Display;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 玩家信息显示区域 - 显示玩家名字、分数和宝石
    /// </summary>
    public class PlayerInfoDisplay : MonoBehaviour
    {
        [Header("Player Info")]
        public string playerName = "Player";
        public int playerId = 0;
        public bool enableDebugLog = true;
        
        [Header("UI Components")]
        public TextMeshProUGUI playerNameText;      // 玩家名字文本
        public TextMeshProUGUI playerScoreText;     // 玩家分数文本
        
        [Header("Gem Display")]
        public Image blackGemImage;                 // 黑色宝石图片
        public Image redGemImage;                   // 红色宝石图片
        public Image yellowGemImage;                // 黄色宝石图片
        public Image greenGemImage;                 // 绿色宝石图片
        public Image blueGemImage;                  // 蓝色宝石图片
        public Image whiteGemImage;                 // 白色宝石图片
        
        [Header("Card Display")]
        public Transform purchasedCardsContainer;   // 已购买卡牌容器
        public Transform reservedCardsContainer;    // 保留卡牌容器
        public TextMeshProUGUI reservedCardsLabel; // 保留卡牌标签
        
        [Header("Noble Display")]
        public Transform noblesContainer;           // 贵族容器
        [System.Obsolete("贵族标签已移除，不再显示文字")]
        public TextMeshProUGUI noblesLabel;        // 贵族标签（已废弃）
        
        [Header("Settings")]
        public string gemSpriteSize = "large";      // 宝石图片尺寸
        
        // 私有字段
        private Dictionary<string, Image> gemImages;
        private Dictionary<string, Sprite[]> gemSprites;
        private GameStateListener gameStateListener;
        private SplendorGameState.AgentState currentAgentState;
        
        private void Awake()
        {
            // 延迟初始化，确保PlayerInfoDisplaySetup先完成组件分配
            StartCoroutine(InitializeAfterFrame());
        }
        
        private IEnumerator InitializeAfterFrame()
        {
            yield return null; // 等待一帧，确保PlayerInfoDisplaySetup完成
            InitializeGemReferences();
            LoadGemSprites();
        }
        
        private void Start()
        {
            // 自动查找GameStateListener
            gameStateListener = FindObjectOfType<GameStateListener>();
            if (gameStateListener != null)
            {
                gameStateListener.AddPlayerInfoDisplay(this);
            }
            
            // 初始化显示
            UpdatePlayerInfo();
        }
        
        /// <summary>
        /// 初始化宝石引用
        /// </summary>
        private void InitializeGemReferences()
        {
            if (enableDebugLog)
            {
                Debug.Log($"PlayerInfoDisplay {playerId}: 初始化宝石引用...");
                Debug.Log($"PlayerInfoDisplay {playerId}: blackGemImage: {blackGemImage != null}");
                Debug.Log($"PlayerInfoDisplay {playerId}: redGemImage: {redGemImage != null}");
                Debug.Log($"PlayerInfoDisplay {playerId}: yellowGemImage: {yellowGemImage != null}");
                Debug.Log($"PlayerInfoDisplay {playerId}: greenGemImage: {greenGemImage != null}");
                Debug.Log($"PlayerInfoDisplay {playerId}: blueGemImage: {blueGemImage != null}");
                Debug.Log($"PlayerInfoDisplay {playerId}: whiteGemImage: {whiteGemImage != null}");
            }
            
            gemImages = new Dictionary<string, Image>
            {
                {"black", blackGemImage},
                {"red", redGemImage},
                {"yellow", yellowGemImage},
                {"green", greenGemImage},
                {"blue", blueGemImage},
                {"white", whiteGemImage}
            };
            
            if (enableDebugLog)
            {
                Debug.Log($"PlayerInfoDisplay {playerId}: gemImages字典创建完成，包含 {gemImages.Count} 个条目");
                foreach (var kvp in gemImages)
                {
                    Debug.Log($"PlayerInfoDisplay {playerId}: {kvp.Key}: {kvp.Value != null}");
                }
            }
        }
        
        /// <summary>
        /// 加载宝石图片
        /// </summary>
        private void LoadGemSprites()
        {
            // 强制使用large尺寸的gem素材
            gemSprites = GemSpriteLoader.LoadAllGemSprites("large");
            
        }
        
        /// <summary>
        /// 更新玩家信息
        /// </summary>
        public void UpdatePlayerInfo()
        {
            // 更新玩家名字和分数
            if (playerNameText != null)
            {
                playerNameText.text = playerName;
            }
            
            if (playerScoreText != null)
            {
                playerScoreText.text = $"Score: {GetCurrentScore()}";
            }
            
            // 更新宝石显示
            UpdateGemDisplay();
            
            // 更新卡牌显示
            UpdateCardDisplay();
            
            // 更新贵族显示
            UpdateNobleDisplay();
        }
        
        /// <summary>
        /// 更新宝石显示
        /// </summary>
        private void UpdateGemDisplay()
        {
            if (currentAgentState == null)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"PlayerInfoDisplay {playerId}: currentAgentState为null，无法更新宝石显示");
                return;
            }
            
            if (enableDebugLog)
                Debug.Log($"PlayerInfoDisplay {playerId}: 开始更新宝石显示，gems: {GetGemCountString(currentAgentState)}");
            
            // 检查gemImages是否已初始化
            if (gemImages == null || gemImages.Count == 0)
            {
                if (enableDebugLog)
                    Debug.Log($"PlayerInfoDisplay {playerId}: gemImages未初始化，调用InitializeGemReferences");
                InitializeGemReferences();
            }
            
            foreach (var kvp in gemImages)
            {
                string gemType = kvp.Key;
                Image gemImage = kvp.Value;
                
                if (gemImage != null)
                {
                    int gemCount = GetGemCount(gemType);
                    
                    if (enableDebugLog)
                        Debug.Log($"PlayerInfoDisplay {playerId}: 更新宝石 {gemType}，数量: {gemCount}");
                    
                    // 更新宝石图片
                    UpdateGemImage(gemType, gemCount);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning($"PlayerInfoDisplay {playerId}: 宝石图片 {gemType} 为null");
                }
            }
        }
        
        /// <summary>
        /// 更新卡牌显示
        /// </summary>
        private void UpdateCardDisplay()
        {
            if (currentAgentState == null) return;
            
            // 更新已购买卡牌显示
            UpdatePurchasedCardsDisplay();
            
            // 更新保留卡牌显示
            UpdateReservedCardsDisplay();
        }
        
        /// <summary>
        /// 更新已购买卡牌显示
        /// </summary>
        private void UpdatePurchasedCardsDisplay()
        {
            if (purchasedCardsContainer == null) return;
            
            // 清除现有卡牌显示
            foreach (Transform child in purchasedCardsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 显示已购买的卡牌（按颜色分组）
            var colorOrder = new[] { "green", "red", "white", "black", "blue" };
            int cardIndex = 0;
            int cardsPerRow = 10; // 每行10张卡牌，减少间距
            
            foreach (string color in colorOrder)
            {
                if (currentAgentState.cards.ContainsKey(color) && currentAgentState.cards[color].Count > 0)
                {
                    foreach (var card in currentAgentState.cards[color])
                    {
                        if (cardIndex >= 20) break; // 最多显示20张卡牌（2行）
                        
                        // 计算行和列位置
                        int row = cardIndex / cardsPerRow;
                        int col = cardIndex % cardsPerRow;
                        
                        CreateCardImage(card, purchasedCardsContainer, cardIndex, row, col);
                        cardIndex++;
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新保留卡牌显示
        /// </summary>
        private void UpdateReservedCardsDisplay()
        {
            if (reservedCardsContainer == null) return;
            
            // 清除现有卡牌显示，但保留标签
            foreach (Transform child in reservedCardsContainer)
            {
                // 跳过ReservedCardsLabel，只清除卡牌
                if (child.name != "ReservedCardsLabel")
                {
                    Destroy(child.gameObject);
                }
            }
            
            // 显示保留的卡牌
            if (currentAgentState.cards.ContainsKey("yellow") && currentAgentState.cards["yellow"].Count > 0)
            {
                int cardIndex = 0;
                foreach (var card in currentAgentState.cards["yellow"])
                {
                    if (cardIndex >= 3) break; // 最多保留3张卡牌
                    
                    CreateCardImage(card, reservedCardsContainer, cardIndex, 0, cardIndex);
                    cardIndex++;
                }
            }
        }
        
        /// <summary>
        /// 更新贵族显示
        /// </summary>
        private void UpdateNobleDisplay()
        {
            if (noblesContainer == null) return;
            
            // 清除现有贵族显示
            foreach (Transform child in noblesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 显示拥有的贵族
            if (currentAgentState != null && currentAgentState.nobles != null && currentAgentState.nobles.Count > 0)
            {
                int nobleIndex = 0;
                foreach (var noble in currentAgentState.nobles)
                {
                    if (nobleIndex >= 5) break; // 最多显示5个贵族
                    
                    CreateNobleImage(noble, noblesContainer, nobleIndex);
                    nobleIndex++;
                }
                
            }
        }
        
        /// <summary>
        /// 创建贵族图片
        /// </summary>
        private void CreateNobleImage(Noble noble, Transform container, int index)
        {
            GameObject nobleGO = new GameObject($"Noble_{noble.Code}");
            nobleGO.transform.SetParent(container, false);
            
            // 添加Image组件
            Image nobleImage = nobleGO.AddComponent<Image>();
            
            // 使用新的sprite加载方法
            Sprite nobleSprite = LoadNobleSprite(noble.Code);
            
            if (nobleSprite != null)
            {
                nobleImage.sprite = nobleSprite;
                nobleImage.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // 如果找不到sprite，使用默认颜色作为fallback
                nobleImage.color = new Color(0.8f, 0.6f, 0.2f, 1f); // 金色
            }
            
            // 设置RectTransform
            RectTransform rectTransform = nobleGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(30, 30); // 贵族图片尺寸
            rectTransform.anchoredPosition = new Vector2(index * 32, 0); // 水平排列，间距32
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            
            // 添加贵族信息提示
            AddNobleTooltip(nobleGO, noble);
        }
        
        /// <summary>
        /// 加载贵族sprite
        /// </summary>
        private Sprite LoadNobleSprite(string nobleCode)
        {
            // 尝试从多个路径加载贵族图片
            string[] possiblePaths = { "nobles_large/", "nobles_small/", "nobles/" };
            
            foreach (string path in possiblePaths)
            {
                // 尝试直接加载
                string directPath = $"{path}{nobleCode}";
                Sprite sprite = Resources.Load<Sprite>(directPath);
                if (sprite != null)
                {
                    return sprite;
                }
                
                // 尝试使用文件名映射
                string mappedFileName = GetMappedFileName(nobleCode);
                if (!string.IsNullOrEmpty(mappedFileName))
                {
                    string mappedPath = $"{path}{mappedFileName}";
                    sprite = Resources.Load<Sprite>(mappedPath);
                    if (sprite != null)
                    {
                        return sprite;
                    }
                }
            }
            
            
            return null;
        }
        
        /// <summary>
        /// 获取映射的文件名
        /// </summary>
        private string GetMappedFileName(string nobleCode)
        {
            // 使用与NobleManager相同的映射逻辑
            var fileNameToCodeMap = new Dictionary<string, string>
            {
                {"4g4r", "4g4r"},
                {"3w3r3bla", "3w3r3B"},
                {"3blu3g3r", "3b3g3r"},
                {"3w3blu3g", "3w3b3g"},
                {"4w4blu", "4w4b"},
                {"4w4bla", "4w4B"},
                {"3w3blu3bla", "3w3b3B"},
                {"4r4bla", "4r4B"},
                {"4blu4g", "4b4g"},
                {"3g3r3bla", "3g3r3B"}
            };
            
            // 反向查找
            foreach (var kvp in fileNameToCodeMap)
            {
                if (kvp.Value == nobleCode)
                {
                    return kvp.Key;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 为贵族添加信息提示（已移除文字显示）
        /// </summary>
        private void AddNobleTooltip(GameObject nobleGO, Noble noble)
        {
            // 不再创建提示文字，只保留方法以维持接口兼容性
        }
        
        /// <summary>
        /// 创建卡牌图片
        /// </summary>
        private void CreateCardImage(Card card, Transform container, int index, int row = 0, int col = 0)
        {
            GameObject cardGO = new GameObject($"Card_{card.Code}");
            cardGO.transform.SetParent(container, false);
            
            // 添加Image组件
            Image cardImage = cardGO.AddComponent<Image>();
            
            // 加载卡牌sprite - 使用Code映射到正确的文件名
            string spritePath = GetCardSpritePath(card);
            Sprite cardSprite = Resources.Load<Sprite>(spritePath);
            
            if (cardSprite != null)
            {
                cardImage.sprite = cardSprite;
                cardImage.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // 如果找不到sprite，使用颜色作为fallback
                cardImage.color = GetCardColor(card.Colour);
            }
            
            // 设置RectTransform
            RectTransform rectTransform = cardGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(25, 35); // 调整卡牌尺寸，使其更小
            rectTransform.anchoredPosition = new Vector2(col * 26, -row * 36); // 减少间距，让卡牌紧密排列
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            
            // 检查是否为reserved card，如果是则添加点击事件和buy button
            if (container == reservedCardsContainer)
            {
                AddReservedCardInteraction(cardGO, card);
            }
        }
        
        /// <summary>
        /// 获取卡牌sprite路径 - 建立Code到文件名的映射
        /// </summary>
        private string GetCardSpritePath(Card card)
        {
            // 根据GameData的命名规则，图片文件名格式是 {colour}_{code}
            // 其中code中的颜色缩写需要映射：
            // b -> blu (blue), B -> bla (black)
            string mappedCode = MapCodeToImageNaming(card.Code);
            string spritePath = $"cards_large/{card.Colour}_{mappedCode}";
            
            if (Resources.Load<Sprite>(spritePath) != null)
            {
                return spritePath;
            }
            
            // 如果映射路径失败，尝试直接使用Code
            string directPath = $"cards_large/{card.Code}";
            if (Resources.Load<Sprite>(directPath) != null)
            {
                return directPath;
            }
            
            // 如果还是失败，尝试使用颜色+成本的方式
            string costPath = $"cards_large/{card.Colour}_{GetCardCostString(card.Cost)}";
            if (Resources.Load<Sprite>(costPath) != null)
            {
                return costPath;
            }
            
            return spritePath;
        }
        
        /// <summary>
        /// 将Code映射到图片命名规则
        /// </summary>
        private string MapCodeToImageNaming(string code)
        {
            // 根据GameData的ParseResourceFilename方法，建立映射关系
            // 图片文件名中的颜色缩写：
            // b -> blu (blue)
            // B -> bla (black)
            // 其他颜色保持原样
            string mappedCode = code;
            
            // 替换颜色缩写
            mappedCode = mappedCode.Replace("b", "blu");  // 小写b -> blu
            mappedCode = mappedCode.Replace("B", "bla");  // 大写B -> bla
            
            
            return mappedCode;
        }
        
        /// <summary>
        /// 获取卡牌成本字符串
        /// </summary>
        private string GetCardCostString(Dictionary<string, int> cost)
        {
            var costStrings = new List<string>();
            foreach (var kvp in cost.OrderBy(x => x.Key))
            {
                if (kvp.Value > 0)
                {
                    costStrings.Add($"{kvp.Value}{GetColorAbbreviation(kvp.Key)}");
                }
            }
            return string.Join("", costStrings);
        }
        
        /// <summary>
        /// 获取颜色缩写
        /// </summary>
        private string GetColorAbbreviation(string color)
        {
            switch (color)
            {
                case "green": return "g";
                case "red": return "r";
                case "white": return "w";
                case "black": return "bla";
                case "blue": return "blu";
                default: return color;
            }
        }
        
        /// <summary>
        /// 为reserved card添加交互功能
        /// </summary>
        private void AddReservedCardInteraction(GameObject cardGO, Card card)
        {
            // 添加Button组件
            Button cardButton = cardGO.AddComponent<Button>();
            
            // 设置按钮点击事件
            cardButton.onClick.AddListener(() => OnReservedCardClicked(card));
            
        }
        

        
        /// <summary>
        /// Reserved card点击事件处理
        /// </summary>
        private void OnReservedCardClicked(Card card)
        {
            
            // 检查是否为当前玩家的reserved card
            bool isCurrentPlayerCard = IsCurrentPlayerReservedCard(card);
            
            if (!isCurrentPlayerCard)
            {
                return;
            }
            
            // 检查是否为当前回合的玩家
            bool isCurrentTurn = IsCurrentTurnPlayer();
            
            if (!isCurrentTurn)
            {
                return;
            }
            
            // 检查是否有有效的buy_reserved action
            bool hasValidAction = HasValidBuyReservedAction(card);
            
            if (hasValidAction)
            {
                // 打开BuyUI
                OpenBuyUI(card);
            }
            else
            {
                // 显示invalid提示 - 使用与board card buy button一致的提示词
                ShowInvalidActionMessage("Cannot buy this card - insufficient resources");
            }
        }
        
        /// <summary>
        /// 检查是否为当前玩家的reserved card
        /// </summary>
        private bool IsCurrentPlayerReservedCard(Card card)
        {
            // 检查当前AgentState是否包含这张reserved card
            if (currentAgentState?.cards == null || !currentAgentState.cards.ContainsKey("yellow"))
                return false;
            
            // 检查这张card是否在当前玩家的reservedCards中（存储在cards["yellow"]）
            return currentAgentState.cards["yellow"].Any(reservedCard => reservedCard.Code == card.Code);
        }
        
        /// <summary>
        /// 检查是否为当前回合的玩家
        /// </summary>
        private bool IsCurrentTurnPlayer()
        {
            // 获取当前游戏状态
            var gameManager = FindObjectOfType<Core.GameManager>();
            if (gameManager?.gameRule?.CurrentGameState == null)
                return false;
            
            var gameState = gameManager.gameRule.CurrentGameState as SplendorUnity.Core.SplendorGameState;
            if (gameState == null)
                return false;
            
            // 检查当前玩家ID是否匹配当前回合
            return gameState.agentToMove == playerId;
        }
        
        /// <summary>
        /// 检查是否有有效的buy_reserved action
        /// </summary>
        private bool HasValidBuyReservedAction(Card card)
        {
            var gameManager = FindObjectOfType<Core.GameManager>();
            if (gameManager?.gameRule?.CurrentGameState == null)
                return false;
            
            var gameState = gameManager.gameRule.CurrentGameState as SplendorUnity.Core.SplendorGameState;
            if (gameState == null)
                return false;
            
            var validActions = gameManager.gameRule?.GetLegalActions((object)gameState, gameState.agentToMove);
            if (validActions == null || validActions.Count == 0)
                return false;
            
            // 查找匹配的buy_reserved action
            foreach (var action in validActions)
            {
                if (action is Core.Action actionObj && actionObj.Type == "buy_reserved")
                {
                    if (actionObj.Card != null && actionObj.Card.Code == card.Code)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 打开BuyUI
        /// </summary>
        private void OpenBuyUI(Card card)
        {
            var gameManager = FindObjectOfType<Core.GameManager>();
            if (gameManager?.gameRule?.CurrentGameState == null)
            {
                return;
            }
            
            var gameState = gameManager.gameRule.CurrentGameState as SplendorUnity.Core.SplendorGameState;
            if (gameState == null)
            {
                return;
            }
            
            var currentPlayer = gameState.agents[gameState.agentToMove];
            
            // 查找BuyUI组件
            var buyUI = FindObjectOfType<UI.BuyUI>();
            if (buyUI != null)
            {
                buyUI.ShowPanel(gameState, currentPlayer, card);
            }
        }
        
        /// <summary>
        /// 显示无效操作提示消息
        /// </summary>
        private void ShowInvalidActionMessage(string message)
        {
            // 查找或创建InvalidActionUI
            var invalidActionUI = FindObjectOfType<CardManager.InvalidActionUI>();
            if (invalidActionUI == null)
            {
                // 如果没有找到，创建一个新的
                invalidActionUI = CreateInvalidActionUI();
            }
            
            if (invalidActionUI != null)
            {
                invalidActionUI.ShowMessage(message);
            }
        }
        
        /// <summary>
        /// 创建InvalidActionUI
        /// </summary>
        private CardManager.InvalidActionUI CreateInvalidActionUI()
        {
            // 创建Canvas
            var canvasGO = new GameObject("InvalidActionCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 确保在最上层
            
            // 添加CanvasScaler
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // 添加GraphicRaycaster
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // 创建InvalidActionUI组件
            var invalidActionUI = canvasGO.AddComponent<CardManager.InvalidActionUI>();
            
            return invalidActionUI;
        }
        

        
        /// <summary>
        /// 获取卡牌颜色
        /// </summary>
        private Color GetCardColor(string color)
        {
            switch (color)
            {
                case "green": return new Color(0.1f, 0.8f, 0.1f);
                case "red": return new Color(0.9f, 0.1f, 0.1f);
                case "white": return new Color(0.95f, 0.95f, 0.95f);
                case "black": return new Color(0.1f, 0.1f, 0.1f);
                case "blue": return new Color(0.1f, 0.1f, 0.9f);
                default: return Color.gray;
            }
        }
        
        /// <summary>
        /// 更新宝石图片
        /// </summary>
        private void UpdateGemImage(string gemType, int count)
        {
            if (!gemImages.ContainsKey(gemType))
            {
                return;
            }
            
            var image = gemImages[gemType];
            if (image == null)
            {
                return;
            }
            
            
            // 如果数量为0，使用none sprite
            if (count == 0)
            {
                // 尝试加载none sprite
                string nonePath = $"gems_large/{gemType}_none";
                Sprite noneSprite = Resources.Load<Sprite>(nonePath);
                if (noneSprite != null)
                {
                    image.sprite = noneSprite;
                    image.enabled = true;
                    image.color = new Color(1f, 1f, 1f, 1f); // 完全不透明
                }
                else
                {
                    // 如果找不到none sprite，隐藏图片
                    image.enabled = false;
                }
            }
            else
            {
                // 尝试加载对应数量的sprite
                string spritePath = $"gems_large/{gemType}_{count}";
                Sprite gemSprite = Resources.Load<Sprite>(spritePath);
                
                if (gemSprite != null)
                {
                    image.sprite = gemSprite;
                    image.enabled = true;
                    image.color = new Color(1f, 1f, 1f, 1f); // 完全不透明
                }
                else
                {
                    // 如果找不到对应数量的sprite，尝试加载none sprite作为fallback
                    string nonePath = $"gems_large/{gemType}_none";
                    Sprite noneSprite = Resources.Load<Sprite>(nonePath);
                    if (noneSprite != null)
                    {
                        image.sprite = noneSprite;
                        image.enabled = true;
                        image.color = new Color(1f, 1f, 1f, 1f); // 完全不透明
                    }
                    else
                    {
                        // 如果还是找不到，隐藏图片
                        image.enabled = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取当前分数
        /// </summary>
        private int GetCurrentScore()
        {
            if (currentAgentState != null)
            {
                return currentAgentState.score;
            }
            return 0;
        }
        
        /// <summary>
        /// 获取指定类型的宝石数量
        /// </summary>
        private int GetGemCount(string gemType)
        {
            if (currentAgentState != null && currentAgentState.gems.ContainsKey(gemType))
            {
                return currentAgentState.gems[gemType];
            }
            return 0;
        }
        
        /// <summary>
        /// 设置玩家名称
        /// </summary>
        public void SetPlayerName(string name)
        {
            playerName = name;
            UpdatePlayerInfo();
        }
        
        /// <summary>
        /// 设置玩家ID
        /// </summary>
        public void SetPlayerId(int id)
        {
            playerId = id;
            UpdatePlayerInfo();
        }
        
        /// <summary>
        /// 更新代理状态
        /// </summary>
        public void UpdateAgentState(SplendorGameState.AgentState agentState)
        {
            if (enableDebugLog)
                Debug.Log($"PlayerInfoDisplay {playerId}: UpdateAgentState被调用，gems: {GetGemCountString(agentState)}");
            
            currentAgentState = agentState;
            UpdatePlayerInfo();
        }
        
        /// <summary>
        /// 获取宝石数量字符串（用于调试）
        /// </summary>
        private string GetGemCountString(SplendorGameState.AgentState agentState)
        {
            if (agentState == null || agentState.gems == null) return "null";
            
            var gemStrings = new List<string>();
            foreach (var kvp in agentState.gems)
            {
                gemStrings.Add($"{kvp.Key}:{kvp.Value}");
            }
            return string.Join(", ", gemStrings);
        }
        
        
        
    }
    

}
