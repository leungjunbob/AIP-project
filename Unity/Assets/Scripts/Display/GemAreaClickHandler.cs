using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SplendorUnity.Core;
using SplendorUnity.UI;

namespace SplendorUnity.Display
{
    /// <summary>
    /// GemArea点击处理器，处理gem的点击事件并弹出收集窗口
    /// </summary>
    public class GemAreaClickHandler : MonoBehaviour
    {
        [Header("组件引用")]
        public GemAreaManager gemAreaManager;
        public GemCollectionPanelSetup gemCollectionPanelSetup;
        
        [Header("点击配置")]
        public bool enableClickEvents = true;
        
        [Header("收集窗口设置")]
        public bool showCollectionPanelOnClick = true;
        
        private GemCollectionPanel collectionPanel;
        private SplendorGameState currentGameState;
        
        private void Start()
        {
            InitializeClickHandler();
            
            // 订阅游戏事件
            SubscribeToGameEvents();
            
            // 监听游戏开始事件，然后设置游戏状态
            StartCoroutine(WaitForGameStart());
        }
        
        /// <summary>
        /// 初始化点击处理器
        /// </summary>
        private void InitializeClickHandler()
        {
            // 自动查找组件
            if (gemAreaManager == null)
                gemAreaManager = FindObjectOfType<GemAreaManager>();
                
            if (gemCollectionPanelSetup == null)
                gemCollectionPanelSetup = FindObjectOfType<GemCollectionPanelSetup>();
                        
            // 自动为所有gem添加点击事件
            if (gemAreaManager != null)
            {
                // 延迟一帧添加点击事件，确保GemAreaManager已经初始化完成
                StartCoroutine(AddClickEventsDelayed());
            }
        }
        
        /// <summary>
        /// 等待游戏开始，然后设置游戏状态
        /// </summary>
        private System.Collections.IEnumerator WaitForGameStart()
        {
            
            // 等待游戏开始
            while (currentGameState == null)
            {
                // 首先尝试从GemAreaIntegration获取游戏状态
                var gemAreaIntegration = FindObjectOfType<GemAreaIntegration>();
                if (gemAreaIntegration != null && gemAreaIntegration.gameState != null)
                {
                   
                    SetGameState(gemAreaIntegration.gameState);
                    break;
                }
                
                // 如果GemAreaIntegration没有，则尝试从GameManager获取
                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null && gameManager.gameRule != null && gameManager.gameRule.CurrentGameState != null)
                {
                    
                    SetGameState((SplendorGameState)gameManager.gameRule.CurrentGameState);
                    break;
                }
                
                // 等待一帧再检查
                yield return null;
            }
        }
        
        /// <summary>
        /// 延迟添加点击事件
        /// </summary>
        private System.Collections.IEnumerator AddClickEventsDelayed()
        {
            // 等待一帧，确保GemAreaManager完全初始化
            yield return null;
            
            if (gemAreaManager != null)
            {
                AddClickEventsToAllGems();
            }
        }
        
        /// <summary>
        /// 为gem Image添加点击事件
        /// </summary>
        /// <param name="gemImage">gem的Image组件</param>
        /// <param name="gemType">gem类型</param>
        public void AddClickEventToGem(Image gemImage, string gemType)
        {
            if (gemImage == null || !enableClickEvents) return;
            
            // 添加Button组件（如果没有的话）
            Button button = gemImage.GetComponent<Button>();
            if (button == null)
            {
                button = gemImage.gameObject.AddComponent<Button>();
            }
            
            // 设置点击事件
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnGemClicked(gemType));
            
