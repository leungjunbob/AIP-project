using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SplendorUnity.Core;
using SplendorUnity.Utils;

namespace SplendorUnity.Display
{
    /// <summary>
    /// 宝石堆管理器，根据游戏状态自动更新宝石堆的显示图片
    /// </summary>
    public class GemPileManager : MonoBehaviour
    {
        [Header("游戏状态引用")]
        public SplendorGameState gameState; // 添加游戏状态引用
        
        [Header("宝石堆UI组件")]
        public Image blackGemPile;
        public Image redGemPile;
        public Image yellowGemPile;
        public Image greenGemPile;
        public Image blueGemPile;
        public Image whiteGemPile;
        
        [Header("宝石图片资源")]
        public Sprite[] blackGemSprites; // 索引0-7对应数量0-7
        public Sprite[] redGemSprites;
        public Sprite[] yellowGemSprites; // 索引0-5对应数量0-5
        public Sprite[] greenGemSprites;
        public Sprite[] blueGemSprites;
        public Sprite[] whiteGemSprites;
        
        [Header("配置")]
        public bool autoLoadSprites = true;
        public string spriteSize = "large"; // 默认使用large尺寸
        public bool hideWhenZero = true; // 数量为0时是否隐藏图片
        
        [Header("自动更新设置")]
        public bool enableAutoUpdate = true; // 启用自动更新
        public float updateInterval = 0.1f; // 更新间隔（秒）
        public bool updateOnGameEvents = true; // 响应游戏事件更新
        public bool updateOnGameStateChange = true; // 检测游戏状态变化时更新
        
        [Header("测试功能")]
        [SerializeField] private bool testMode = false; // 测试模式开关
        [SerializeField] private int testGemCount = 3; // 测试宝石数量
        
        private Dictionary<string, Image> gemPileImages;
        private Dictionary<string, Sprite[]> gemSprites;
        private Dictionary<string, int> lastKnownGemCounts; // 记录上次的宝石数量
        private Coroutine autoUpdateCoroutine;
        private GameManager gameManager;
        
        private void Awake()
        {
            InitializeGemPileReferences();
            InitializeLastKnownCounts();
            
            // 只有在没有手动设置资源时才自动加载
            if (autoLoadSprites && !HasManualSprites())
            {
                LoadAllGemSprites();
            }
        }
        
