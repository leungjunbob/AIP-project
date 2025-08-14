using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 宝石收集面板UI设置助手 - 帮助快速创建和配置GemCollectionPanel的UI结构
    /// </summary>
    public class GemCollectionPanelSetup : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private bool _createUIOnStart = false; // 禁用自动创建，改为按需创建
        public bool createUIOnStart => _createUIOnStart; // 只读属性，确保默认值为false
        public Vector2 panelSize = new Vector2(600, 400);
        public Vector2 panelPosition = new Vector2(0, 0);
        
        [Header("Gem Selection Settings")]
        public Vector2 gemButtonSize = new Vector2(60, 60);
        public float gemButtonSpacing = 80f;
        
        [Header("Text Settings")]
        public int titleFontSize = 24;
        public int quantityFontSize = 20;
        public int buttonFontSize = 18;
        
        [Header("Debug")]
        public bool enableDebugLog = true;
        
        private Canvas canvas;
        private GemCollectionPanel gemCollectionPanel;
        
        private void Start()
        {
            // 强制禁用自动创建，防止意外启动
            if (_createUIOnStart)
            {
                _createUIOnStart = false;
            }
            
        }
        
        /// <summary>
        /// 创建宝石收集面板的UI结构
        /// </summary>
        [ContextMenu("Create GemCollectionPanel UI")]
        public void CreateGemCollectionPanelUI()
        {
            try
            {
                
                // 检查是否已经存在面板，如果存在则先销毁
                if (gemCollectionPanel != null)
                {
                    DestroyGemCollectionPanelUI();
                }
                
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
                
                // Create main panel container
                GameObject panelGO = new GameObject("GemCollectionPanel");
                if (panelGO == null)
                {
                    return;
                }
                
                panelGO.transform.SetParent(canvas.transform, false);
                
                // Add RectTransform component first
                RectTransform rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.sizeDelta = panelSize;
                rectTransform.anchoredPosition = panelPosition;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f); // Center
                rectTransform.pivot = new Vector2(0.5f, 0.5f); // Center
                
                
                // 先创建所有UI组件
                CreatePanelBackground(panelGO);
                
                CreateTitle(panelGO);
                
                CreateGemSelectionArea(panelGO);
                
                CreateQuantityControlArea(panelGO);
                
                
                CreateInvalidMessage(panelGO);
                
                // 最后添加GemCollectionPanel组件
                gemCollectionPanel = panelGO.AddComponent<GemCollectionPanel>();
                
                // 配置GemCollectionPanel
                ConfigureGemCollectionPanel();
                
            }
            catch (System.Exception e)
            {
            }
        }
        
        /// <summary>
        /// 创建面板背景
        /// </summary>
        private void CreatePanelBackground(GameObject parent)
        {
            GameObject backgroundGO = new GameObject("Background");
            backgroundGO.transform.SetParent(parent.transform, false);
            
            // Add Image component
            Image image = backgroundGO.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // 半透明黑色
            
            // Set RectTransform
            RectTransform rectTransform = backgroundGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = panelSize;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle(GameObject parent)
        {
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(parent.transform, false);
            
            // 尝试添加TextMeshProUGUI组件，如果失败则使用普通Text
            TextMeshProUGUI tmpText = null;
            try
            {
                tmpText = titleGO.AddComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = "Collect Gems";
                    tmpText.fontSize = titleFontSize;
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontStyle = FontStyles.Bold;
                }
            }
            catch (System.Exception e)
            {
            }
            
            // 如果TextMeshProUGUI失败，尝试使用普通Text
            if (tmpText == null)
            {
                try
                {
                    UnityEngine.UI.Text text = titleGO.AddComponent<UnityEngine.UI.Text>();
                    if (text != null)
                    {
                        text.text = "Collect Gems";
                        text.fontSize = titleFontSize;
                        text.color = Color.white;
                        text.alignment = TextAnchor.MiddleCenter;
                        text.fontStyle = FontStyle.Bold;
                    }
                }
                catch (System.Exception e2)
                {
                    return;
                }
            }
            
            // Set RectTransform
            RectTransform rectTransform = titleGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 40);
            rectTransform.anchoredPosition = new Vector2(0, 150);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        /// <summary>
        /// 创建宝石选择区域
        /// </summary>
        private void CreateGemSelectionArea(GameObject parent)
        {
            GameObject selectionGO = new GameObject("GemSelectionArea");
            selectionGO.transform.SetParent(parent.transform, false);
            
            // Add RectTransform
            RectTransform rectTransform = selectionGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 100);
            rectTransform.anchoredPosition = new Vector2(0, 50);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Create gem selection container
            GameObject containerGO = new GameObject("GemSelectionContainer");
            containerGO.transform.SetParent(selectionGO.transform, false);
            
            RectTransform containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(500, 100);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Store reference for later assignment
            // We'll assign this to gemCollectionPanel after it's created
            
        }
        
        /// <summary>
        /// 创建数量控制区域
        /// </summary>
        private void CreateQuantityControlArea(GameObject parent)
        {
            if (parent == null)
            {
                return;
            }
            
            GameObject controlGO = new GameObject("QuantityControlArea");
            if (controlGO == null)
            {
                return;
            }
            
            controlGO.transform.SetParent(parent.transform, false);
            
            // Add RectTransform
            RectTransform rectTransform = controlGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 80);
            rectTransform.anchoredPosition = new Vector2(0, -20);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Create quantity text
            GameObject quantityGO = new GameObject("QuantityText");
            if (quantityGO == null)
            {
                return;
            }
            
            quantityGO.transform.SetParent(controlGO.transform, false);
            
            // 尝试添加TextMeshProUGUI组件，如果失败则使用普通Text
            TextMeshProUGUI quantityText = null;
            try
            {
                quantityText = quantityGO.AddComponent<TextMeshProUGUI>();
                if (quantityText != null)
                {
                    quantityText.text = "Total: 0";
                    quantityText.fontSize = quantityFontSize;
                    quantityText.color = Color.white;
                    quantityText.alignment = TextAlignmentOptions.Center;
                }
            }
            catch (System.Exception e)
            {
            }
            
            // 如果TextMeshProUGUI失败，尝试使用普通Text
            if (quantityText == null)
            {
                try
                {
                    UnityEngine.UI.Text text = quantityGO.AddComponent<UnityEngine.UI.Text>();
                    if (text != null)
                    {
                        text.text = "Total: 0";
                        text.fontSize = quantityFontSize;
                        text.color = Color.white;
                        text.alignment = TextAnchor.MiddleCenter;
                    }
                }
                catch (System.Exception e2)
                {
                    return;
                }
            }
            
            RectTransform quantityRect = quantityGO.GetComponent<RectTransform>();
            if (quantityRect == null)
            {
                return;
            }
            quantityRect.sizeDelta = new Vector2(200, 30);
            quantityRect.anchoredPosition = new Vector2(0, 20);
            quantityRect.anchorMin = new Vector2(0.5f, 0.5f);
            quantityRect.anchorMax = new Vector2(0.5f, 0.5f);
            quantityRect.pivot = new Vector2(0.5f, 0.5f);
            

        }
        

        
        /// <summary>
        /// 创建无效提示消息
        /// </summary>
        private void CreateInvalidMessage(GameObject parent)
        {
            GameObject messageGO = new GameObject("InvalidMessage");
            messageGO.transform.SetParent(parent.transform, false);
            
            // Add RectTransform
            RectTransform rectTransform = messageGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 40);
            rectTransform.anchoredPosition = new Vector2(0, -150);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // 尝试添加TextMeshProUGUI组件，如果失败则使用普通Text
            TextMeshProUGUI messageText = null;
            try
            {
                messageText = messageGO.AddComponent<TextMeshProUGUI>();
                if (messageText != null)
                {
                    messageText.text = "";
                    messageText.fontSize = 18;
                    messageText.color = Color.red;
                    messageText.alignment = TextAlignmentOptions.Center;
                    messageText.fontStyle = FontStyles.Bold;
                }
            }
            catch (System.Exception e)
            {
            }
            
            // 如果TextMeshProUGUI失败，尝试使用普通Text
            if (messageText == null)
            {
                try
                {
                    UnityEngine.UI.Text text = messageGO.AddComponent<UnityEngine.UI.Text>();
                    if (text != null)
                    {
                        text.text = "";
                        text.fontSize = 18;
                        text.color = Color.red;
                        text.alignment = TextAnchor.MiddleCenter;
                        text.fontStyle = FontStyle.Bold;
                    }
                }
                catch (System.Exception e2)
                {
                    return;
                }
            }
            
            // Initially hide the message
            messageGO.SetActive(false);
            
            // Store reference for later assignment
            // We'll assign this to gemCollectionPanel after it's created
        
        }
        
        /// <summary>
        /// 配置GemCollectionPanel
        /// </summary>
        private void ConfigureGemCollectionPanel()
        {
            if (gemCollectionPanel != null)
            {
                // 设置面板容器
                gemCollectionPanel.panelContainer = gemCollectionPanel.gameObject;
                
                // 查找并分配UI组件
                AssignUIComponents();
            }
        }
        
        /// <summary>
        /// 分配UI组件到GemCollectionPanel
        /// </summary>
        private void AssignUIComponents()
        {
            if (gemCollectionPanel == null) return;
            
            // 查找宝石选择容器
            Transform gemSelectionContainer = gemCollectionPanel.transform.Find("GemSelectionArea/GemSelectionContainer");
            if (gemSelectionContainer != null)
            {
                gemCollectionPanel.gemSelectionContainer = gemSelectionContainer;
            }
            
            // 查找数量控制组件
            Transform quantityControlArea = gemCollectionPanel.transform.Find("QuantityControlArea");
            if (quantityControlArea != null)
            {
                TextMeshProUGUI quantityText = quantityControlArea.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
                
                if (quantityText != null) gemCollectionPanel.quantityText = quantityText;
            }
            
            // 查找操作按钮
            Transform actionButtons = gemCollectionPanel.transform.Find("ActionButtons");
            if (actionButtons != null)
            {
                Button confirmButton = actionButtons.Find("ConfirmButton")?.GetComponent<Button>();
                Button cancelButton = actionButtons.Find("CancelButton")?.GetComponent<Button>();
                
                if (confirmButton != null) gemCollectionPanel.confirmButton = confirmButton;
                if (cancelButton != null) gemCollectionPanel.cancelButton = cancelButton;
            }
            
            // 查找无效提示消息
            Transform invalidMessage = gemCollectionPanel.transform.Find("InvalidMessage");
            if (invalidMessage != null)
            {
                TextMeshProUGUI messageText = invalidMessage.GetComponent<TextMeshProUGUI>();
                if (messageText != null) gemCollectionPanel.invalidMessageText = messageText;
            }

        }
        
        /// <summary>
        /// 销毁宝石收集面板UI
        /// </summary>
        [ContextMenu("Destroy GemCollectionPanel UI")]
        public void DestroyGemCollectionPanelUI()
        {
            if (gemCollectionPanel != null)
            {
                DestroyImmediate(gemCollectionPanel.gameObject);
                gemCollectionPanel = null;
            }
        }
        
        /// <summary>
        /// 重新创建UI
        /// </summary>
        [ContextMenu("Recreate UI")]
        public void RecreateUI()
        {
            DestroyGemCollectionPanelUI();
            CreateGemCollectionPanelUI();
        }
        
        /// <summary>
        /// 获取设置状态
        /// </summary>
        [ContextMenu("Get Setup Status")]
        public string GetSetupStatus()
        {
            string status = "=== GemCollectionPanelSetup Status ===\n";
            status += $"Canvas: {(canvas != null ? "Found" : "Not Found")}\n";
            status += $"GemCollectionPanel: {(gemCollectionPanel != null ? "Created" : "Not Created")}\n";
            status += $"Panel Size: {panelSize}\n";
            status += $"Panel Position: {panelPosition}\n";
            status += $"Gem Button Size: {gemButtonSize}\n";
            status += $"Gem Button Spacing: {gemButtonSpacing}";
            
            return status;
        }
    }
}