            // 设置按钮过渡效果
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = gemImage;
        }
        
        /// <summary>
        /// 为所有gem添加点击事件
        /// </summary>
        public void AddClickEventsToAllGems()
        {
            if (gemAreaManager == null) return;
            
            // 获取所有gem Image组件
            var gemImages = gemAreaManager.GetGemImages();
            if (gemImages != null)
            {
                foreach (var kvp in gemImages)
                {
                    string gemType = kvp.Key;
                    Image gemImage = kvp.Value;
                    
                    if (gemImage != null)
                    {
                        AddClickEventToGem(gemImage, gemType);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gem点击事件处理
        /// </summary>
        /// <param name="gemType">被点击的gem类型</param>
        public void OnGemClicked(string gemType)
        {
            
            if (!showCollectionPanelOnClick) 
            {
                return;
            }
            
            // 检查组件引用
            if (gemCollectionPanelSetup == null)
            {
                return;
            }
            
            if (currentGameState == null)
            {
                return;
            }
            
            // 检查是否可以收集gem
            if (CanCollectGem(gemType))
            {
                ShowCollectionPanel(gemType);
            }
        }
        
        /// <summary>
        /// 检查是否可以收集gem
        /// </summary>
        /// <param name="gemType">gem类型</param>
        /// <returns>是否可以收集</returns>
        private bool CanCollectGem(string gemType)
        {
            if (currentGameState?.board?.gems == null) 
            {
                return false;
            }
            
            // 检查银行中是否有该类型的gem
            if (!currentGameState.board.gems.ContainsKey(gemType))
            {
                return false;
            }
            
            if (currentGameState.board.gems[gemType] <= 0)
            {
   
                return false;
            }
                        
            // 这里可以添加更多收集规则检查
            // 比如：当前回合、玩家手牌数量限制等
            
            return true;
        }
        
        /// <summary>
        /// 显示收集面板
        /// </summary>
        /// <param name="gemType">被点击的gem类型</param>
        private void ShowCollectionPanel(string gemType)
        {
            
            if (gemCollectionPanelSetup == null)
            {
                return;
            }
        
            
            // 检查是否已经有活动的收集面板
            if (collectionPanel != null && collectionPanel.gameObject.activeInHierarchy)
            {
                return;
            }
            
            
            // 创建收集面板UI
            gemCollectionPanelSetup.CreateGemCollectionPanelUI();
            
            // 获取创建的GemCollectionPanel组件
            collectionPanel = FindObjectOfType<GemCollectionPanel>();
            if (collectionPanel != null)
            {
                
                // 显示面板
                collectionPanel.ShowPanel(currentGameState, GetCurrentPlayer());
                
            }
        }
        
        /// <summary>
        /// 获取当前玩家状态
        /// </summary>
        /// <returns>当前玩家状态</returns>
        private SplendorGameState.AgentState GetCurrentPlayer()
        {
            if (currentGameState?.agents == null || currentGameState.agents.Count == 0)
                return null;
            
            // 查找GameManager来确定当前轮到哪个玩家
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.gameRule != null)
            {
                int currentAgentIndex = gameManager.gameRule.GetCurrentAgentIndex();
                
                // 确保索引在有效范围内
                if (currentAgentIndex >= 0 && currentAgentIndex < currentGameState.agents.Count)
                {
                    var currentPlayer = currentGameState.agents[currentAgentIndex];
                    return currentPlayer;
                }
            }
            
            // 如果无法确定当前玩家，返回第一个玩家作为备选
            return currentGameState.agents[0];
        }
        
        /// <summary>
        /// 设置游戏状态
        /// </summary>
        /// <param name="gameState">游戏状态</param>
        public void SetGameState(SplendorGameState gameState)
        {
            currentGameState = gameState;
            
        }
        
        /// <summary>
        /// 隐藏收集面板
        /// </summary>
        public void HideCollectionPanel()
        {
            if (collectionPanel != null)
            {
                collectionPanel.HidePanel();
            }
        }
        
        
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToGameEvents()
        {
            
            GameManager.OnGameStarted += OnGameStarted;
            GameManager.OnActionExecuted += OnActionExecuted;
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void OnDestroy()
        {
            GameManager.OnGameStarted -= OnGameStarted;
            GameManager.OnActionExecuted -= OnActionExecuted;
        }
        
        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStarted(GameManager manager)
        {
            
            // 立即获取游戏状态
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.gameRule != null && gameManager.gameRule.CurrentGameState != null)
            {                
                SetGameState((SplendorGameState)gameManager.gameRule.CurrentGameState);
            }
        }
        
        /// <summary>
        /// 动作执行事件处理
        /// </summary>
        private void OnActionExecuted(int agentIndex, object action)
        {
            
            // 动作执行后刷新游戏状态
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.gameRule != null && gameManager.gameRule.CurrentGameState != null)
            {
                SetGameState((SplendorGameState)gameManager.gameRule.CurrentGameState);
            }
        }
    }
}
