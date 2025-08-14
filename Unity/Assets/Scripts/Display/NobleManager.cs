using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplendorUnity.Core;
using SplendorUnity.Models;
using SplendorUnity.Events;

namespace SplendorUnity.Display
{
    /// <summary>
    /// 贵族管理器 - 自动管理贵族显示
    /// 根据GameState自动更新贵族显示，自动导入图片资源
    /// </summary>
    public class NobleManager : MonoBehaviour
    {
        [Header("贵族显示设置")]
        public bool enableAutoUpdate = true;
        public float updateInterval = 0.5f;
        public bool updateOnGameEvents = true;
        public bool updateOnGameStateChange = true;
        
        [Header("贵族UI组件")]
        public Transform nobleContainer; // 贵族容器
        public GameObject noblePrefab; // 贵族预制体（可选）
        public int maxNobleSlots = 5; // 最大贵族槽位数
        
        [Header("图片资源")]
        public Sprite noneSprite; // 无贵族时的默认图片
        public string nobleImagePath = "nobles_large/"; // 贵族图片路径
        
        [Header("组件引用")]
        public SplendorGameState gameState;
        public GameManager gameManager;
        
        
        // 私有字段
        private List<NobleDisplay> nobleDisplays = new List<NobleDisplay>();
        private List<Noble> lastKnownNobles = new List<Noble>();
        private Coroutine autoUpdateCoroutine;
        private Dictionary<string, Sprite> nobleSprites = new Dictionary<string, Sprite>();
        
        private void Awake()
        {
            InitializeNobleDisplays();
            LoadNobleSprites();
        }
        
