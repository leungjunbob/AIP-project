using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplendorUnity.Core;
using SplendorUnity.Models;
using SplendorUnity.Events;
using SplendorUnity.UI;
using TMPro;

namespace SplendorUnity.Display
{
    /// <summary>
    /// 卡片管理器 - 自动管理Level 1、Level 2和Level 3卡片显示
    /// 根据GameState自动更新卡片显示，自动导入图片资源
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("卡片显示设置")]
        public bool updateOnGameEvents = true;
        
        [Header("卡片UI组件")]
        public Transform level1CardContainer; // Level 1卡片容器
        public Transform level2CardContainer; // Level 2卡片容器
        public Transform level3CardContainer; // Level 3卡片容器
        public GameObject cardPrefab; // 卡片预制体（可选）
        
        [Header("各等级卡片槽位设置")]
        public int level1CardSlots = 4; // Level 1卡片槽位数（通常显示4张卡片）
        public int level2CardSlots = 4; // Level 2卡片槽位数（通常显示4张卡片）
        public int level3CardSlots = 4; // Level 3卡片槽位数（通常显示4张卡片）
        
        [Header("图片资源")]
        public Sprite noneSprite; // 无卡片时的默认图片
        public string cardImagePath = "cards_large/"; // 卡片图片路径
        
        [Header("组件引用")]
        public SplendorGameState gameState;
        public GameManager gameManager;
        
        [Header("调试设置")]
        public bool enableDebugLog = true;
        
        [Header("自动更新设置")]
        public bool enableAutoUpdate = true; // 启用自动更新
        public float updateInterval = 0.5f; // 更新间隔（秒）
        public bool updateOnGameStateChange = true; // 检测游戏状态变化时更新
        
        // 私有字段
        private List<CardDisplay> level1CardDisplays = new List<CardDisplay>();
        private List<CardDisplay> level2CardDisplays = new List<CardDisplay>();
        private List<CardDisplay> level3CardDisplays = new List<CardDisplay>();
        private List<Card> lastKnownLevel1Cards = new List<Card>();
        private List<Card> lastKnownLevel2Cards = new List<Card>();
        private List<Card> lastKnownLevel3Cards = new List<Card>();
        private Dictionary<string, Sprite> cardSprites = new Dictionary<string, Sprite>();
        private Coroutine autoUpdateCoroutine;
        
        private void Awake()
        {
            // 确保在Awake中初始化lastKnownCards
            lastKnownLevel1Cards = new List<Card>();
            lastKnownLevel2Cards = new List<Card>();
            lastKnownLevel3Cards = new List<Card>();
            InitializeCardDisplays();
            LoadCardSprites();
        }
        
        private void Start()
        {
            // 尝试找到必要的组件
            if (gameState == null)
            {
                gameState = FindObjectOfType<SplendorGameState>();
            }
            
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
            
            // 订阅游戏事件
            if (updateOnGameEvents)
            {
                SubscribeToGameEvents();
            }
            
            // 启动自动更新协程
            if (enableAutoUpdate)
            {
                StartAutoUpdate();
            }
        }
        
        /// <summary>
        /// 延迟一帧后进行初始更新
        /// </summary>
        private IEnumerator InitialUpdateAfterFrame()
        {
            yield return null;
                
            // 尝试从当前游戏状态更新卡片显示
            UpdateCardsFromGameState();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
            
            // 停止自动更新
            if (autoUpdateCoroutine != null)
            {
                StopCoroutine(autoUpdateCoroutine);
            }
        }
        
        /// <summary>
        /// 启动自动更新
        /// </summary>
        private void StartAutoUpdate()
        {
            if (autoUpdateCoroutine != null)
            {
                StopCoroutine(autoUpdateCoroutine);
            }
            
            autoUpdateCoroutine = StartCoroutine(AutoUpdateCoroutine());
        }
        
        /// <summary>
        /// 停止自动更新
        /// </summary>
        public void StopAutoUpdate()
        {
            if (autoUpdateCoroutine != null)
            {
                StopCoroutine(autoUpdateCoroutine);
                autoUpdateCoroutine = null;
            }
        }
        
        /// <summary>
        /// 自动更新协程
        /// </summary>
        private IEnumerator AutoUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);
                
                if (updateOnGameStateChange)
                {
                    CheckAndUpdateIfChanged();
                }
            }
        }
        
        /// <summary>
        /// 检查游戏状态是否变化，如果变化则更新
        /// </summary>
        private void CheckAndUpdateIfChanged()
        {
            if (gameState?.board?.dealt == null) return;
            
            bool hasChanged = false;
            
            // 检查Level 1卡牌是否有变化
            var currentLevel1Cards = GetLevel1CardsFromGameState();
            if (HasCardListChanged(currentLevel1Cards, lastKnownLevel1Cards))
            {
                hasChanged = true;
            }
            
            // 检查Level 2卡牌是否有变化
            var currentLevel2Cards = GetLevel2CardsFromGameState();
            if (HasCardListChanged(currentLevel2Cards, lastKnownLevel2Cards))
            {
                hasChanged = true;
            }
            
            // 检查Level 3卡牌是否有变化
            var currentLevel3Cards = GetLevel3CardsFromGameState();
            if (HasCardListChanged(currentLevel3Cards, lastKnownLevel3Cards))
            {
                hasChanged = true;
            }
            
            if (hasChanged)
            {
                UpdateCardsFromGameState();
            }
        }
        
        /// <summary>
        /// 检查卡牌列表是否有变化
        /// </summary>
        private bool HasCardListChanged(List<Card> currentCards, List<Card> lastKnownCards)
        {
            if (currentCards.Count != lastKnownCards.Count) return true;
            
            for (int i = 0; i < currentCards.Count; i++)
            {
                if (i >= lastKnownCards.Count) return true;
                
                var currentCard = currentCards[i];
                var lastCard = lastKnownCards[i];
                
                if (currentCard == null && lastCard == null) continue;
                if (currentCard == null || lastCard == null) return true;
                if (currentCard.Code != lastCard.Code) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 初始化卡片显示槽位
        /// </summary>
        private void InitializeCardDisplays()
        {
            // 检查并设置默认容器
            if (level1CardContainer == null)
            {
                level1CardContainer = transform;
            }
            if (level2CardContainer == null)
            {
                level2CardContainer = transform;
            }
            if (level3CardContainer == null)
            {
                level3CardContainer = transform;
            }
            
            // 清除现有的显示
            foreach (var display in level1CardDisplays)
            {
                if (display != null && display.gameObject != null)
                    DestroyImmediate(display.gameObject);
            }
            level1CardDisplays.Clear();
            foreach (var display in level2CardDisplays)
            {
                if (display != null && display.gameObject != null)
                    DestroyImmediate(display.gameObject);
            }
            level2CardDisplays.Clear();
            foreach (var display in level3CardDisplays)
            {
                if (display != null && display.gameObject != null)
                    DestroyImmediate(display.gameObject);
            }
            level3CardDisplays.Clear();
            
            // 创建Level 1卡片显示槽位
            for (int i = 0; i < level1CardSlots; i++)
            {
                GameObject cardSlot;
                if (cardPrefab != null)
                {
                    cardSlot = Instantiate(cardPrefab, level1CardContainer);
                    cardSlot.name = $"Level1CardSlot_{i}";
                }
                else
                {
                    // 创建默认的卡片槽位
                    cardSlot = CreateDefaultCardSlot(i, "Level1");
                }
                
                var cardDisplay = cardSlot.GetComponent<CardDisplay>();
                if (cardDisplay == null)
                {
                    cardDisplay = cardSlot.AddComponent<CardDisplay>();
                }
                
                level1CardDisplays.Add(cardDisplay);
                
                // 初始化显示
                cardDisplay.Initialize(i, noneSprite);
                
                // 延迟一帧后设置图片，确保组件初始化完成
                StartCoroutine(InitializeCardDisplayAfterFrame(cardDisplay, i, "Level1"));
            }
            
            // 创建Level 2卡片显示槽位
            for (int i = 0; i < level2CardSlots; i++)
            {
                GameObject cardSlot;
                if (cardPrefab != null)
                {
                    cardSlot = Instantiate(cardPrefab, level2CardContainer);
                    cardSlot.name = $"Level2CardSlot_{i}";
                }
                else
                {
                    // 创建默认的卡片槽位
                    cardSlot = CreateDefaultCardSlot(i, "Level2");
                }
                
                var cardDisplay = cardSlot.GetComponent<CardDisplay>();
                if (cardDisplay == null)
                {
                    cardDisplay = cardSlot.AddComponent<CardDisplay>();
                }
                
                level2CardDisplays.Add(cardDisplay);
                
                // 初始化显示
                cardDisplay.Initialize(i, noneSprite);
                
                // 延迟一帧后设置图片，确保组件初始化完成
                StartCoroutine(InitializeCardDisplayAfterFrame(cardDisplay, i, "Level2"));
            }
            
            // 创建Level 3卡片显示槽位
            for (int i = 0; i < level3CardSlots; i++)
            {
                GameObject cardSlot;
                if (cardPrefab != null)
                {
                    cardSlot = Instantiate(cardPrefab, level3CardContainer);
                    cardSlot.name = $"Level3CardSlot_{i}";
                }
                else
                {
                    // 创建默认的卡片槽位
                    cardSlot = CreateDefaultCardSlot(i, "Level3");
                }
                
                var cardDisplay = cardSlot.GetComponent<CardDisplay>();
                if (cardDisplay == null)
                {
                    cardDisplay = cardSlot.AddComponent<CardDisplay>();
                }
                
                level3CardDisplays.Add(cardDisplay);
                
                // 初始化显示
                cardDisplay.Initialize(i, noneSprite);
                
                // 延迟一帧后设置图片，确保组件初始化完成
                StartCoroutine(InitializeCardDisplayAfterFrame(cardDisplay, i, "Level3"));
            }
        }
        
        /// <summary>
        /// 创建默认的卡片槽位
        /// </summary>
        private GameObject CreateDefaultCardSlot(int index, string level)
        {
            var cardSlot = new GameObject($"{level}CardSlot_{index}");
            
            // 根据等级设置父容器
            Transform parentContainer = null;
            switch (level)
            {
                case "Level1":
                    parentContainer = level1CardContainer;
                    break;
                case "Level2":
                    parentContainer = level2CardContainer;
                    break;
                case "Level3":
                    parentContainer = level3CardContainer;
                    break;
                default:
                    parentContainer = transform; // 默认使用当前Transform
                    break;
            }
            
            cardSlot.transform.SetParent(parentContainer);
            
            // 添加Image组件
            var image = cardSlot.AddComponent<Image>();
            image.sprite = noneSprite;
            image.color = Color.white;
            
            // 设置RectTransform
            var rectTransform = cardSlot.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(200, 230); // 卡片尺寸
                
                // 根据等级设置不同的位置（相对于各自的容器）
                
                float xOffset = index * 220; // 水平排列，间距20
                float yOffset = 0;
                
                rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
                
                // 确保有正确的锚点设置，以便按钮能正确显示
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            
            return cardSlot;
        }
        
        /// <summary>
        /// 延迟初始化卡片显示
        /// </summary>
        private IEnumerator InitializeCardDisplayAfterFrame(CardDisplay cardDisplay, int index, string level)
        {
            yield return null; // 等待一帧
            
            if (cardDisplay != null)
            {
                // 设置默认图片
                if (noneSprite != null)
                {
                    cardDisplay.UpdateCard(null, noneSprite);
                }
                
            }
        }
        
        /// <summary>
        /// 加载卡片图片资源
        /// </summary>
        private void LoadCardSprites()
        {
            cardSprites.Clear();
            
            // 加载none图片
            if (noneSprite != null)
            {
                cardSprites["none"] = noneSprite;
            }
            
            // 尝试从多个路径加载卡片图片
            string[] possiblePaths = { "cards_large/", "cards_small/", "cards/" };
            
            foreach (string path in possiblePaths)
            {
                var cardResources = Resources.LoadAll<Sprite>(path);
                if (cardResources.Length > 0)
                {
                   
                    foreach (var sprite in cardResources)
                    {
                        // 提取卡片代码（去掉文件扩展名和路径）
                        string fileName = sprite.name;
                        string cardCode = ConvertFileNameToCardCode(fileName);
                        
                        if (!string.IsNullOrEmpty(cardCode) && !cardSprites.ContainsKey(cardCode))
                        {
                            cardSprites[cardCode] = sprite;
                        }
                    }
                    break; // 找到一个有效路径就停止
                }
            }
        }
        
        /// <summary>
        /// 将文件名转换为卡片代码
        /// </summary>
        private string ConvertFileNameToCardCode(string fileName)
        {
            // 移除文件扩展名
            string name = fileName;
            if (name.Contains("."))
            {
                name = name.Substring(0, name.LastIndexOf('.'));
            }
            
            // 文件名到卡片代码的映射（根据实际文件名）
            // 格式通常是：颜色_代码.png，如：black_1g1w1r1blu.png
            if (name.Contains("_"))
            {
                string[] parts = name.Split('_');
                if (parts.Length >= 2)
                {
                    string color = parts[0];
                    string code = parts[1];
                    
                    // 将代码转换为标准格式（将blu->b, bla->B等）
                    string standardCode = ConvertCodeToStandard(code);
                    
                    // 直接返回标准代码，因为GameData中的代码就是这种格式
                    return standardCode;
                }
            }
            
            // 如果无法解析，尝试直接匹配文件名（去掉颜色前缀）
            if (name.Length > 5 && (name.StartsWith("black") || name.StartsWith("blue") || name.StartsWith("green") || name.StartsWith("red") || name.StartsWith("white")))
            {
                string code = name.Substring(5); // 去掉颜色前缀
                return ConvertCodeToStandard(code);
            }
            
            return name; // 如果无法解析，直接返回文件名
        }
        
        /// <summary>
        /// 转换颜色到标准格式
        /// </summary>
        private string ConvertColorToStandard(string color)
        {
            var colorMap = new Dictionary<string, string>
            {
                {"black", "black"},
                {"blue", "blue"},
                {"green", "green"},
                {"red", "red"},
                {"white", "white"}
            };
            
            return colorMap.ContainsKey(color.ToLower()) ? color.ToLower() : color;
        }
        
        /// <summary>
        /// 转换代码到标准格式
        /// </summary>
        private string ConvertCodeToStandard(string code)
        {
            if (string.IsNullOrEmpty(code)) return code;
            
            // 将代码中的颜色缩写转换为标准格式
            // 根据实际文件名，需要将blu->b, bla->B等
            var codeMap = new Dictionary<string, string>
            {
                {"blu", "b"},
                {"bla", "B"},
                {"gre", "g"},
                {"whi", "w"},
                {"red", "r"}
            };
            
            string result = code;
            foreach (var kvp in codeMap)
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }
            
            return result;
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToGameEvents()
        {
            // 使用静态事件订阅
            GameManager.OnGameStarted += OnGameStarted;
            GameManager.OnActionExecuted += OnActionExecuted;
            GameManager.OnGameEnded += OnGameEnded;
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            // 使用静态事件取消订阅
            GameManager.OnGameStarted -= OnGameStarted;
            GameManager.OnActionExecuted -= OnActionExecuted;
            GameManager.OnGameEnded -= OnGameEnded;
        }
        
        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted(GameManager gameManager)
        {
            
            // 检查游戏状态是否可用
            if (gameState == null)
            {
                gameState = FindObjectOfType<SplendorGameState>();
            }
                    
            StartCoroutine(UpdateAfterFrame());
        }
        
        /// <summary>
        /// 游戏动作执行事件
        /// </summary>
        private void OnActionExecuted(int agentIndex, object action)
        {
            
            StartCoroutine(UpdateAfterFrame());
        }
        
        /// <summary>
        /// 游戏结束事件
        /// </summary>
        private void OnGameEnded(GameManager gameManager)
        {

        }
        
        /// <summary>
        /// 延迟一帧后更新
        /// </summary>
        private IEnumerator UpdateAfterFrame()
        {
            yield return null;
            
                
            UpdateCardsFromGameState();
        }
        

        
        /// <summary>
        /// 从游戏状态获取level1卡片
        /// </summary>
        private List<Card> GetLevel1CardsFromGameState()
        {
            var cards = new List<Card>();
            
            
            if (gameState != null && gameState.board != null)
            {
                
                // level1卡片存储在board.dealt[0]中（DeckId从1开始，转换为0开始）
                if (gameState.board.dealt != null && gameState.board.dealt.Length > 0)
                {
                    
                    // 安全地访问数组索引
                    int level1Index = 0;
                    if (level1Index < gameState.board.dealt.Length)
                    {
                        var level1Cards = gameState.board.dealt[level1Index];
                        
                        if (level1Cards != null)
                        {
                            
                            foreach (var card in level1Cards)
                            {
                                if (card != null)
                                {
                                    cards.Add(card);
                                }
                            }
                            
                        }
                    }
                }
            }
            
            return cards;
        }
        
        /// <summary>
        /// 从游戏状态获取level2卡片
        /// </summary>
        private List<Card> GetLevel2CardsFromGameState()
        {
            var cards = new List<Card>();
            
            
            if (gameState != null && gameState.board != null)
            {
                
                // level2卡片存储在board.dealt[1]中（DeckId从1开始，转换为0开始）
                if (gameState.board.dealt != null && gameState.board.dealt.Length > 1)
                {
                    
                    // 安全地访问数组索引
                    int level2Index = 1;
                    if (level2Index < gameState.board.dealt.Length)
                    {
                        var level2Cards = gameState.board.dealt[level2Index];
                        
                        if (level2Cards != null)
                        {
                            
                            foreach (var card in level2Cards)
                            {
                                if (card != null)
                                {
                                    cards.Add(card);
                                }
                            }
                        }
                    }

                }
            }            
            return cards;
        }
        
        /// <summary>
        /// 从游戏状态获取level3卡片
        /// </summary>
        private List<Card> GetLevel3CardsFromGameState()
        {
            var cards = new List<Card>();
                        
            if (gameState != null && gameState.board != null)
            {
                
                // level3卡片存储在board.dealt[2]中（DeckId从1开始，转换为0开始）
                if (gameState.board.dealt != null && gameState.board.dealt.Length > 2)
                {
                    
                    // 安全地访问数组索引
                    int level3Index = 2;
                    if (level3Index < gameState.board.dealt.Length)
                    {
                        var level3Cards = gameState.board.dealt[level3Index];
                        
                        if (level3Cards != null)
                        {
                            
                            foreach (var card in level3Cards)
                            {
                                if (card != null)
                                {
                                    cards.Add(card);
                                }
                            }
                        }
                    }
                }
            }            
            return cards;
        }
        
        /// <summary>
        /// 检查Level 1卡片是否有变化
        /// </summary>
        private bool HasLevel1CardsChanged(List<Card> currentCards)
        {
            if (currentCards == null)
            {
                return false;
            }
            
            if (lastKnownLevel1Cards == null)
            {
                lastKnownLevel1Cards = new List<Card>();
                return true; // 第一次比较，认为有变化
            }
            
            if (lastKnownLevel1Cards.Count != currentCards.Count)
                return true;
            
            for (int i = 0; i < currentCards.Count; i++)
            {
                if (i >= lastKnownLevel1Cards.Count || currentCards[i] == null || lastKnownLevel1Cards[i] == null || !currentCards[i].Equals(lastKnownLevel1Cards[i]))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查Level 2卡片是否有变化
        /// </summary>
        private bool HasLevel2CardsChanged(List<Card> currentCards)
        {
            if (currentCards == null)
            {
                return false;
            }
            
            if (lastKnownLevel2Cards == null)
            {
                lastKnownLevel2Cards = new List<Card>();
                return true; // 第一次比较，认为有变化
            }
            
            if (lastKnownLevel2Cards.Count != currentCards.Count)
                return true;
            
            for (int i = 0; i < currentCards.Count; i++)
            {
                if (i >= lastKnownLevel2Cards.Count || currentCards[i] == null || lastKnownLevel2Cards[i] == null || !currentCards[i].Equals(lastKnownLevel2Cards[i]))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查Level 3卡片是否有变化
        /// </summary>
        private bool HasLevel3CardsChanged(List<Card> currentCards)
        {
            if (currentCards == null)
            {
                return false;
            }
            
            if (lastKnownLevel3Cards == null)
            {
                lastKnownLevel3Cards = new List<Card>();
                return true; // 第一次比较，认为有变化
            }
            
            if (lastKnownLevel3Cards.Count != currentCards.Count)
                return true;
            
            for (int i = 0; i < currentCards.Count; i++)
            {
                if (i >= lastKnownLevel3Cards.Count || currentCards[i] == null || lastKnownLevel3Cards[i] == null || !currentCards[i].Equals(lastKnownLevel3Cards[i]))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取自动更新状态信息
        /// </summary>
        public string GetAutoUpdateStatus()
        {
            string status = $"自动更新: {(enableAutoUpdate ? "启用" : "禁用")}\n";
            status += $"更新间隔: {updateInterval}秒\n";
            status += $"事件响应: {(updateOnGameEvents ? "启用" : "禁用")}\n";
            status += $"状态检测: {(updateOnGameStateChange ? "启用" : "禁用")}\n";
            status += $"协程状态: {(autoUpdateCoroutine != null ? "运行中" : "未运行")}\n";
            status += $"GameManager: {(gameManager != null ? "已连接" : "未连接")}\n";
            status += $"GameState: {(gameState != null ? "已设置" : "未设置")}";
            
            return status;
        }
        
        /// <summary>
        /// 从游戏状态更新卡片显示
        /// </summary>
        public void UpdateCardsFromGameState()
        {
            // 优先从GameManager获取最新的游戏状态
            var currentGameState = gameManager?.gameRule?.CurrentGameState as SplendorUnity.Core.SplendorGameState;
            if (currentGameState == null)
            {
                // 如果GameManager不可用，尝试使用缓存的gameState
                if (gameState == null || gameState.board == null)
                {
                    return;
                }
                currentGameState = gameState;
            }
            
            // 更新缓存的gameState引用
            gameState = currentGameState;
            
            // 获取所有等级的卡片
            var currentLevel1Cards = GetLevel1CardsFromGameState();
            var currentLevel2Cards = GetLevel2CardsFromGameState();
            var currentLevel3Cards = GetLevel3CardsFromGameState();
                       
            // 更新Level 1卡片
            UpdateLevelCards(currentLevel1Cards, level1CardDisplays, level1CardSlots, "Level 1");
            
            // 更新Level 2卡片
            UpdateLevelCards(currentLevel2Cards, level2CardDisplays, level2CardSlots, "Level 2");
            
            // 更新Level 3卡片
            UpdateLevelCards(currentLevel3Cards, level3CardDisplays, level3CardSlots, "Level 3");
            
            // 更新已知卡片列表
            lastKnownLevel1Cards = new List<Card>(currentLevel1Cards);
            lastKnownLevel2Cards = new List<Card>(currentLevel2Cards);
            lastKnownLevel3Cards = new List<Card>(currentLevel3Cards);
        }
        
        /// <summary>
        /// 更新指定等级的卡片显示
        /// </summary>
        private void UpdateLevelCards(List<Card> currentCards, List<CardDisplay> cardDisplays, int maxSlots, string levelName)
        {            
            // 更新每个槽位
            for (int i = 0; i < maxSlots; i++)
            {
                if (i < cardDisplays.Count)
                {
                    var display = cardDisplays[i];
                    if (display != null)
                    {
                        if (i < currentCards.Count)
                        {
                            var card = currentCards[i];
                            if (card != null)
                            {
                                var sprite = GetCardSprite(card);
                                display.UpdateCard(card, sprite);
                                
                            }
                            else
                            {
                                // 卡片为空
                                display.UpdateCard(null, noneSprite);
                            }
                        }
                        else
                        {
                            // 空槽位
                            display.UpdateCard(null, noneSprite);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取卡片对应的图片
        /// </summary>
        private Sprite GetCardSprite(Card card)
        {
            if (card == null) return noneSprite;
            if (string.IsNullOrEmpty(card.Code)) return noneSprite;
            
            // 尝试直接匹配卡片代码
            if (cardSprites.ContainsKey(card.Code))
            {
                return cardSprites[card.Code];
            }
            
            // 尝试匹配去掉颜色前缀的代码
            if (card.Code.Length > 5 && (card.Code.StartsWith("black") || card.Code.StartsWith("blue") || card.Code.StartsWith("green") || card.Code.StartsWith("red") || card.Code.StartsWith("white")))
            {
                string codeWithoutColor = card.Code.Substring(5);
                if (cardSprites.ContainsKey(codeWithoutColor))
                {
                    return cardSprites[codeWithoutColor];
                }
            }
            

            return noneSprite;
        }
        
        
               
        /// <summary>
        /// 测试指定等级的卡片显示
        /// </summary>
        private void TestLevelCardDisplay(List<CardDisplay> cardDisplays, int maxSlots, string levelName)
        {            
            // 确保图片已加载
            LoadCardSprites();
            
            // 创建测试卡片数据
            var testCards = new List<Card>
            {
                Card.CreateCard("1g1w1r1b", "black", 0, 0, new Dictionary<string, int>{{"green", 1}, {"white", 1}, {"red", 1}, {"blue", 1}}),
                Card.CreateCard("2b1r2w", "black", 0, 0, new Dictionary<string, int>{{"blue", 2}, {"red", 1}, {"white", 2}}),
                Card.CreateCard("3g", "black", 0, 0, new Dictionary<string, int>{{"green", 3}}),
                Card.CreateCard("4b", "black", 0, 1, new Dictionary<string, int>{{"blue", 4}})
            };
            
            // 手动更新显示
            for (int i = 0; i < Math.Min(testCards.Count, cardDisplays.Count); i++)
            {
                var card = testCards[i];
                var display = cardDisplays[i];
                
                if (display != null)
                {
                    var sprite = GetCardSprite(card);
                    display.UpdateCard(card, sprite);
                }
            }
        }
        
        /// <summary>
        /// 卡片显示组件
        /// </summary>
        [System.Serializable]
        public class CardDisplay : MonoBehaviour
        {
            [Header("卡片信息")]
            public Card currentCard;
            
            [Header("UI组件")]
            public Image cardImage; // 卡片图片
            public Text cardText; // 可选：显示卡片信息
            
            [Header("交互按钮")]
            public Button reserveButton; // 预留按钮
            public Button buyButton; // 购买按钮
            public GameObject buttonContainer; // 按钮容器
            
            [Header("交互设置")]
            public bool enableClickInteraction = true; // 是否启用点击交互
            
            private void Awake()
            {
                // 获取或创建必要的UI组件
                if (cardImage == null)
                {
                    cardImage = GetComponent<Image>();
                }
                
                if (cardText == null)
                {
                    cardText = GetComponentInChildren<Text>();
                }
                
                // 创建按钮容器和按钮
                CreateInteractionButtons();
                
                // 添加点击交互
                if (enableClickInteraction)
                {
                    AddClickInteraction();
                }
            }
            
            /// <summary>
            /// 添加点击交互
            /// </summary>
            private void AddClickInteraction()
            {
                // 添加EventTrigger组件来处理点击事件
                var eventTrigger = gameObject.AddComponent<EventTrigger>();
                
                // 创建PointerClick事件
                var pointerClickEntry = new EventTrigger.Entry();
                pointerClickEntry.eventID = EventTriggerType.PointerClick;
                pointerClickEntry.callback.AddListener((data) => OnCardClicked());
                eventTrigger.triggers.Add(pointerClickEntry);
                
                // 确保有Image组件来接收点击事件
                if (cardImage == null)
                {
                    cardImage = GetComponent<Image>();
                }
                
                // 设置Image的Raycast Target为true，使其能接收点击事件
                if (cardImage != null)
                {
                    cardImage.raycastTarget = true;
                }
                
            }
            
            /// <summary>
            /// 卡片被点击时的处理
            /// </summary>
            private void OnCardClicked()
            {
                if (!enableClickInteraction) return;
                
                // 如果当前没有显示按钮，则显示按钮
                if (buttonContainer != null && !buttonContainer.activeSelf)
                {
                    SetButtonsVisible(true);
                    
                    // 3秒后自动隐藏按钮
                    StartCoroutine(AutoHideButtons());
                }
                else
                {
                    // 如果按钮已经显示，则隐藏
                    SetButtonsVisible(false);
                }
            }
            
            /// <summary>
            /// 自动隐藏按钮
            /// </summary>
            private IEnumerator AutoHideButtons()
            {
                yield return new WaitForSeconds(3f);
                SetButtonsVisible(false);
            }
            
            /// <summary>
            /// 创建交互按钮
            /// </summary>
            private void CreateInteractionButtons()
            {
                // 创建按钮容器
                buttonContainer = new GameObject("ButtonContainer");
                buttonContainer.transform.SetParent(transform);
                
                // 设置按钮容器的RectTransform
                var containerRect = buttonContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0, 0);
                containerRect.anchorMax = new Vector2(1, 1);
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;
                
                // 创建Reserve按钮（左边）- 黄色，位置：左边5%-40%，底部10%-30%
                CreateButton("ReserveButton", "Reserve", new Vector2(0.05f, 0.1f), new Vector2(0.4f, 0.3f), OnReserveButtonClick, Color.yellow);
                
                // 创建Buy按钮（右边）- 蓝色，位置：右边60%-95%，底部10%-30%
                CreateButton("BuyButton", "Buy", new Vector2(0.6f, 0.1f), new Vector2(0.95f, 0.3f), OnBuyButtonClick, Color.blue);
                
                // 初始时隐藏按钮
                SetButtonsVisible(false);
                
            }
            
            /// <summary>
            /// 创建按钮
            /// </summary>
            private void CreateButton(string buttonName, string buttonText, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick, Color buttonColor)
            {
                var buttonGO = new GameObject(buttonName);
                buttonGO.transform.SetParent(buttonContainer.transform);
                
                // 添加Button组件
                var button = buttonGO.AddComponent<Button>();
                
                // 添加Image组件作为按钮背景
                var buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.color = buttonColor; // 使用传入的颜色
                
                // 设置RectTransform
                var buttonRect = buttonGO.GetComponent<RectTransform>();
                buttonRect.anchorMin = anchorMin;
                buttonRect.anchorMax = anchorMax;
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;
                
                // 创建文本
                var textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform);
                
                var text = textGO.AddComponent<Text>();
                text.text = buttonText;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 12; // 放大2倍：14 * 2 = 28
                text.color = Color.red;
                text.alignment = TextAnchor.MiddleCenter;
                
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                // 设置按钮点击事件
                button.onClick.AddListener(onClick);
                
                // 根据按钮类型设置引用
                if (buttonName == "ReserveButton")
                {
                    reserveButton = button;
                }
                else if (buttonName == "BuyButton")
                {
                    buyButton = button;
                }
            }
            
            /// <summary>
            /// 设置按钮可见性
            /// </summary>
            public void SetButtonsVisible(bool visible)
            {
                if (buttonContainer != null)
                {
                    buttonContainer.SetActive(visible);
                }
            }
            
            /// <summary>
            /// Reserve按钮点击事件
            /// </summary>
            private void OnReserveButtonClick()
            {                
                // 检查是否可以reserve这张卡片
                if (CanReserveCard())
                {
                    // 创建reserve action
                    var reserveAction = CreateReserveAction();
                    if (reserveAction != null)
                    {
                        // 发送action给GameManager
                        SendActionToGameManager(reserveAction);
                        
                        // 隐藏按钮
                        SetButtonsVisible(false);
                
                    }
                    else
                    {
                        // 显示错误提示
                        ShowInvalidActionMessage("Failed to create reserve action");
                    }
                }
                else
                {
                    // 显示错误提示
                    ShowInvalidActionMessage("Invalid action: Cannot reserve this card");
                }
            }
            
            /// <summary>
            /// Buy按钮点击事件
            /// </summary>
            private void OnBuyButtonClick()
            {                
                // 显示购买UI
                ShowBuyUI();
            }
            
            /// <summary>
            /// 初始化卡片显示
            /// </summary>
            public void Initialize(int index, Sprite defaultSprite)
            {
                name = $"CardDisplay_{index}";
                
                if (cardImage != null && defaultSprite != null)
                {
                    cardImage.sprite = defaultSprite;
                    cardImage.color = Color.white;
                }
                
                if (cardText != null)
                {
                    cardText.text = $"Card {index}";
                    cardText.color = Color.black;
                }
            }
            
            /// <summary>
            /// 更新卡片显示
            /// </summary>
            public void UpdateCard(Card card, Sprite sprite)
            {
                currentCard = card;
                
                if (cardImage != null)
                {
                    if (sprite != null)
                    {
                        cardImage.sprite = sprite;
                        cardImage.color = Color.white;
                    }
                    else
                    {
                        cardImage.color = Color.gray;
                    }
                }
                
                if (cardText != null)
                {
                    if (card != null)
                    {
                        cardText.text = $"{card.Code}\n{card.Points}pts";
                    }
                    else
                    {
                        cardText.text = "Empty";
                    }
                }
                
                // 更新按钮状态 - 只有有卡片的槽位才能交互
                if (buttonContainer != null)
                {
                    bool hasCard = card != null;
                    SetButtonsVisible(false); // 先隐藏按钮
                    
                    // 只有有卡片的槽位才能接收点击事件
                    if (cardImage != null)
                    {
                        cardImage.raycastTarget = hasCard;
                    }
                }
            }
            
            /// <summary>
            /// 检查是否可以reserve这张卡片
            /// </summary>
            private bool CanReserveCard()
            {
                if (currentCard == null)
                {
                    return false;
                }
                
                // 获取当前游戏状态和玩家
                var gameManager = FindObjectOfType<Core.GameManager>();
                if (gameManager == null)
                {
                    return false;
                }
                
                // 获取当前游戏状态
                var gameState = gameManager.gameRule?.CurrentGameState as SplendorUnity.Core.SplendorGameState;
                if (gameState == null)
                {
                    return false;
                }
                
                // 获取当前回合的玩家ID
                int currentPlayerId = gameState.agentToMove;
                
                // 获取valid actions
                var validActions = gameManager.gameRule?.GetLegalActions((object)gameState, currentPlayerId);
                if (validActions == null || validActions.Count == 0)
                {
                    return false;
                }
                
                // 检查是否有reserve类型的action包含当前卡片
                foreach (var action in validActions)
                {
                    if (action is Core.Action actionObj && actionObj.Type == "reserve")
                    {
                        if (actionObj.Card != null && actionObj.Card.Code == currentCard.Code)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            
            /// <summary>
            /// 检查玩家是否能够购买当前卡片
            /// </summary>
            private bool CanBuyCard(SplendorUnity.Core.SplendorGameState gameState, SplendorUnity.Core.SplendorGameState.AgentState currentPlayer, SplendorUnity.Models.Card targetCard)
            {
                if (targetCard == null)
                {
                    return false;
                }
                
                if (currentPlayer == null)
                {
                    return false;
                }
                
                // 获取当前游戏状态和玩家
                var gameManager = FindObjectOfType<Core.GameManager>();
                if (gameManager == null)
                {
                    return false;
                }
                
                // 使用传入的currentPlayer的ID
                int currentPlayerId = currentPlayer.id;
                
                // 获取valid actions
                var validActions = gameManager.gameRule?.GetLegalActions((object)gameState, currentPlayerId);
                if (validActions == null || validActions.Count == 0)
                {
                    return false;
                }
                
                // 检查是否有buy类型的action包含当前卡片
                foreach (var action in validActions)
                {
                    if (action is Core.Action actionObj && (actionObj.Type == "buy" || actionObj.Type == "buy_reserved"))
                    {
                        if (actionObj.Card != null && actionObj.Card.Code == targetCard.Code)
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            
            /// <summary>
            /// 创建reserve action
            /// </summary>
            private Core.Action CreateReserveAction()
            {
                if (currentCard == null)
                {
                    return null;
                }
                
                // 获取当前游戏状态
                var gameManager = FindObjectOfType<Core.GameManager>();
                if (gameManager == null)
                {
                    return null;
                }
                
                var gameState = gameManager.gameRule?.CurrentGameState as SplendorUnity.Core.SplendorGameState;
                if (gameState == null)
                {
                    return null;
                }
                
                // 检查是否有黄色宝石可以收集
                var yellowGemCount = gameState.board.gems.ContainsKey("yellow") ? gameState.board.gems["yellow"] : 0;
                var collectedGems = yellowGemCount > 0 ? new Dictionary<string, int> { { "yellow", 1 } } : new Dictionary<string, int>();
                
                // 创建reserve action
                var reserveAction = Core.Action.CreateReserveAction(currentCard, collectedGems);
                
                return reserveAction;
            }
            
            /// <summary>
            /// 发送action给GameManager
            /// </summary>
            private void SendActionToGameManager(Core.Action action)
            {
                var gameManager = FindObjectOfType<Core.GameManager>();
                if (gameManager != null)
                {
                    gameManager.SetPlayerAction(action);
                }
            }
            
            /// <summary>
            /// 显示购买UI
            /// </summary>
            private void ShowBuyUI()
            {
                if (currentCard == null)
                {
                    return;
                }
                
                // 查找或创建BuyUI
                var buyUI = FindObjectOfType<UI.BuyUI>();
                if (buyUI == null)
                {
                    // 如果没有找到，创建一个新的
                    buyUI = CreateBuyUI();
                }
                
                if (buyUI != null)
                {
                    // 获取当前游戏状态和玩家
                    var gameManager = FindObjectOfType<Core.GameManager>();
                    if (gameManager != null && gameManager.gameRule?.CurrentGameState is SplendorUnity.Core.SplendorGameState gameState)
                    {
                        // 获取当前回合的玩家
                        var currentPlayer = gameState.agents[gameState.agentToMove];
                        if (currentPlayer != null)
                        {
                            // 检查玩家是否能够购买当前卡片
                            if (CanBuyCard(gameState, currentPlayer, currentCard))
                            {
                                // 显示购买面板
                                buyUI.ShowPanel(gameState, currentPlayer, currentCard);
                                
                                // 隐藏卡片按钮
                                SetButtonsVisible(false);
                            }
                            else
                            {
                                // 无法购买，显示提示信息
                                ShowInvalidActionMessage("Cannot buy this card - insufficient resources");
                            }
                        }
                    }
                }
            }
            
            /// <summary>
            /// 创建BuyUI
            /// </summary>
            private UI.BuyUI CreateBuyUI()
            {
                // 创建Canvas
                var canvasGO = new GameObject("BuyUICanvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // 确保在最上层
                
                // 添加CanvasScaler
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                // 添加GraphicRaycaster
                canvasGO.AddComponent<GraphicRaycaster>();
                
                // 创建BuyUI组件
                var buyUI = canvasGO.AddComponent<UI.BuyUI>();
                
                return buyUI;
            }
            
            /// <summary>
            /// 显示无效操作提示消息
            /// </summary>
            private void ShowInvalidActionMessage(string message)
            {
                // 查找或创建InvalidActionUI
                var invalidActionUI = FindObjectOfType<InvalidActionUI>();
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
            private InvalidActionUI CreateInvalidActionUI()
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
                var invalidActionUI = canvasGO.AddComponent<InvalidActionUI>();
                
                return invalidActionUI;
            }
            
            /// <summary>
            /// 高亮显示
            /// </summary>
            public void Highlight()
            {
                if (cardImage != null)
                {
                    cardImage.color = Color.yellow;
                }
            }
            
            /// <summary>
            /// 取消高亮
            /// </summary>
            public void Unhighlight()
            {
                if (cardImage != null)
                {
                    cardImage.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// 无效操作提示UI组件
        /// </summary>
        [System.Serializable]
        public class InvalidActionUI : MonoBehaviour
        {
            [Header("UI组件")]
            public GameObject messagePanel;
            public Text messageText;
            public Button closeButton;
            
            [Header("显示设置")]
            public float autoHideDelay = 3f; // 自动隐藏延迟时间
            public Color backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f); // 红色半透明背景
            public Color textColor = Color.white; // 白色文字
            
            private void Awake()
            {
                CreateUI();
            }
            
            /// <summary>
            /// 创建UI界面
            /// </summary>
            private void CreateUI()
            {
                // 创建消息面板
                messagePanel = new GameObject("MessagePanel");
                messagePanel.transform.SetParent(transform);
                
                // 添加Image组件作为背景
                var panelImage = messagePanel.AddComponent<Image>();
                panelImage.color = backgroundColor;
                
                // 设置RectTransform
                var panelRect = messagePanel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.3f, 0.4f);
                panelRect.anchorMax = new Vector2(0.7f, 0.6f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                // 创建消息文本
                var textGO = new GameObject("MessageText");
                textGO.transform.SetParent(messagePanel.transform);
                
                messageText = textGO.AddComponent<Text>();
                messageText.text = "Invalid Action";
                messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                messageText.fontSize = 24;
                messageText.color = textColor;
                messageText.alignment = TextAnchor.MiddleCenter;
                messageText.fontStyle = FontStyle.Bold;
                
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.1f, 0.2f);
                textRect.anchorMax = new Vector2(0.9f, 0.8f);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                // 创建关闭按钮
                var buttonGO = new GameObject("CloseButton");
                buttonGO.transform.SetParent(messagePanel.transform);
                
                closeButton = buttonGO.AddComponent<Button>();
                var buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                
                var buttonRect = buttonGO.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.4f, 0.05f);
                buttonRect.anchorMax = new Vector2(0.6f, 0.15f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;
                
                // 添加按钮文本
                var buttonTextGO = new GameObject("ButtonText");
                buttonTextGO.transform.SetParent(buttonGO.transform);
                
                var buttonText = buttonTextGO.AddComponent<Text>();
                buttonText.text = "OK";
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 18;
                buttonText.color = Color.white;
                buttonText.alignment = TextAnchor.MiddleCenter;
                
                var buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;
                
                // 设置按钮点击事件
                closeButton.onClick.AddListener(() => HideMessage());
                
                // 初始时隐藏
                messagePanel.SetActive(false);
                
            }
            
            /// <summary>
            /// 显示消息
            /// </summary>
            public void ShowMessage(string message)
            {
                if (messageText != null)
                {
                    messageText.text = message;
                }
                
                if (messagePanel != null)
                {
                    messagePanel.SetActive(true);
                    
                    // 自动隐藏
                    StartCoroutine(AutoHideMessage());
                    
                }
            }
            
            /// <summary>
            /// 隐藏消息
            /// </summary>
            public void HideMessage()
            {
                if (messagePanel != null)
                {
                    messagePanel.SetActive(false);
                }
            }
            
            /// <summary>
            /// 自动隐藏消息
            /// </summary>
            private IEnumerator AutoHideMessage()
            {
                yield return new WaitForSeconds(autoHideDelay);
                HideMessage();
            }
        }
    }
}
