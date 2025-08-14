using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SplendorUnity.Core;
using SplendorUnity.Models;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 宝石收集面板 - 实现点击式收集流程
    /// </summary>
    public class GemCollectionPanel : MonoBehaviour
    {
        [Header("UI Components")]
        public GameObject panelContainer;           // 面板容器
        public Transform gemSelectionContainer;     // 宝石选择容器
        public Transform quantityControlContainer;  // 数量控制容器
        public Button confirmButton;               // 确认按钮
        public Button cancelButton;                // 取消按钮
        public TextMeshProUGUI invalidMessageText; // 无效提示文本
        
        [Header("Gem Selection")]
        public GameObject gemSelectionPrefab;      // 宝石选择预制体
        
        [Header("Quantity Control")]
        public TextMeshProUGUI quantityText;      // 数量显示文本
        
        [Header("Settings")]
        public bool enableDebugLog = false;
        
        // 私有字段
        private Dictionary<string, int> selectedGems;  // 选择的宝石数量
        private Dictionary<string, GameObject> gemButtons; // 宝石容器引用
        private int currentTotalQuantity;              // 当前总数量
        private const int MAX_TOTAL_GEMS = 10;         // 最大总宝石数
        private const int MAX_SAME_COLOR = 2;          // 最大同色宝石数（根据Splendor规则）
        private SplendorGameState gameState;           // 游戏状态
        private SplendorGameState.AgentState currentPlayer; // 当前玩家
        private object matchedValidGameAction;        // 存储找到的匹配的valid action
        
        private void Awake()
        {
            InitializePanel();
        }
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        private void InitializePanel()
        {
            selectedGems = new Dictionary<string, int>
            {
                {"black", 0},
                {"red", 0},
                {"green", 0},
                {"blue", 0},
                {"white", 0}
            };
            
            gemButtons = new Dictionary<string, GameObject>();
            currentTotalQuantity = 0;
            
            // 设置按钮事件
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
            
            // 隐藏面板
            if (panelContainer != null)
                panelContainer.SetActive(false);
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Initialization completed");
        }
        
        /// <summary>
        /// 显示收集面板
        /// </summary>
        public void ShowPanel(SplendorGameState gameState, SplendorGameState.AgentState currentPlayer)
        {
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: ShowPanel called with gameState: {gameState != null}, currentPlayer: {currentPlayer != null}");
            
            this.gameState = gameState;
            this.currentPlayer = currentPlayer;
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Fields set - gameState: {this.gameState != null}, currentPlayer: {this.currentPlayer != null}");
            
            // 显示当前玩家的详细信息
            if (this.currentPlayer != null)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"GemCollectionPanel: 当前玩家ID: {this.currentPlayer.id}");
                    Debug.Log($"GemCollectionPanel: 当前玩家分数: {this.currentPlayer.score}");
                    
                    int totalGems = 0;
                    Debug.Log("GemCollectionPanel: 当前玩家宝石详情:");
                    foreach (var kvp in this.currentPlayer.gems)
                    {
                        Debug.Log($"  {kvp.Key}: {kvp.Value}");
                        totalGems += kvp.Value;
                    }
                    Debug.Log($"GemCollectionPanel: 当前玩家总宝石数: {totalGems}");
                }
            }
            
            // 重置选择
            ResetSelection();
            
            // 重置匹配的valid action
            matchedValidGameAction = null;
            
            // 创建宝石选择按钮
            CreateGemSelectionButtons();
            
            // 更新数量控制
            UpdateQuantityControl();
            
            // 显示面板
            if (panelContainer != null)
                panelContainer.SetActive(true);
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Showing collection panel");
        }
        
        /// <summary>
        /// 隐藏收集面板
        /// </summary>
        public void HidePanel()
        {
            if (panelContainer != null)
                panelContainer.SetActive(false);
            
            // 重置匹配的valid action
            matchedValidGameAction = null;
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Hiding collection panel");
        }
        
        /// <summary>
        /// 创建宝石选择按钮
        /// </summary>
        private void CreateGemSelectionButtons()
        {
            if (gemSelectionContainer == null) return;
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Starting to create gem selection buttons");
            
            // 清除现有按钮
            foreach (Transform child in gemSelectionContainer)
            {
                Destroy(child.gameObject);
            }
            gemButtons.Clear();
            
            // 创建宝石按钮（排除黄色）
            string[] gemTypes = {"black", "red", "green", "blue", "white"};
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Preparing to create {gemTypes.Length} gem buttons");
            
            foreach (string gemType in gemTypes)
            {
                // 检查银行中是否有该类型宝石
                if (gameState.board.gems.ContainsKey(gemType))
                {
                    int gemCount = gameState.board.gems[gemType];
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: {gemType} gems in bank: {gemCount}");
                    
                    // 即使数量为0也创建按钮，但设置为不可交互
                    CreateGemButton(gemType, gemCount);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: No {gemType} gems in bank");
                    // Even if there are no gems in bank, create button (set as non-interactive)
                    CreateGemButton(gemType, 0);
                }
            }
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Gem selection buttons created, total: {gemButtons.Count}");
            
            // 在所有宝石按钮创建完成后，创建操作按钮
            CreateActionButtons();
        }
        
        /// <summary>
        /// 获取宝石的默认颜色
        /// </summary>
        private Color GetGemColor(string gemType)
        {
            switch (gemType.ToLower())
            {
                case "black": return Color.black;
                case "red": return Color.red;
                case "green": return Color.green;
                case "blue": return Color.blue;
                case "white": return Color.white;
                case "yellow": return Color.yellow;
                default: return Color.gray;
            }
        }
        
        /// <summary>
        /// 创建宝石数量显示文本
        /// </summary>
        private void CreateGemCountText(GameObject buttonGO, int count)
        {
            GameObject textGO = new GameObject("GemCountText");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            // 尝试添加TextMeshProUGUI组件，如果失败则使用普通Text
            TextMeshProUGUI tmpText = null;
            try
            {
                tmpText = textGO.AddComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = count.ToString();
                    tmpText.fontSize = 12;
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontStyle = FontStyles.Bold;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"GemCollectionPanel: 无法添加TextMeshProUGUI组件，尝试使用普通Text: {e.Message}");
            }
            
            // 如果TextMeshProUGUI失败，尝试使用普通Text
            if (tmpText == null)
            {
                try
                {
                    UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
                    if (text != null)
                    {
                        text.text = count.ToString();
                        text.fontSize = 12;
                        text.color = Color.white;
                        text.alignment = TextAnchor.MiddleCenter;
                        text.fontStyle = FontStyle.Bold;
                        if (enableDebugLog)
                            Debug.Log("GemCollectionPanel: 成功使用普通Text组件替代TextMeshProUGUI");
                    }
                }
                catch (System.Exception e2)
                {
                    if (enableDebugLog)
                        Debug.LogError($"GemCollectionPanel: 无法添加任何文本组件: {e2.Message}");
                }
            }
            
            // 设置文本位置（右上角）
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.sizeDelta = new Vector2(20, 20);
                textRect.anchoredPosition = new Vector2(15, 15);
                textRect.anchorMin = new Vector2(1, 1);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.pivot = new Vector2(1, 1);
            }
        }
        
        /// <summary>
        /// 创建单个宝石选择区域（包含图标和+/-按钮）
        /// </summary>
        private void CreateGemButton(string gemType, int gemCount)
        {
            // 创建主容器
            GameObject gemContainer = new GameObject($"GemContainer_{gemType}");
            gemContainer.transform.SetParent(gemSelectionContainer, false);
            
            // 设置容器大小和位置
            RectTransform containerRect = gemContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(80, 120); // 宽度80，高度120
            
            // 计算容器位置（水平排列，间隔更宽）
            int buttonIndex = gemButtons.Count;
            float xOffset = (buttonIndex - 2) * 100; // 每个容器间隔100像素
            containerRect.anchoredPosition = new Vector2(xOffset, 0);
            
            // 创建gem图标（只显示，不可点击）
            GameObject gemIcon = new GameObject($"GemIcon_{gemType}");
            gemIcon.transform.SetParent(gemContainer.transform, false);
            
            // 添加Image组件
            Image image = gemIcon.AddComponent<Image>();
            
            // 加载宝石sprite
            string spritePath = $"gems_large/{gemType}_1";
            Sprite gemSprite = Resources.Load<Sprite>(spritePath);
            if (gemSprite != null)
            {
                image.sprite = gemSprite;
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogWarning($"GemCollectionPanel: Cannot load {gemType} gem sprite: {spritePath}");
                // 如果无法加载sprite，设置默认颜色
                image.color = GetGemColor(gemType);
            }
            
            // 设置图标大小和位置（在容器顶部）
            RectTransform iconRect = gemIcon.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(70, 70); // 图标变大到70x70
            iconRect.anchoredPosition = new Vector2(0, 25);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 创建选择数量显示
            CreateGemSelectionCount(gemContainer, gemType);
            
            // 创建+/-按钮
            CreateGemControlButtons(gemContainer, gemType, gemCount);
            
            // 存储容器引用（用于后续更新）
            gemButtons[gemType] = gemContainer; // 存储GameObject引用
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Created {gemType} gem container, count: {gemCount}");
        }
        
        /// <summary>
        /// 创建操作按钮（Cancel和Confirm）
        /// </summary>
        private void CreateActionButtons()
        {
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Creating action buttons...");
            
            // 创建按钮容器
            GameObject buttonContainer = new GameObject("ActionButtonContainer");
            buttonContainer.transform.SetParent(gemSelectionContainer, false);
            
            // 设置容器位置（在所有gem下方）
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(400, 60);
            containerRect.anchoredPosition = new Vector2(0, -80);
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 创建Cancel按钮
            GameObject cancelGO = new GameObject("CancelButton");
            cancelGO.transform.SetParent(buttonContainer.transform, false);
            
            Button cancelBtn = cancelGO.AddComponent<Button>();
            Image cancelImage = cancelGO.AddComponent<Image>();
            cancelImage.color = new Color(0.8f, 0.2f, 0.2f); // 红色
            
            // 设置Cancel按钮大小和位置
            RectTransform cancelRect = cancelGO.GetComponent<RectTransform>();
            cancelRect.sizeDelta = new Vector2(120, 40);
            cancelRect.anchoredPosition = new Vector2(-80, 0);
            cancelRect.anchorMin = new Vector2(0.5f, 0.5f);
            cancelRect.anchorMax = new Vector2(0.5f, 0.5f);
            cancelRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 添加Cancel文本
            CreateButtonText(cancelGO, "Cancel", 16);
            
            // 设置Cancel按钮事件
            cancelBtn.onClick.AddListener(OnCancelClicked);
            
            // 创建Confirm按钮
            GameObject confirmGO = new GameObject("ConfirmButton");
            confirmGO.transform.SetParent(buttonContainer.transform, false);
            
            Button confirmBtn = confirmGO.AddComponent<Button>();
            Image confirmImage = confirmGO.AddComponent<Image>();
            confirmImage.color = new Color(0.2f, 0.8f, 0.2f); // 绿色
            
            // 设置Confirm按钮大小和位置
            RectTransform confirmRect = confirmGO.GetComponent<RectTransform>();
            confirmRect.sizeDelta = new Vector2(120, 40);
            confirmRect.anchoredPosition = new Vector2(80, 0);
            confirmRect.anchorMin = new Vector2(0.5f, 0.5f);
            confirmRect.anchorMax = new Vector2(0.5f, 0.5f);
            confirmRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 添加Confirm文本
            CreateButtonText(confirmGO, "Confirm", 16);
            
            // 设置Confirm按钮事件
            confirmBtn.onClick.AddListener(OnConfirmClicked);
            
            // 存储按钮引用到字段
            cancelButton = cancelBtn;
            confirmButton = confirmBtn;
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Action buttons created successfully. cancelButton: {cancelButton != null}, confirmButton: {confirmButton != null}");
        }
        
        /// <summary>
        /// 创建宝石选择数量显示
        /// </summary>
        private void CreateGemSelectionCount(GameObject container, string gemType)
        {
            GameObject countGO = new GameObject("SelectionCount");
            countGO.transform.SetParent(container.transform, false);
            
            // 尝试添加TextMeshProUGUI组件，如果失败则使用普通Text
            TextMeshProUGUI tmpText = null;
            try
            {
                tmpText = countGO.AddComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = "0";
                    tmpText.fontSize = 16;
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontStyle = FontStyles.Bold;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"GemCollectionPanel: 无法添加TextMeshProUGUI组件，尝试使用普通Text: {e.Message}");
            }
            
            // 如果TextMeshProUGUI失败，尝试使用普通Text
            if (tmpText == null)
            {
                try
                {
                    UnityEngine.UI.Text text = countGO.AddComponent<UnityEngine.UI.Text>();
                    if (text != null)
                    {
                        text.text = "0";
                        text.fontSize = 16;
                        text.color = Color.white;
                        text.alignment = TextAnchor.MiddleCenter;
                        text.fontStyle = FontStyle.Bold;
                        if (enableDebugLog)
                            Debug.Log("GemCollectionPanel: 成功使用普通Text组件替代TextMeshProUGUI");
                    }
                }
                catch (System.Exception e2)
                {
                    if (enableDebugLog)
                        Debug.LogError($"GemCollectionPanel: 无法添加任何文本组件: {e2.Message}");
                }
            }
            
            // 设置文本位置（在图标下方中央）
            RectTransform textRect = countGO.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.sizeDelta = new Vector2(40, 20);
                textRect.anchoredPosition = new Vector2(0, 0);
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
            }
            
            // 存储引用以便后续更新
            if (gemType == "black") selectedGems["black"] = 0;
            else if (gemType == "red") selectedGems["red"] = 0;
            else if (gemType == "green") selectedGems["green"] = 0;
            else if (gemType == "blue") selectedGems["blue"] = 0;
            else if (gemType == "white") selectedGems["white"] = 0;
        }
        
        /// <summary>
        /// 创建宝石控制按钮（+/-）
        /// </summary>
        private void CreateGemControlButtons(GameObject container, string gemType, int availableCount)
        {
            // 获取玩家当前拥有的该类型宝石数量（用于return）
            int playerOwnedGems = 0;
            if (currentPlayer?.gems != null && currentPlayer.gems.ContainsKey(gemType))
            {
                playerOwnedGems = currentPlayer.gems[gemType];
            }
            
            if (enableDebugLog)
                Debug.Log($"CreateGemControlButtons: {gemType} - 银行可用: {availableCount}, 玩家拥有: {playerOwnedGems}");
            
            // 创建-按钮（左边）
            GameObject minusGO = new GameObject("MinusButton");
            minusGO.transform.SetParent(container.transform, false);
            
            Button minusButton = minusGO.AddComponent<Button>();
            Image minusImage = minusGO.AddComponent<Image>();
            minusImage.color = new Color(0.8f, 0.2f, 0.2f); // 红色
            
            // 设置-按钮大小和位置
            RectTransform minusRect = minusGO.GetComponent<RectTransform>();
            minusRect.sizeDelta = new Vector2(25, 25);
            minusRect.anchoredPosition = new Vector2(-20, -30);
            minusRect.anchorMin = new Vector2(0.5f, 0.5f);
            minusRect.anchorMax = new Vector2(0.5f, 0.5f);
            minusRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 添加-文本
            CreateButtonText(minusGO, "-", 14);
            
            // 设置-按钮事件，传递玩家拥有的宝石数量作为下限
            minusButton.onClick.AddListener(() => OnGemMinusClicked(gemType, playerOwnedGems));
            
            // 创建+按钮（右边）
            GameObject plusGO = new GameObject("PlusButton");
            plusGO.transform.SetParent(container.transform, false);
            
            Button plusButton = plusGO.AddComponent<Button>();
            Image plusImage = plusGO.AddComponent<Image>();
            plusImage.color = new Color(0.2f, 0.8f, 0.2f); // 绿色
            
            // 设置+按钮大小和位置
            RectTransform plusRect = plusGO.GetComponent<RectTransform>();
            plusRect.sizeDelta = new Vector2(25, 25);
            plusRect.anchoredPosition = new Vector2(20, -30);
            plusRect.anchorMin = new Vector2(0.5f, 0.5f);
            minusRect.anchorMax = new Vector2(0.5f, 0.5f);
            plusRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 添加+文本
            CreateButtonText(plusGO, "+", 14);
            
            // 设置+按钮事件
            plusButton.onClick.AddListener(() => OnGemPlusClicked(gemType, availableCount));
            
            // 设置初始按钮状态
            UpdateGemButtonStates(gemType);
            
            if (enableDebugLog)
                Debug.Log($"CreateGemControlButtons: {gemType} - 初始按钮状态已设置");
        }
        
        /// <summary>
        /// 创建按钮文本
        /// </summary>
        private void CreateButtonText(GameObject buttonGO, string text, int fontSize)
        {
            GameObject textGO = new GameObject("ButtonText");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            // 尝试添加TextMeshProUGUI组件，如果失败则使用普通Text
            TextMeshProUGUI tmpText = null;
            try
            {
                tmpText = textGO.AddComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = text;
                    tmpText.fontSize = fontSize;
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontStyle = FontStyles.Bold;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"GemCollectionPanel: 无法添加TextMeshProUGUI组件，尝试使用普通Text: {e.Message}");
            }
            
            // 如果TextMeshProUGUI失败，尝试使用普通Text
            if (tmpText == null)
            {
                try
                {
                    UnityEngine.UI.Text textComponent = textGO.AddComponent<UnityEngine.UI.Text>();
                    if (textComponent != null)
                    {
                        textComponent.text = text;
                        textComponent.fontSize = fontSize;
                        textComponent.color = Color.white;
                        textComponent.alignment = TextAnchor.MiddleCenter;
                        textComponent.fontStyle = FontStyle.Bold;
                    }
                }
                catch (System.Exception e2)
                {
                    if (enableDebugLog)
                        Debug.LogError($"GemCollectionPanel: 无法添加任何文本组件: {e2.Message}");
                }
            }
            
            // 设置文本位置（居中）
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.sizeDelta = new Vector2(25, 25);
                textRect.anchoredPosition = Vector2.zero;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }
        
        /// <summary>
        /// 宝石+按钮点击事件
        /// </summary>
        private void OnGemPlusClicked(string gemType, int availableCount)
        {
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Clicked {gemType} gem plus button");
            
            if (selectedGems.ContainsKey(gemType))
            {
                int currentSelected = selectedGems[gemType];
                int newSelected = currentSelected + 1;
                
                // 上限：银行中可用的宝石数量
                int maxLimit = availableCount;
                
                if (newSelected <= maxLimit)
                {
                    selectedGems[gemType] = newSelected;
                    
                    // 更新总数量（注意：负数表示return，不增加总数量）
                    if (newSelected >= 0)
                    {
                        currentTotalQuantity++;
                    }
                    
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: {gemType} gem选择数量: {newSelected} (上限: {maxLimit})");
                    
                    // 更新数量控制
                    UpdateQuantityControl();
                    
                    // 更新选择数量显示
                    UpdateGemSelectionCount(gemType);
                    
                    // 更新按钮状态
                    UpdateGemButtonStates(gemType);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: {gemType} gem已达到上限 {maxLimit}，无法再增加");
                }
            }
        }
        
        /// <summary>
        /// 宝石-按钮点击事件
        /// </summary>
        /// <param name="gemType">宝石类型</param>
        /// <param name="playerOwnedGems">玩家拥有的该类型宝石数量</param>
        private void OnGemMinusClicked(string gemType, int playerOwnedGems)
        {
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Clicked {gemType} gem minus button, player owns {playerOwnedGems}");
            
            if (selectedGems.ContainsKey(gemType))
            {
                int currentSelected = selectedGems[gemType];
                int newSelected = currentSelected - 1;
                
                // 下限：如果玩家没有该类型宝石，下限为0；如果有，下限为-玩家拥有的数量（表示return）
                int minLimit = -playerOwnedGems;
                
                if (newSelected >= minLimit)
                {
                    selectedGems[gemType] = newSelected;
                    
                    // 更新总数量（注意：负数表示return，不减少总数量）
                    if (newSelected >= 0)
                    {
                        currentTotalQuantity--;
                    }
                    
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: {gemType} gem选择数量: {newSelected} (下限: {minLimit})");
                    
                    // 更新数量控制
                    UpdateQuantityControl();
                    
                    // 更新选择数量显示
                    UpdateGemSelectionCount(gemType);
                    
                    // 更新按钮状态
                    UpdateGemButtonStates(gemType);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: {gemType} gem已达到下限 {minLimit}，无法再减少");
                }
            }
        }
        
        /// <summary>
        /// 更新宝石选择数量显示
        /// </summary>
        private void UpdateGemSelectionCount(string gemType)
        {
            // 查找对应的容器
            Transform container = gemSelectionContainer.Find($"GemContainer_{gemType}");
            if (container != null)
            {
                Transform countText = container.Find("SelectionCount");
                if (countText != null)
                {
                    int selectedCount = selectedGems[gemType];
                    string displayText;
                    
                    // 处理负数显示（return的宝石）
                    if (selectedCount < 0)
                    {
                        displayText = $"Return {Mathf.Abs(selectedCount)}";
                    }
                    else if (selectedCount == 0)
                    {
                        displayText = "0";
                    }
                    else
                    {
                        displayText = selectedCount.ToString();
                    }
                    
                    // 尝试更新TextMeshProUGUI
                    TextMeshProUGUI tmpText = countText.GetComponent<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        tmpText.text = displayText;
                        // 设置颜色：负数显示为红色，正数显示为绿色，0显示为默认色
                        if (selectedCount < 0)
                            tmpText.color = Color.red;
                        else if (selectedCount > 0)
                            tmpText.color = Color.green;
                        else
                            tmpText.color = Color.white;
                    }
                    else
                    {
                        // 尝试更新普通Text
                        UnityEngine.UI.Text text = countText.GetComponent<UnityEngine.UI.Text>();
                        if (text != null)
                        {
                            text.text = displayText;
                            // 设置颜色
                            if (selectedCount < 0)
                                text.color = Color.red;
                            else if (selectedCount > 0)
                                text.color = Color.green;
                            else
                                text.color = Color.white;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新数量控制
        /// </summary>
        private void UpdateQuantityControl()
        {
            // 计算实际的总数量（不包括return的宝石）
            int actualTotalQuantity = 0;
            int returnTotalQuantity = 0;
            
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    actualTotalQuantity += kvp.Value;
                }
                else if (kvp.Value < 0)
                {
                    returnTotalQuantity += Mathf.Abs(kvp.Value);
                }
            }
            
            // 计算玩家当前宝石总数
            int currentPlayerTotalGems = 0;
            if (currentPlayer != null && currentPlayer.gems != null)
            {
                foreach (var kvp in currentPlayer.gems)
                {
                    currentPlayerTotalGems += kvp.Value;
                }
            }
            
            // 计算最终宝石数
            int finalGemCount = currentPlayerTotalGems + actualTotalQuantity - returnTotalQuantity;
            
            if (quantityText != null)
            {
                string displayText = $"Collect: {actualTotalQuantity}";
                if (returnTotalQuantity > 0)
                {
                    displayText += $" | Return: {returnTotalQuantity}";
                }
                displayText += $" | Final: {finalGemCount}/10";
                
                // 如果超过10个，显示警告
                if (finalGemCount > 10)
                {
                    displayText += " (EXCEEDS LIMIT!)";
                }
                
                quantityText.text = displayText;
            }
            
            // 更新按钮状态
            UpdateButtonStates();
            
            // 更新所有宝石的按钮状态
            foreach (var gemType in selectedGems.Keys)
            {
                UpdateGemButtonStates(gemType);
            }
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Quantity control updated - Collect: {actualTotalQuantity}, Return: {returnTotalQuantity}, Final: {finalGemCount}/10");
        }
        
        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            // 计算实际收集的宝石数量（不包括return的）
            int actualCollectQuantity = 0;
            int returnQuantity = 0;
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    actualCollectQuantity += kvp.Value;
                }
                else if (kvp.Value < 0)
                {
                    returnQuantity += Mathf.Abs(kvp.Value);
                }
            }
            
            // 计算玩家当前宝石总数
            int currentPlayerTotalGems = 0;
            if (currentPlayer != null && currentPlayer.gems != null)
            {
                foreach (var kvp in currentPlayer.gems)
                {
                    currentPlayerTotalGems += kvp.Value;
                }
            }
            
            // 计算最终宝石数
            int finalGemCount = currentPlayerTotalGems + actualCollectQuantity - returnQuantity;
            
            // 检查确认按钮状态：
            // 1. 必须有收集的宝石
            // 2. 最终宝石数不能超过10个
            bool canConfirm = actualCollectQuantity > 0 && finalGemCount <= 10;
            
            if (confirmButton != null)
            {
                confirmButton.interactable = canConfirm;
                
                if (enableDebugLog)
                {
                    Debug.Log($"GemCollectionPanel: Confirm button state updated - " +
                             $"Current gems: {currentPlayerTotalGems}, " +
                             $"Collect: {actualCollectQuantity}, " +
                             $"Return: {returnQuantity}, " +
                             $"Final: {finalGemCount}, " +
                             $"Can confirm: {canConfirm}");
                }
            }
        }
        
        /// <summary>
        /// 更新特定宝石的按钮状态
        /// </summary>
        private void UpdateGemButtonStates(string gemType)
        {
            if (!gemButtons.ContainsKey(gemType)) return;
            
            GameObject gemContainer = gemButtons[gemType];
            if (gemContainer == null) return;
            
            // 获取+/-按钮
            Button plusButton = gemContainer.transform.Find("PlusButton")?.GetComponent<Button>();
            Button minusButton = gemContainer.transform.Find("MinusButton")?.GetComponent<Button>();
            
            if (plusButton == null || minusButton == null) return;
            
            // 获取当前选择的数量
            int currentSelected = selectedGems.GetValueOrDefault(gemType, 0);
            
            // 获取银行中可用的宝石数量
            int bankAvailable = gameState.board.gems.GetValueOrDefault(gemType, 0);
            
            // 获取玩家拥有的宝石数量
            int playerOwned = currentPlayer.gems.GetValueOrDefault(gemType, 0);
            
            // 更新+按钮状态：不能超过银行可用数量
            bool canIncrease = currentSelected < bankAvailable;
            plusButton.interactable = canIncrease;
            
            // 更新-按钮状态：不能低于-玩家拥有的数量
            bool canDecrease = currentSelected > -playerOwned;
            minusButton.interactable = canDecrease;
            
            if (enableDebugLog)
            {
                Debug.Log($"GemCollectionPanel: {gemType} button states updated - " +
                         $"Current: {currentSelected}, " +
                         $"Bank: {bankAvailable}, " +
                         $"Player: {playerOwned}, " +
                         $"Can +: {canIncrease}, " +
                         $"Can -: {canDecrease}");
            }
        }
        

        
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OnConfirmClicked()
        {
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Confirm button clicked");
            
            // 检查基本状态
            if (gameState == null)
            {
                Debug.LogError("GemCollectionPanel: gameState is null!");
                return;
            }
            
            if (currentPlayer == null)
            {
                Debug.LogError("GemCollectionPanel: currentPlayer is null!");
                return;
            }
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Starting validation with {currentTotalQuantity} gems selected");
            
            if (ValidateSelection())
            {
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Validation passed, executing collection...");
                
                // 执行收集动作
                ExecuteCollection();
                
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Collection executed, hiding panel...");
                
                HidePanel();
            }
            else
            {
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Validation failed, keeping panel open");
            }
            // 如果验证失败，ValidateSelection已经显示了错误信息，UI保持打开状态
        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void OnCancelClicked()
        {
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Cancel button clicked");
            
            HidePanel();
        }
        
        /// <summary>
        /// 验证选择是否合法
        /// </summary>
        private bool ValidateSelection()
        {
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Starting validation, currentTotalQuantity: {currentTotalQuantity}");
            
            if (currentTotalQuantity == 0)
            {
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Validation failed - no gems selected");
                ShowInvalidMessage("Please select at least one gem!");
                return false;
            }
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Checking bank gem availability...");
            
            // 检查银行中是否有足够的宝石
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    if (!gameState.board.gems.ContainsKey(kvp.Key))
                    {
                        if (enableDebugLog)
                            Debug.Log($"GemCollectionPanel: Validation failed - no {kvp.Key} gems in bank");
                        ShowInvalidMessage($"No {kvp.Key} gems in bank!");
                        return false;
                    }
                    
                    if (gameState.board.gems[kvp.Key] < kvp.Value)
                    {
                        if (enableDebugLog)
                            Debug.Log($"GemCollectionPanel: Validation failed - insufficient {kvp.Key} gems");
                        ShowInvalidMessage($"Insufficient {kvp.Key} gems! Need {kvp.Value}, only {gameState.board.gems[kvp.Key]} available");
                        return false;
                    }
                }
            }
            
            // 检查collect后的总宝石数是否会超过10个
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Checking if total gems will exceed 10 after collection...");
            
            int currentPlayerTotalGems = 0;
            foreach (var kvp in currentPlayer.gems)
            {
                currentPlayerTotalGems += kvp.Value;
            }
            
            int collectTotal = 0;
            int returnTotal = 0;
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    collectTotal += kvp.Value;
                }
                else if (kvp.Value < 0)
                {
                    returnTotal += Mathf.Abs(kvp.Value);
                }
            }
            
            int finalGemCount = currentPlayerTotalGems + collectTotal - returnTotal;
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Current gems: {currentPlayerTotalGems}, Collect: {collectTotal}, Return: {returnTotal}, Final: {finalGemCount}");
            
            // 如果最终宝石数超过10个，强制要求return
            if (finalGemCount > 10)
            {
                int requiredReturn = finalGemCount - 10;
                ShowInvalidMessage($"After collection, you will have {finalGemCount} gems! You must return at least {requiredReturn} gems to stay under the limit of 10.");
                return false;
            }
            
            // 检查return的宝石数量是否超过玩家拥有的数量
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Checking return gem validation...");
            
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value < 0) // 负数表示return
                {
                    int returnAmount = Mathf.Abs(kvp.Value);
                    int playerOwned = currentPlayer.gems.GetValueOrDefault(kvp.Key, 0);
                    
                    if (returnAmount > playerOwned)
                    {
                        if (enableDebugLog)
                            Debug.Log($"GemCollectionPanel: Validation failed - cannot return {returnAmount} {kvp.Key} gems, player only owns {playerOwned}");
                        ShowInvalidMessage($"Cannot return {returnAmount} {kvp.Key} gems! You only own {playerOwned}");
                        return false;
                    }
                }
            }
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Bank and return validation passed, checking valid actions...");
            
            // 检查选择的gem是否在valid actions中（包括基本规则验证）
            if (!IsSelectionInValidActions())
            {
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Validation failed - not in valid actions");
                return false; // 错误信息已经在ValidateBasicRules中显示
            }
            
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Validation passed successfully");
            
            return true;
        }
        
        /// <summary>
        /// 检查选择的gem是否在valid actions中
        /// 实现新的逻辑：从valid actions中找到匹配的gem action，直接使用该action
        /// </summary>
        private bool IsSelectionInValidActions()
        {
            try
            {
                // 获取当前玩家的valid actions
                var gameRule = FindObjectOfType<SplendorGameRule>();
                if (gameRule == null)
        {
            if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: SplendorGameRule not found, skipping validation");
                    return true; // 如果找不到游戏规则，暂时允许所有动作
                }
                
                // 获取当前玩家的ID
                int currentPlayerId = -1;
                for (int i = 0; i < gameState.agents.Count; i++)
                {
                    if (gameState.agents[i] == currentPlayer)
                    {
                        currentPlayerId = i;
                        break;
                    }
                }
                
                if (currentPlayerId == -1)
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: Current player not found in game state");
                    return true; // 如果找不到玩家ID，暂时允许所有动作
                }
                
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Current player ID: {currentPlayerId}");
                
                // 首先进行基本规则验证
                if (!ValidateBasicRules())
                {
                    if (enableDebugLog)
                        Debug.Log("GemCollectionPanel: Basic rules validation failed");
                    return false;
                }
                
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Basic rules validation passed");
                
                // 获取valid actions
                var validActions = gameRule.GetLegalActions(gameState, currentPlayerId);
                if (validActions == null || validActions.Count == 0)
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: No valid actions found");
                    return true; // 如果没有合法动作，暂时允许所有动作
                }
                
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Found {validActions.Count} valid actions");
                
                // 新的逻辑：从valid actions中找到匹配的gem action
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Searching for matching gem action among {validActions.Count} valid actions");
                
                var matchedValidAction = FindMatchingGemAction(validActions);
                if (matchedValidAction != null)
                {
                    if (enableDebugLog)
                        Debug.Log("GemCollectionPanel: Found matching valid gem action!");
                    
                    // 存储找到的valid action，供后续使用
                    matchedValidGameAction = matchedValidAction;
                    return true;
                }
                
                if (enableDebugLog)
                    Debug.LogWarning("GemCollectionPanel: No matching gem action found in valid actions");
                return false;
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogError($"GemCollectionPanel: Error checking valid actions: {e.Message}");
                return true; // 如果出现错误，暂时允许所有动作
            }
        }
        
        /// <summary>
        /// 从valid actions中找到匹配当前选择的gem action
        /// </summary>
        private object FindMatchingGemAction(List<object> validActions)
        {
            try
            {
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Looking for gem action matching selected gems: {JsonUtility.ToJson(selectedGems)}");
                
                // 分离收集的宝石和归还的宝石
                var selectedCollectedGems = new Dictionary<string, int>();
                var selectedReturnedGems = new Dictionary<string, int>();
                
                foreach (var kvp in selectedGems)
                {
                    if (kvp.Value > 0)
                    {
                        selectedCollectedGems[kvp.Key] = kvp.Value;
                    }
                    else if (kvp.Value < 0)
                    {
                        selectedReturnedGems[kvp.Key] = Mathf.Abs(kvp.Value);
                    }
                }
                
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Selected collected gems: {JsonUtility.ToJson(selectedCollectedGems)}");
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Selected returned gems: {JsonUtility.ToJson(selectedReturnedGems)}");
                
                // 如果没有选择任何收集的宝石，返回null
                if (selectedCollectedGems.Count == 0)
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: No gems selected for collection (all quantities are 0 or negative)");
                    return null;
                }
                
                // 遍历所有valid actions，找到collect类型的action
                foreach (var action in validActions)
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: Checking action type: {action?.GetType()}");
                    
                    // 检查是否是Action对象
                    if (action is SplendorUnity.Core.Action actionObj)
                    {
                        if (enableDebugLog)
                            Debug.Log($"GemCollectionPanel: Action object - Type: {actionObj.Type}, CollectedGems: {JsonUtility.ToJson(actionObj.CollectedGems)}, ReturnedGems: {JsonUtility.ToJson(actionObj.ReturnedGems)}");
                        
                        // 检查是否是collect类型的action
                        if (actionObj.Type == "collect" && actionObj.CollectedGems != null)
                        {
                            // 比较CollectedGems是否匹配
                            bool collectedGemsMatch = CompareGemDictionaries(selectedCollectedGems, actionObj.CollectedGems, "Selected vs Valid CollectedGems");
                            
                            // 比较ReturnedGems是否匹配（如果玩家选择了return的宝石）
                            bool returnedGemsMatch = true;
                            if (selectedReturnedGems.Count > 0)
                            {
                                returnedGemsMatch = CompareGemDictionaries(selectedReturnedGems, actionObj.ReturnedGems, "Selected vs Valid ReturnedGems");
                            }
                            
                            if (collectedGemsMatch && returnedGemsMatch)
                            {
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Found matching collect action with returned gems!");
                                return action;
                            }
                        }
                    }
                    // 检查是否是Dictionary格式的action（向后兼容）
                    else if (action is Dictionary<string, object> actionDict)
                    {
                        if (enableDebugLog)
                            Debug.Log($"GemCollectionPanel: Dictionary action - Keys: {string.Join(", ", actionDict.Keys)}");
                        
                        // 检查是否包含gems键
                        if (actionDict.ContainsKey("gems") && actionDict["gems"] is Dictionary<string, int> actionGems)
                        {
                            if (enableDebugLog)
                                Debug.Log($"GemCollectionPanel: Dictionary action gems: {JsonUtility.ToJson(actionGems)}");
                            
                            // 比较gems是否匹配
                            if (CompareGemDictionaries(selectedCollectedGems, actionGems, "Selected vs Valid Gems"))
                            {
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Found matching dictionary action!");
                                return action;
                            }
                        }
                    }
                }
                
                if (enableDebugLog)
                    Debug.LogWarning("GemCollectionPanel: No matching gem action found");
                return null;
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogError($"GemCollectionPanel: Error finding matching gem action: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 验证基本收集规则
        /// </summary>
        private bool ValidateBasicRules()
        {
            if (enableDebugLog)
                Debug.Log($"ValidateBasicRules: 开始验证玩家 {currentPlayer?.id} 的基本规则");
            
            // 计算玩家当前拥有的宝石总数
            int playerGemCount = 0;
            if (currentPlayer.gems != null)
            {
                foreach (var gemCount in currentPlayer.gems.Values)
                {
                    playerGemCount += gemCount;
                }
            }
            
            if (enableDebugLog)
                Debug.Log($"ValidateBasicRules: 玩家 {currentPlayer?.id} 当前拥有 {playerGemCount} 个宝石");
            
            // 分离收集的宝石和归还的宝石
            int collectGemCount = 0;
            int returnGemCount = 0;
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    collectGemCount += kvp.Value;
                }
                else if (kvp.Value < 0)
                {
                    returnGemCount += Mathf.Abs(kvp.Value);
                }
            }
            
            if (enableDebugLog)
                Debug.Log($"ValidateBasicRules: 玩家选择收集 {collectGemCount} 个宝石，归还 {returnGemCount} 个宝石");
            
            // 规则1：收集的宝石总数量限制
            if (collectGemCount > MAX_TOTAL_GEMS)
            {
                ShowInvalidMessage($"Maximum {MAX_TOTAL_GEMS} gems can be collected!");
                return false;
            }
            
            // 规则2：同色宝石数量限制（根据Splendor规则，最多只能收集2个相同颜色）
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 2) // 修改为2，因为Splendor规则最多只能收集2个相同颜色
                {
                    ShowInvalidMessage("Maximum 2 gems of the same color allowed!");
                    return false;
                }
            }
            
            // 规则3：根据玩家当前宝石数量限制收集数量
            int maxCollectible = 0;
            if (playerGemCount <= 7)
            {
                maxCollectible = 3;
            }
            else if (playerGemCount == 8)
            {
                maxCollectible = 2;
            }
            else
            {
                maxCollectible = 1;
            }
            
            if (enableDebugLog)
                Debug.Log($"ValidateBasicRules: 玩家有 {playerGemCount} 个宝石，最多可收集 {maxCollectible} 个宝石");
            
            if (collectGemCount > maxCollectible)
            {
                ShowInvalidMessage($"With {playerGemCount} gems, you can only collect up to {maxCollectible} gems!");
                return false;
            }
            
            // 规则4：相同颜色宝石的特殊规则
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value == 2) // 如果选择了2个相同颜色的宝石
                {
                    string gemType = kvp.Key;
                    // 检查银行中是否有足够的宝石（需要≥4个）
                    if (!gameState.board.gems.ContainsKey(gemType) || gameState.board.gems[gemType] < 4)
                    {
                        ShowInvalidMessage($"Cannot collect 2 {gemType} gems! Need at least 4 in bank.");
                        return false;
                    }
                }
            }
            
            // 规则5：如果收集多个不同颜色，每个颜色只能1个
            if (collectGemCount > 1)
            {
                int multiColorCount = 0;
                foreach (var kvp in selectedGems)
                {
                    if (kvp.Value > 0)
                    {
                        multiColorCount++;
                    }
                }
                
                if (multiColorCount > 1) // 多个不同颜色
                {
                    foreach (var kvp in selectedGems)
                    {
                        if (kvp.Value > 1) // 任何颜色都不能超过1个
                        {
                            ShowInvalidMessage("When collecting multiple colors, each color can only have 1 gem!");
                            return false;
                        }
                    }
                }
            }
            
            // 规则6：归还的宝石数量不能超过玩家拥有的数量
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value < 0) // 负数表示归还
                {
                    int returnAmount = Mathf.Abs(kvp.Value);
                    int playerOwned = currentPlayer.gems.GetValueOrDefault(kvp.Key, 0);
                    
                    if (returnAmount > playerOwned)
                    {
                        ShowInvalidMessage($"Cannot return {returnAmount} {kvp.Key} gems! You only own {playerOwned}");
                        return false;
                    }
                }
            }
            
            // 规则7：检查collect后的总宝石数是否会超过10个
            int finalGemCount = playerGemCount + collectGemCount - returnGemCount;
            if (enableDebugLog)
                Debug.Log($"ValidateBasicRules: 最终宝石数: {finalGemCount} (当前: {playerGemCount} + 收集: {collectGemCount} - 归还: {returnGemCount})");
            
            if (finalGemCount > 10)
            {
                int requiredReturn = finalGemCount - 10;
                ShowInvalidMessage($"After collection, you will have {finalGemCount} gems! You must return at least {requiredReturn} gems to stay under the limit of 10.");
                return false;
            }
            
            if (enableDebugLog)
                Debug.Log("ValidateBasicRules: 基本规则验证通过");
            
            return true;
        }
        
        /// <summary>
        /// 创建gem收集action
        /// </summary>
        private object CreateGemCollectionAction()
        {
            // 分离收集的宝石和归还的宝石
            var collectedGems = new Dictionary<string, int>();
            var returnedGems = new Dictionary<string, int>();
            
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    // 正数表示收集的宝石
                    collectedGems[kvp.Key] = kvp.Value;
                }
                else if (kvp.Value < 0)
                {
                    // 负数表示归还的宝石
                    returnedGems[kvp.Key] = Mathf.Abs(kvp.Value);
                }
            }
            
            // 创建收集gem的action，使用正确的Action类格式
            var action = new SplendorUnity.Core.Action();
            action.Type = "collect";
            action.CollectedGems = collectedGems;
            action.ReturnedGems = returnedGems;
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Created Action object - Type: {action.Type}, CollectedGems: {JsonUtility.ToJson(action.CollectedGems)}, ReturnedGems: {JsonUtility.ToJson(action.ReturnedGems)}");
            
            return action;
        }
        
        /// <summary>
        /// 检查两个action是否相同
        /// </summary>
        private bool IsSameAction(object action1, object action2)
        {
            try
            {
                if (action1 == null || action2 == null) 
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: One of the actions is null");
                    return false;
                }
                
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Comparing actions - Type1: {action1.GetType()}, Type2: {action2.GetType()}");
                
                // 如果两个都是Action对象，直接比较
                if (action1 is SplendorUnity.Core.Action actionObj1 && action2 is SplendorUnity.Core.Action actionObj2)
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: Both actions are Action objects - Type1: {actionObj1.Type}, Type2: {actionObj2.Type}");
                    
                    // 检查动作类型
                    if (actionObj1.Type != actionObj2.Type)
                    {
                        if (enableDebugLog)
                            Debug.LogWarning($"GemCollectionPanel: Action types don't match - Type1: {actionObj1.Type}, Type2: {actionObj2.Type}");
                        return false;
                    }
                    
                    // 检查收集的宝石
                    if (!CompareGemDictionaries(actionObj1.CollectedGems, actionObj2.CollectedGems, "CollectedGems"))
                    {
                        return false;
                    }
                    
                    // 检查归还的宝石
                    if (!CompareGemDictionaries(actionObj1.ReturnedGems, actionObj2.ReturnedGems, "ReturnedGems"))
                    {
                        return false;
                    }
                    
                    if (enableDebugLog)
                        Debug.Log("GemCollectionPanel: Action objects comparison successful - actions match!");
                    return true;
                }
                
                // 如果action是Dictionary，比较内容（保持向后兼容）
                if (action1 is Dictionary<string, object> dict1 && action2 is Dictionary<string, object> dict2)
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: Both actions are dictionaries - Count1: {dict1.Count}, Count2: {dict2.Count}");
                    
                    if (dict1.Count != dict2.Count) 
                    {
                        if (enableDebugLog)
                            Debug.LogWarning("GemCollectionPanel: Dictionary counts don't match");
                        return false;
                    }
                    
                    foreach (var kvp in dict1)
                    {
                        if (!dict2.ContainsKey(kvp.Key)) 
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: Key '{kvp.Key}' not found in second action");
                            return false;
                        }
                        
                        if (kvp.Key == "gems")
                        {
                            // 比较gem选择
                            if (kvp.Value is Dictionary<string, int> gems1 && dict2[kvp.Key] is Dictionary<string, int> gems2)
                            {
                                if (enableDebugLog)
                                    Debug.Log($"GemCollectionPanel: Comparing gems - Count1: {gems1.Count}, Count2: {gems2.Count}");
                                
                                if (gems1.Count != gems2.Count) 
                                {
                                    if (enableDebugLog)
                                        Debug.LogWarning("GemCollectionPanel: Gem counts don't match");
                                    return false;
                                }
                                
                                foreach (var gemKvp in gems1)
                                {
                                    if (!gems2.ContainsKey(gemKvp.Key) || gems2[gemKvp.Key] != gemKvp.Value)
                                    {
                                        if (enableDebugLog)
                                            Debug.LogWarning($"GemCollectionPanel: Gem mismatch for {gemKvp.Key} - Value1: {gemKvp.Value}, Value2: {(gems2.ContainsKey(gemKvp.Key) ? gems2[kvp.Key].ToString() : "missing")}");
                                        return false;
                                    }
                                }
                                
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Gems comparison successful");
                            }
                            else
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: Gems values are not dictionaries - Type1: {kvp.Value?.GetType()}, Type2: {dict2[kvp.Key]?.GetType()}");
                                return false;
                            }
                        }
                        else if (!kvp.Value.Equals(dict2[kvp.Key]))
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: Value mismatch for key '{kvp.Key}' - Value1: {kvp.Value}, Value2: {dict2[kvp.Key]}");
                            return false;
                        }
                    }
                    
                    if (enableDebugLog)
                        Debug.Log("GemCollectionPanel: Dictionary comparison successful - actions match!");
                    return true;
                }
                
                // 如果action是其他类型，直接比较
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Actions are not dictionaries or Action objects, using direct comparison");
                return action1.Equals(action2);
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogError($"GemCollectionPanel: Error comparing actions: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 比较两个宝石字典是否相等
        /// </summary>
        private bool CompareGemDictionaries(Dictionary<string, int> gems1, Dictionary<string, int> gems2, string dictName)
        {
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: Comparing {dictName} - Gems1: {JsonUtility.ToJson(gems1)}, Gems2: {JsonUtility.ToJson(gems2)}");
            
            if (gems1 == null && gems2 == null)
            {
                if (enableDebugLog)
                    Debug.Log($"GemCollectionPanel: Both {dictName} are null");
                return true;
            }
            
            if (gems1 == null || gems2 == null)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"GemCollectionPanel: One of {dictName} is null - Gems1: {gems1}, Gems2: {gems2}");
                return false;
            }
            
            if (gems1.Count != gems2.Count)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"GemCollectionPanel: {dictName} counts don't match - Count1: {gems1.Count}, Count2: {gems2.Count}");
                return false;
            }
            
            // 检查每个宝石类型和数量是否匹配
            foreach (var kvp in gems1)
            {
                if (!gems2.ContainsKey(kvp.Key))
                {
                    if (enableDebugLog)
                        Debug.LogWarning($"GemCollectionPanel: {dictName} - Key '{kvp.Key}' missing in gems2");
                    return false;
                }
                
                if (gems2[kvp.Key] != kvp.Value)
                {
                    if (enableDebugLog)
                        Debug.LogWarning($"GemCollectionPanel: {dictName} - Value mismatch for '{kvp.Key}': {kvp.Value} vs {gems2[kvp.Key]}");
                    return false;
                }
            }
            
            if (enableDebugLog)
                Debug.Log($"GemCollectionPanel: {dictName} comparison successful - all gems match!");
            return true;
        }
        
        /// <summary>
        /// 执行收集动作
        /// </summary>
        private void ExecuteCollection()
        {
            if (enableDebugLog)
                Debug.Log("GemCollectionPanel: Executing collection action");
            
            try
            {
                // 使用找到的匹配的valid action，而不是创建新的
                if (matchedValidGameAction != null)
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: Using matched valid action: {JsonUtility.ToJson(matchedValidGameAction)}");
                    
                    // 通知游戏管理器玩家选择了Action
                    NotifyGameManagerOfAction(matchedValidGameAction);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: No matched valid action found, creating fallback action");
                    
                    // 如果没有找到匹配的action，创建fallback action
                    var collectionAction = CreateGemCollectionAction();
                    NotifyGameManagerOfAction(collectionAction);
                }
                
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Action notification sent to GameManager");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GemCollectionPanel: Error executing collection action: {e.Message}");
            }
        }
        
        /// <summary>
        /// 通知游戏管理器玩家选择了Action
        /// </summary>
        private void NotifyGameManagerOfAction(object selectedAction)
        {
            try
            {
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Starting to notify GameManager of action");
                
                // 查找游戏管理器
                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    if (enableDebugLog)
                        Debug.Log($"GemCollectionPanel: Found GameManager, notifying of selected action: {JsonUtility.ToJson(selectedAction)}");
                    
                    // 尝试多种方法来通知GameManager玩家选择了Action
                    bool actionNotified = false;
                    
                    // 方法1：尝试调用OnPlayerActionSelected
                    var onPlayerActionSelectedMethod = gameManager.GetType().GetMethod("OnPlayerActionSelected");
                    if (onPlayerActionSelectedMethod != null)
                    {
                        try
                        {
                            onPlayerActionSelectedMethod.Invoke(gameManager, new object[] { selectedAction });
                            if (enableDebugLog)
                                Debug.Log("GemCollectionPanel: Successfully called OnPlayerActionSelected");
                            actionNotified = true;
                        }
                        catch (System.Exception e)
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: OnPlayerActionSelected failed: {e.Message}");
                        }
                    }
                    
                    // 方法2：尝试调用PlayerActionSelected
                    if (!actionNotified)
                    {
                        var playerActionSelectedMethod = gameManager.GetType().GetMethod("PlayerActionSelected");
                        if (playerActionSelectedMethod != null)
                        {
                            try
                            {
                                playerActionSelectedMethod.Invoke(gameManager, new object[] { selectedAction });
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Successfully called PlayerActionSelected");
                                actionNotified = true;
                            }
                            catch (System.Exception e)
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: PlayerActionSelected failed: {e.Message}");
                            }
                        }
                    }
                    
                    // 方法3：尝试调用SetPlayerAction
                    if (!actionNotified)
                    {
                        var setPlayerActionMethod = gameManager.GetType().GetMethod("SetPlayerAction");
                        if (setPlayerActionMethod != null)
                        {
                            try
                            {
                                setPlayerActionMethod.Invoke(gameManager, new object[] { selectedAction });
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Successfully called SetPlayerAction");
                                actionNotified = true;
                            }
                            catch (System.Exception e)
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: SetPlayerAction failed: {e.Message}");
                            }
                        }
                    }
                    
                    // 方法4：尝试调用ContinueToNextTurn（作为备选方案）
                    if (!actionNotified)
                    {
                        var continueToNextTurnMethod = gameManager.GetType().GetMethod("ContinueToNextTurn");
                        if (continueToNextTurnMethod != null)
                        {
                            try
                            {
                                continueToNextTurnMethod.Invoke(gameManager, null);
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Fallback: called ContinueToNextTurn");
                                actionNotified = true;
                            }
                            catch (System.Exception e)
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: ContinueToNextTurn fallback failed: {e.Message}");
                            }
                        }
                    }
                    
                    if (!actionNotified)
                    {
                        if (enableDebugLog)
                            Debug.LogWarning("GemCollectionPanel: No suitable method found to notify GameManager of player action");
                    }
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: GameManager not found, cannot notify of player action");
                }
                
                // 关键：通知GameManager玩家已经输入了，让游戏继续
                NotifyGameManagerPlayerHasInput();
                
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogError($"GemCollectionPanel: Error notifying GameManager of action: {e.Message}");
            }
        }
        
        /// <summary>
        /// 通知GameManager玩家已经输入了，让游戏继续
        /// </summary>
        private void NotifyGameManagerPlayerHasInput()
        {
            try
            {
                if (enableDebugLog)
                    Debug.Log("GemCollectionPanel: Notifying GameManager that player has input");
                
                // 查找GameManager
                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    // 方法1：使用新的SetPlayerAction方法
                    var setPlayerActionMethod = gameManager.GetType().GetMethod("SetPlayerAction");
                    if (setPlayerActionMethod != null)
                    {
                        try
                        {
                            setPlayerActionMethod.Invoke(gameManager, new object[] { matchedValidGameAction });
                            if (enableDebugLog)
                                Debug.Log("GemCollectionPanel: Successfully called SetPlayerAction with matched action");
                            return;
                        }
                        catch (System.Exception e)
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: SetPlayerAction failed: {e.Message}");
                        }
                    }
                    
                    // 方法2：尝试设置玩家输入标志
                    var setPlayerInputFlagMethod = gameManager.GetType().GetMethod("SetPlayerInputFlag");
                    if (setPlayerInputFlagMethod != null)
                    {
                        try
                        {
                            setPlayerInputFlagMethod.Invoke(gameManager, new object[] { true });
                            if (enableDebugLog)
                                Debug.Log("GemCollectionPanel: Successfully called SetPlayerInputFlag(true)");
                            return;
                        }
                        catch (System.Exception e)
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: SetPlayerInputFlag failed: {e.Message}");
                        }
                    }
                    
                    // 方法3：尝试直接调用ContinueToNextTurn
                    var continueToNextTurnMethod = gameManager.GetType().GetMethod("ContinueToNextTurn");
                    if (continueToNextTurnMethod != null)
                    {
                        try
                        {
                            continueToNextTurnMethod.Invoke(gameManager, null);
                            if (enableDebugLog)
                                Debug.Log("GemCollectionPanel: Successfully called ContinueToNextTurn");
                            return;
                        }
                        catch (System.Exception e)
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: ContinueToNextTurn failed: {e.Message}");
                        }
                    }
                    
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: Could not find any method to notify GameManager of player input");
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: GameManager not found, cannot notify of player input");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogError($"GemCollectionPanel: Error notifying GameManager of player input: {e.Message}");
            }
        }
        
        /// <summary>
        /// 通知游戏管理器进入下一回合（保留作为备选方法）
        /// </summary>
        private void NotifyGameManager()
        {
            try
            {
                // 查找游戏管理器
                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    if (enableDebugLog)
                        Debug.Log("GemCollectionPanel: Notifying GameManager to continue to next turn");
                    
                    // 尝试多种方法来推进游戏
                    bool gameContinued = false;
                    
                    // 方法1：尝试调用ContinueToNextTurn
                    var continueToNextTurnMethod = gameManager.GetType().GetMethod("ContinueToNextTurn");
                    if (continueToNextTurnMethod != null)
                    {
                        try
                        {
                            continueToNextTurnMethod.Invoke(gameManager, null);
                            if (enableDebugLog)
                                Debug.Log("GemCollectionPanel: Successfully called ContinueToNextTurn");
                            gameContinued = true;
                        }
                        catch (System.Exception e)
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"GemCollectionPanel: ContinueToNextTurn failed: {e.Message}");
                        }
                    }
                    
                    // 方法2：尝试调用ContinueGame
                    if (!gameContinued)
                    {
                        var continueGameMethod = gameManager.GetType().GetMethod("ContinueGame");
                        if (continueGameMethod != null)
                        {
                            try
                            {
                                continueGameMethod.Invoke(gameManager, null);
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Successfully called ContinueGame");
                                gameContinued = true;
                            }
                            catch (System.Exception e)
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: ContinueGame failed: {e.Message}");
                            }
                        }
                    }
                    
                    // 方法3：尝试调用PlayerActionCompleted
                    if (!gameContinued)
                    {
                        var playerActionCompletedMethod = gameManager.GetType().GetMethod("PlayerActionCompleted");
                        if (playerActionCompletedMethod != null)
                        {
                            try
                            {
                                playerActionCompletedMethod.Invoke(gameManager, null);
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Successfully called PlayerActionCompleted");
                                gameContinued = true;
                            }
                            catch (System.Exception e)
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: PlayerActionCompleted failed: {e.Message}");
                            }
                        }
                    }
                    
                    // 方法4：尝试调用OnPlayerActionCompleted
                    if (!gameContinued)
                    {
                        var onPlayerActionCompletedMethod = gameManager.GetType().GetMethod("OnPlayerActionCompleted");
                        if (onPlayerActionCompletedMethod != null)
                        {
                            try
                            {
                                onPlayerActionCompletedMethod.Invoke(gameManager, null);
                                if (enableDebugLog)
                                    Debug.Log("GemCollectionPanel: Successfully called OnPlayerActionCompleted");
                                gameContinued = true;
                            }
                            catch (System.Exception e)
                            {
                                if (enableDebugLog)
                                    Debug.LogWarning($"GemCollectionPanel: OnPlayerActionCompleted failed: {e.Message}");
                            }
                        }
                    }
                    
                    if (!gameContinued)
                    {
                        if (enableDebugLog)
                            Debug.LogWarning("GemCollectionPanel: No suitable method found to continue the game");
                    }
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("GemCollectionPanel: GameManager not found, cannot continue to next turn");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                    Debug.LogError($"GemCollectionPanel: Error notifying GameManager: {e.Message}");
            }
        }
        
        /// <summary>
        /// 显示无效提示
        /// </summary>
        private void ShowInvalidMessage(string message)
        {
            if (invalidMessageText != null)
            {
                invalidMessageText.text = message;
                invalidMessageText.gameObject.SetActive(true);
                
                // 5秒后自动隐藏
                Invoke(nameof(HideInvalidMessage), 5f);
            }
            else
            {
                // 如果没有invalidMessageText，直接显示在Console中
                Debug.LogWarning($"GemCollectionPanel: {message}");
            }
            
            if (enableDebugLog)
                Debug.LogWarning($"GemCollectionPanel: Invalid selection - {message}");
        }
        
        /// <summary>
        /// 隐藏无效提示
        /// </summary>
        private void HideInvalidMessage()
        {
            if (invalidMessageText != null)
                invalidMessageText.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 重置选择
        /// </summary>
        private void ResetSelection()
        {
            // 创建键的副本，避免在遍历时修改集合
            var gemTypes = new List<string>(selectedGems.Keys);
            foreach (string gemType in gemTypes)
            {
                selectedGems[gemType] = 0;
                // 更新显示的数量
                UpdateGemSelectionCount(gemType);
            }
            currentTotalQuantity = 0;
            
            // 重置匹配的valid action
            matchedValidGameAction = null;
            
            UpdateQuantityControl();
            HideInvalidMessage();
        }
        
        /// <summary>
        /// 获取选择状态
        /// </summary>
        [ContextMenu("Get Selection Status")]
        public string GetSelectionStatus()
        {
            string status = "=== Gem Collection Panel Status ===\n";
            status += $"Current Total Quantity: {currentTotalQuantity}\n";
            status += $"Max Total Gems: {MAX_TOTAL_GEMS}\n";
            status += $"Max Same Color: {MAX_SAME_COLOR}\n";
            
            status += "\nSelected Gems:\n";
            foreach (var kvp in selectedGems)
            {
                status += $"{kvp.Key}: {kvp.Value}\n";
            }
            
            status += $"\nValidation: {(ValidateSelection() ? "Valid" : "Invalid")}\n";
            
            return status;
        }
        
        [ContextMenu("测试Action匹配逻辑")]
        public void TestActionMatching()
        {
            if (enableDebugLog)
                Debug.Log("=== 测试Action匹配逻辑 ===");
            
            try
            {
                // 模拟选择一些宝石
                selectedGems["red"] = 2;
                selectedGems["blue"] = 1;
                
                if (enableDebugLog)
                    Debug.Log($"模拟选择的宝石: {JsonUtility.ToJson(selectedGems)}");
                
                // 测试验证
                bool isValid = ValidateSelection();
                
                if (enableDebugLog)
                    Debug.Log($"验证结果: {isValid}");
                
                if (matchedValidGameAction != null)
                {
                    if (enableDebugLog)
                        Debug.Log($"找到匹配的valid action: {JsonUtility.ToJson(matchedValidGameAction)}");
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("没有找到匹配的valid action");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"测试过程中出现错误: {e.Message}");
            }
        }
        
        [ContextMenu("检查玩家宝石状态")]
        public void CheckPlayerGemStatus()
        {
            if (enableDebugLog)
                Debug.Log("=== 检查玩家宝石状态 ===");
            
            try
            {
                if (currentPlayer == null)
                {
                    Debug.LogWarning("currentPlayer 为 null");
                    return;
                }
                
                if (currentPlayer.gems == null)
                {
                    Debug.LogWarning("currentPlayer.gems 为 null");
                    return;
                }
                
                Debug.Log($"玩家ID: {currentPlayer.id}");
                Debug.Log($"玩家分数: {currentPlayer.score}");
                
                int totalGems = 0;
                Debug.Log("玩家宝石详情:");
                foreach (var kvp in currentPlayer.gems)
                {
                    Debug.Log($"  {kvp.Key}: {kvp.Value}");
                    totalGems += kvp.Value;
                }
                
                Debug.Log($"玩家总宝石数: {totalGems}");
                
                // 测试验证逻辑
                if (enableDebugLog)
                    Debug.Log("开始测试验证逻辑...");
                
                bool isValid = ValidateBasicRules();
                Debug.Log($"验证结果: {isValid}");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"检查过程中出现错误: {e.Message}");
            }
        }
    }
}