        private void Start()
        {
            // 尝试找到必要的组件
            if (gameState == null)
                gameState = FindObjectOfType<SplendorGameState>();
            
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            // 订阅游戏事件
            if (updateOnGameEvents && gameManager != null)
            {
                SubscribeToGameEvents();
            }
            
            // 启动自动更新
            if (enableAutoUpdate)
            {
                StartAutoUpdate();
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }
        
        /// <summary>
        /// 初始化贵族显示槽位
        /// </summary>
        private void InitializeNobleDisplays()
        {
            if (nobleContainer == null)
            {
                nobleContainer = transform;
            }
            
            // 清除现有的显示
            foreach (var display in nobleDisplays)
            {
                if (display != null)
                    DestroyImmediate(display.gameObject);
            }
            nobleDisplays.Clear();
            
            // 创建贵族显示槽位
            for (int i = 0; i < maxNobleSlots; i++)
            {
                GameObject nobleSlot;
                if (noblePrefab != null)
                {
                    nobleSlot = Instantiate(noblePrefab, nobleContainer);
                }
                else
                {
                    // 创建默认的贵族槽位
                    nobleSlot = new GameObject($"NobleSlot_{i}");
                    nobleSlot.transform.SetParent(nobleContainer);
                    
                    // 添加Image组件
                    var image = nobleSlot.AddComponent<Image>();
                    image.sprite = noneSprite;
                    image.enabled = true;
                    
                    // 设置Image的RectTransform
                    var imageRect = image.GetComponent<RectTransform>();
                    if (imageRect != null)
                    {
                        imageRect.anchorMin = Vector2.zero;
                        imageRect.anchorMax = Vector2.one;
                        imageRect.offsetMin = Vector2.zero;
                        imageRect.offsetMax = Vector2.zero;
                        imageRect.sizeDelta = new Vector2(80, 80); // 设置默认大小
                        
                        // 调整贵族位置，向下移动以适应新的卡牌3行布局
                        // 卡牌现在占用3行，每行高度约250，所以贵族需要向下移动
                        float yOffset = -50; // 向下移动800像素，为卡牌3行布局留出空间
                        imageRect.anchoredPosition = new Vector2(0, yOffset);
                    }
                }
                
                var nobleDisplay = nobleSlot.GetComponent<NobleDisplay>();
                if (nobleDisplay == null)
                {
                    nobleDisplay = nobleSlot.AddComponent<NobleDisplay>();
                }
                
                // 等待一帧让组件初始化完成
                StartCoroutine(InitializeNobleDisplayAfterFrame(nobleDisplay, i));
            }
            
        }
        
        /// <summary>
        /// 在一帧后初始化贵族显示组件
        /// </summary>
        private IEnumerator InitializeNobleDisplayAfterFrame(NobleDisplay nobleDisplay, int index)
        {
            yield return null; // 等待一帧
            
            nobleDisplay.Initialize(index, noneSprite);
            nobleDisplays.Add(nobleDisplay);
                
            // 如果这是最后一个槽位，加载图片资源并更新显示
            if (nobleDisplays.Count == maxNobleSlots)
            {
                LoadNobleSprites();
                UpdateNoblesFromGameState();
            }
        }
        
        /// <summary>
        /// 加载贵族图片资源
        /// </summary>
        private void LoadNobleSprites()
        {
            nobleSprites.Clear();
            
            // 加载none图片
            if (noneSprite != null)
            {
                nobleSprites["none"] = noneSprite;
            }
            
            // 尝试从多个路径加载贵族图片
            string[] possiblePaths = { "nobles_large/", "nobles_small/", "nobles/" };
            
            foreach (string path in possiblePaths)
            {
                var nobleResources = Resources.LoadAll<Sprite>(path);
                if (nobleResources.Length > 0)
                {
                    foreach (var sprite in nobleResources)
                    {
                        // 提取贵族代码（去掉文件扩展名和路径）
                        string fileName = sprite.name;
                        string nobleCode = ConvertFileNameToNobleCode(fileName);
                        
                        if (!string.IsNullOrEmpty(nobleCode) && !nobleSprites.ContainsKey(nobleCode))
                        {
                            nobleSprites[nobleCode] = sprite;
                        }
                    }
                    break; // 找到图片后停止搜索
                }
            }
            
                
            // 如果没有找到图片，尝试从GameData加载贵族信息来预分配
            if (nobleSprites.Count <= 1) // 只有none图片
            {
                LoadNobleSpritesFromGameData();
            }
        }
        
        /// <summary>
        /// 将文件名转换为贵族代码
        /// </summary>
        private string ConvertFileNameToNobleCode(string fileName)
        {
            // 移除文件扩展名
            string name = fileName;
            if (name.Contains("."))
            {
                name = name.Substring(0, name.LastIndexOf('.'));
            }
            
            // 文件名到贵族代码的映射（根据实际文件名）
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
            
            if (fileNameToCodeMap.ContainsKey(name))
            {
                return fileNameToCodeMap[name];
            }
            
            // 如果没有找到映射，尝试直接匹配
            if (name.Length >= 4 && char.IsDigit(name[0]))
            {
                return name;
            }
            
            return null;
        }
        
        /// <summary>
        /// 从GameData加载贵族信息并预分配图片槽位
        /// </summary>
        private void LoadNobleSpritesFromGameData()
        {
            try
            {
                // 尝试从GameData获取贵族信息
                var nobleData = SplendorUnity.Utils.GameData.NOBLES;
                if (nobleData != null)
                {
                    foreach (var nobleInfo in nobleData)
                    {
                        string nobleCode = nobleInfo.Item1;
                        if (!nobleSprites.ContainsKey(nobleCode))
                        {
                            // 为每个贵族创建一个占位符
                            nobleSprites[nobleCode] = noneSprite;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {

            }
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToGameEvents()
        {
            if (gameManager != null)
            {
                GameEvents.OnGameStarted += OnGameStarted;
                GameEvents.OnActionExecuted += OnGameActionExecuted;
                GameEvents.OnGameEnded += OnGameEnded;
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnActionExecuted -= OnGameActionExecuted;
            GameEvents.OnGameEnded -= OnGameEnded;
        }
        
        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStarted()
        {
            StartCoroutine(UpdateAfterFrame());
        }
        
        /// <summary>
        /// 游戏动作执行事件处理
        /// </summary>
        private void OnGameActionExecuted(int agentIndex, object action)
        {
            StartCoroutine(UpdateAfterFrame());
        }
        
        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        private void OnGameEnded()
        {
        }
        
        /// <summary>
        /// 等待一帧后更新
        /// </summary>
        private IEnumerator UpdateAfterFrame()
        {
            yield return null;
            UpdateNoblesFromGameState();
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
        private void StopAutoUpdate()
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
                CheckAndUpdateIfChanged();
            }
        }
        
        /// <summary>
        /// 检查并更新贵族显示（如果发生变化）
        /// </summary>
        private void CheckAndUpdateIfChanged()
        {
            // 优先从GameManager获取最新的游戏状态
            var currentGameState = gameManager?.gameRule?.CurrentGameState as SplendorGameState;
            if (currentGameState == null)
            {
                // 如果GameManager不可用，尝试使用缓存的gameState
                if (gameState == null || gameState.board == null)
                {
                    return;
                }
                currentGameState = gameState;
            }
            
            var currentNobles = currentGameState.board.nobles;
            
            
            // 检查贵族列表是否发生变化
            if (HasNoblesChanged(currentNobles))
            {
                
                // 更新缓存的gameState引用
                gameState = currentGameState;
                
                UpdateNoblesFromGameState();
                lastKnownNobles = currentNobles.ToList();
            }
        }
        
        /// <summary>
        /// 检查贵族列表是否发生变化
        /// </summary>
        private bool HasNoblesChanged(List<Noble> currentNobles)
        {
            if (currentNobles == null)
                return lastKnownNobles.Count > 0;
                
            if (lastKnownNobles.Count != currentNobles.Count)
                return true;
                
            for (int i = 0; i < currentNobles.Count; i++)
            {
                if (i >= lastKnownNobles.Count || 
                    currentNobles[i]?.Code != lastKnownNobles[i]?.Code ||
                    currentNobles[i]?.Points != lastKnownNobles[i]?.Points)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 从游戏状态更新贵族显示
        /// </summary>
        public void UpdateNoblesFromGameState()
        {
            // 优先从GameManager获取最新的游戏状态
            var currentGameState = gameManager?.gameRule?.CurrentGameState as SplendorGameState;
            if (currentGameState == null)
            {
                // 如果GameManager不可用，尝试使用缓存的gameState
                if (gameState == null || gameState.board == null)
                {
                    return;
                }
                currentGameState = gameState;
            }
            
            var nobles = currentGameState.board.nobles;
            if (nobles == null)
            {
                nobles = new List<Noble>();
            }
            

            
            // 更新缓存的gameState引用
            gameState = currentGameState;
            
            // 更新所有槽位
            for (int i = 0; i < maxNobleSlots; i++)
            {
                if (i < nobleDisplays.Count)
                {
                    var display = nobleDisplays[i];
                    if (display == null)
                    {
                        continue;
                    }
                    
                    Noble noble = null;
                    Sprite sprite = noneSprite;
                    
                    if (i < nobles.Count)
                    {
                        noble = nobles[i];
                        if (noble != null)
                        {
                            // 尝试根据贵族代码找到对应的图片
                            string nobleCode = noble.Code;
                            if (nobleSprites.ContainsKey(nobleCode))
                            {
                                sprite = nobleSprites[nobleCode];
                            }
                            else
                            {
                                sprite = noneSprite;
                            }
                        }
                    }
                    
                    // 更新显示
                    display.UpdateNoble(noble, sprite);
                    
                }
            }
            
            // 更新已知的贵族列表
            lastKnownNobles.Clear();
            foreach (var noble in nobles)
            {
                if (noble != null)
                {
                    lastKnownNobles.Add(noble);
                }
            }
        }
        
        
        
        /// <summary>
        /// 立即更新贵族显示
        /// </summary>
        [ContextMenu("立即更新贵族显示")]
        public void UpdateNow()
        {
            UpdateNoblesFromGameState();
        }
        
        /// <summary>
        /// 切换自动更新
        /// </summary>
        [ContextMenu("切换自动更新")]
        public void ToggleAutoUpdate()
        {
            enableAutoUpdate = !enableAutoUpdate;
            if (enableAutoUpdate)
            {
                StartAutoUpdate();
            }
            else
            {
                StopAutoUpdate();
            }
        }
        

        
        /// <summary>
        /// 调试贵族状态信息
        /// </summary>
        [ContextMenu("调试贵族状态")]
        public void DebugNobleState()
        {
            
            // 检查GameManager状态
            if (gameManager != null)
            {
                if (gameManager.gameRule != null)
                {
                    var currentState = gameManager.gameRule.CurrentGameState as SplendorGameState;
                    if (currentState != null)
                    {
                        var boardNobles = currentState.board.nobles;
                    }
                }
            }
            
            // 检查缓存的gameState
            if (gameState != null)
            {
                if (gameState.board != null)
                {
                    var cachedNobles = gameState.board.nobles;
                }
            }
        }
        
        /// <summary>
        /// 强制同步游戏状态并更新显示
        /// </summary>
        [ContextMenu("强制同步游戏状态")]
        public void ForceSyncGameState()
        {
            
            // 强制从GameManager获取最新状态
            if (gameManager?.gameRule?.CurrentGameState != null)
            {
                gameState = gameManager.gameRule.CurrentGameState as SplendorGameState;                
                // 强制更新显示
                UpdateNoblesFromGameState();
                
                // 重置lastKnownNobles
                if (gameState?.board?.nobles != null)
                {
                    lastKnownNobles = gameState.board.nobles.ToList();
                }
            }
        }
        
        /// <summary>
        /// 强制检查状态变化
        /// </summary>
        [ContextMenu("强制检查状态变化")]
        public void ForceCheckStateChange()
        {
           
            CheckAndUpdateIfChanged();
        }
        
         
        /// <summary>
        /// 测试图片加载
        /// </summary>
        private void TestImageLoading()
        {
            
            // 测试从不同路径加载图片
            string[] testPaths = { "nobles_large/", "nobles_small/", "nobles/" };
            
            foreach (string path in testPaths)
            {
                var sprites = Resources.LoadAll<Sprite>(path);
                if (sprites.Length > 0)
                {
                    foreach (var sprite in sprites)
                    {
                        string nobleCode = ConvertFileNameToNobleCode(sprite.name);
                    }
                }
            }
        }
        
        
        /// <summary>
        /// 单个贵族显示组件
        /// </summary>
        [System.Serializable]
        public class NobleDisplay : MonoBehaviour
        {
            [Header("贵族信息")]
            public int slotIndex;
            public Noble currentNoble;
            
            [Header("UI组件")]
            public Image nobleImage;
            public Text nobleText; // 可选：显示贵族信息
            
            private void Awake()
            {
                // 获取或创建必要的UI组件
                if (nobleImage == null)
                {
                    nobleImage = GetComponent<Image>();
                }
                
                if (nobleText == null)
                {
                    nobleText = GetComponentInChildren<Text>();
                }
            }
            
            /// <summary>
            /// 初始化贵族显示
            /// </summary>
            public void Initialize(int index, Sprite defaultSprite)
            {
                slotIndex = index;
                currentNoble = null;
                
                if (nobleImage != null)
                    nobleImage.sprite = defaultSprite;
                
                if (nobleText != null)
                    nobleText.text = $"Slot {index}";
            }
            
            /// <summary>
            /// 更新贵族显示
            /// </summary>
            public void UpdateNoble(Noble noble, Sprite sprite)
            {
                currentNoble = noble;
                
                if (nobleImage != null)
                {
                    nobleImage.sprite = sprite;
                    nobleImage.enabled = true;
                    
                    // 根据是否有贵族调整透明度
                    if (noble != null)
                    {
                        var color = nobleImage.color;
                        color.a = 1.0f;
                        nobleImage.color = color;
                    }
                    else
                    {
                        var color = nobleImage.color;
                        color.a = 0.5f;
                        nobleImage.color = color;
                    }
                }
                
                if (nobleText != null)
                {
                    if (noble != null)
                    {
                        nobleText.text = $"{noble.Code}\n{noble.Points}pts";
                        nobleText.enabled = true;
                    }
                    else
                    {
                        nobleText.text = "空槽位";
                        nobleText.enabled = true;
                    }
                }
                
            }
            
            /// <summary>
            /// 高亮贵族（可选功能）
            /// </summary>
            public void Highlight()
            {
                if (nobleImage != null)
                {
                    var color = nobleImage.color;
                    color.a = 1f;
                    nobleImage.color = color;
                }
            }
            
            /// <summary>
            /// 取消高亮
            /// </summary>
            public void Unhighlight()
            {
                if (nobleImage != null)
                {
                    var color = nobleImage.color;
                    color.a = 0.7f;
                    nobleImage.color = color;
                }
            }
        }
    }
}