        private void Start()
        {
            // 如果没有手动设置gameState，尝试自动查找
            if (gameState == null)
            {
                gameState = FindObjectOfType<SplendorGameState>();
            }
            
            // 查找GameManager
            gameManager = FindObjectOfType<GameManager>();
            
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
        
        private void OnDestroy()
        {
            // 取消订阅事件
            if (updateOnGameEvents)
            {
                UnsubscribeFromGameEvents();
            }
            
            // 停止自动更新
            if (autoUpdateCoroutine != null)
            {
                StopCoroutine(autoUpdateCoroutine);
            }
        }
        
        /// <summary>
        /// 初始化宝石堆引用
        /// </summary>
        private void InitializeGemPileReferences()
        {
            gemPileImages = new Dictionary<string, Image>
            {
                {"black", blackGemPile},
                {"red", redGemPile},
                {"yellow", yellowGemPile},
                {"green", greenGemPile},
                {"blue", blueGemPile},
                {"white", whiteGemPile}
            };
            
            gemSprites = new Dictionary<string, Sprite[]>
            {
                {"black", blackGemSprites},
                {"red", redGemSprites},
                {"yellow", yellowGemSprites},
                {"green", greenGemSprites},
                {"blue", blueGemSprites},
                {"white", whiteGemSprites}
            };
        }
        
        /// <summary>
        /// 初始化上次已知的宝石数量记录
        /// </summary>
        private void InitializeLastKnownCounts()
        {
            lastKnownGemCounts = new Dictionary<string, int>
            {
                {"black", -1}, {"red", -1}, {"yellow", -1}, {"green", -1}, {"blue", -1}, {"white", -1}
            };
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToGameEvents()
        {
            if (gameManager != null)
            {
                GameManager.OnActionExecuted += OnGameActionExecuted;
                GameManager.OnGameStarted += OnGameStarted;
                GameManager.OnGameEnded += OnGameEnded;
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            if (gameManager != null)
            {
                GameManager.OnActionExecuted -= OnGameActionExecuted;
                GameManager.OnGameStarted -= OnGameStarted;
                GameManager.OnGameEnded -= OnGameEnded;
            }
        }
        
        /// <summary>
        /// 游戏动作执行事件处理
        /// </summary>
        private void OnGameActionExecuted(int agentIndex, object action)
        {

            
            // 延迟一帧更新，确保游戏状态已经更新
            StartCoroutine(UpdateAfterFrame());
        }
        
        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStarted(GameManager manager)
        {
           
            // 延迟更新，确保游戏状态已初始化
            StartCoroutine(UpdateAfterFrame());
        }
        
        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        private void OnGameEnded(GameManager manager)
        {
        }
        
        /// <summary>
        /// 延迟一帧后更新
        /// </summary>
        private IEnumerator UpdateAfterFrame()
        {
            yield return null; // 等待一帧
            UpdateGemPilesFromGameState();
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
            if (gameState?.board?.gems == null) return;
            
            bool hasChanged = false;
            
            foreach (var gem in gameState.board.gems)
            {
                string gemType = gem.Key;
                int currentCount = gem.Value;
                
                if (lastKnownGemCounts.ContainsKey(gemType) && 
                    lastKnownGemCounts[gemType] != currentCount)
                {
                    hasChanged = true;
                    break;
                }
            }
            
            if (hasChanged)
            {
                
                UpdateGemPilesFromGameState();
            }
        }
        
        /// <summary>
        /// 从游戏状态更新宝石堆
        /// </summary>
        public void UpdateGemPilesFromGameState()
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
            
            if (gameState?.board?.gems != null)
            {
                UpdateGemPiles(gameState);
                
                // 更新记录的数量
                foreach (var gem in gameState.board.gems)
                {
                    if (lastKnownGemCounts.ContainsKey(gem.Key))
                    {
                        lastKnownGemCounts[gem.Key] = gem.Value;
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新宝石堆显示
        /// </summary>
        /// <param name="gameState">游戏状态</param>
        public void UpdateGemPiles(SplendorGameState gameState)
        {
            if (gameState?.board?.gems == null) return;
            
            foreach (var gem in gameState.board.gems)
            {
                UpdateGemPile(gem.Key, gem.Value);
            }
        }
        
        /// <summary>
        /// 更新单个宝石堆
        /// </summary>
        /// <param name="gemType">宝石类型</param>
        /// <param name="count">数量</param>
        public void UpdateGemPile(string gemType, int count)
        {
            if (!gemPileImages.ContainsKey(gemType) || !gemSprites.ContainsKey(gemType))
            {
                return;
            }
            
            var image = gemPileImages[gemType];
            var sprites = gemSprites[gemType];
            
            if (image == null)
            {
                return;
            }
            
            // 处理数量为0的情况
            if (count == 0 && hideWhenZero)
            {
                image.enabled = false; // 隐藏图片
                return;
            }
            
            // 确保图片可见
            image.enabled = true;
            
            // 计算正确的数组索引
            int maxCount = GemSpriteLoader.GetGemTypeMaxCount(gemType);
            int spriteIndex = Mathf.Clamp(count, 0, maxCount);
            
            if (spriteIndex < sprites.Length && sprites[spriteIndex] != null)
            {
                image.sprite = sprites[spriteIndex];
            }
        }
        
        /// <summary>
        /// 检查是否手动设置了精灵资源
        /// </summary>
        private bool HasManualSprites()
        {
            return blackGemSprites != null && blackGemSprites.Length > 0 ||
                   redGemSprites != null && redGemSprites.Length > 0 ||
                   yellowGemSprites != null && yellowGemSprites.Length > 0 ||
                   greenGemSprites != null && greenGemSprites.Length > 0 ||
                   blueGemSprites != null && blueGemSprites.Length > 0 ||
                   whiteGemSprites != null && whiteGemSprites.Length > 0;
        }
        
        /// <summary>
        /// 自动加载所有宝石图片
        /// </summary>
        private void LoadAllGemSprites()
        {
                
            var loadedSprites = GemSpriteLoader.LoadAllGemSprites(spriteSize);
            
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
        /// 获取当前宝石堆数量
        /// </summary>
        public int GetGemCount(string gemType)
        {
            if (gameState?.board?.gems != null && gameState.board.gems.ContainsKey(gemType))
            {
                return gameState.board.gems[gemType];
            }
            return 0;
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
    }
}
