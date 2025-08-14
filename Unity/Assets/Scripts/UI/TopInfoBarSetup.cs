using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SplendorUnity.UI
{
    /// <summary>
    /// TopInfoBar Setup Helper - Helps quickly create and configure TopInfoBar UI structure
    /// </summary>
    public class TopInfoBarSetup : MonoBehaviour
    {
        [Header("UI Settings")]
        public bool createUIOnStart = false; // 改为false，避免自动创建
        public Vector2 canvasSize = new Vector2(1920, 1080);
        public Vector2 infoBarSize = new Vector2(1920, 100); // Increased size for larger text
        public Vector2 infoBarPosition = new Vector2(0, 440); // Position relative to Canvas center
        
        [Header("Text Settings")]
        public string player1Name = "Player 1";
        public string player2Name = "Player 2";
        public Color currentPlayerColor = Color.yellow;
        public Color normalPlayerColor = Color.white;
        
        [Header("Countdown Settings")]
        public Color normalTimeColor = Color.white;
        public Color warningTimeColor = Color.yellow;
        public Color dangerTimeColor = Color.red;
        
        private Canvas canvas;
        private TopInfoBar topInfoBar;
        
        private void Start()
        {

        }
        
        /// <summary>
        /// Create TopInfoBar UI structure
        /// </summary>
        [ContextMenu("Create TopInfoBar UI")]
        public void CreateTopInfoBarUI()
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
            
            // Create TopInfoBar container
            GameObject infoBarGO = new GameObject("TopInfoBar");
            infoBarGO.transform.SetParent(canvas.transform, false);
            
            // Add TopInfoBar component
            topInfoBar = infoBarGO.AddComponent<TopInfoBar>();
            
            // Set RectTransform
            RectTransform rectTransform = infoBarGO.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = infoBarGO.AddComponent<RectTransform>();
            }
            
            rectTransform.sizeDelta = infoBarSize;
            rectTransform.anchoredPosition = infoBarPosition;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Create text components
            CreateTextComponents(infoBarGO);
            
            // Configure TopInfoBar
            topInfoBar.player1Name = player1Name;
            topInfoBar.player2Name = player2Name;
            topInfoBar.currentPlayerColor = currentPlayerColor;
            topInfoBar.normalPlayerColor = normalPlayerColor;
            topInfoBar.normalTimeColor = normalTimeColor;
            topInfoBar.warningTimeColor = warningTimeColor;
            topInfoBar.dangerTimeColor = dangerTimeColor;
            
        }
        
        /// <summary>
        /// Create text components
        /// </summary>
        private void CreateTextComponents(GameObject parent)
        {
            // Create current player text
            CreateTextComponent(parent, "CurrentPlayerText", "Current Player: Waiting...", 
                new Vector2(-350, 0), new Vector2(250, 40), 24, Color.yellow);
            
            // Create turn number text
            CreateTextComponent(parent, "TurnNumberText", "Turn: 0", 
                new Vector2(-100, 0), new Vector2(150, 40), 24, Color.white);
            
            // Create player 1 score text
            CreateTextComponent(parent, "Player1ScoreText", $"{player1Name}: 0 pts", 
                new Vector2(150, 0), new Vector2(200, 40), 22, Color.white);
            
            // Create player 2 score text
            CreateTextComponent(parent, "Player2ScoreText", $"{player2Name}: 0 pts", 
                new Vector2(400, 0), new Vector2(200, 40), 22, Color.white);
            
            // Create countdown text
            CreateTextComponent(parent, "CountdownText", "Time: 30.0s", 
                new Vector2(600, 0), new Vector2(200, 40), 22, normalTimeColor);
            
            // Create game status text
            CreateTextComponent(parent, "GameStatusText", "Waiting for Game Start...", 
                new Vector2(0, -35), new Vector2(500, 30), 20, Color.gray);
        }
        
        /// <summary>
        /// Create single text component
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
            
            // Set RectTransform
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
        }
        
        /// <summary>
        /// Destroy TopInfoBar UI
        /// </summary>
        [ContextMenu("Destroy TopInfoBar UI")]
        public void DestroyTopInfoBarUI()
        {
            if (topInfoBar != null)
            {
                DestroyImmediate(topInfoBar.gameObject);
                topInfoBar = null;
            }
        }
        
        /// <summary>
        /// Recreate UI
        /// </summary>
        [ContextMenu("Recreate UI")]
        public void RecreateUI()
        {
            DestroyTopInfoBarUI();
            CreateTopInfoBarUI();
        }
        
        /// <summary>
        /// Get setup status
        /// </summary>
        [ContextMenu("Get Setup Status")]
        public string GetSetupStatus()
        {
            string status = $"=== TopInfoBarSetup Status ===\n";
            status += $"Canvas: {(canvas != null ? "Found" : "Not Found")}\n";
            status += $"TopInfoBar: {(topInfoBar != null ? "Created" : "Not Created")}\n";
            status += $"Player 1 Name: {player1Name}\n";
            status += $"Player 2 Name: {player2Name}\n";
            status += $"Current Player Color: {currentPlayerColor}\n";
            status += $"Normal Player Color: {normalPlayerColor}\n";
            status += $"Normal Time Color: {normalTimeColor}\n";
            status += $"Warning Time Color: {warningTimeColor}\n";
            status += $"Danger Time Color: {dangerTimeColor}\n";
            status += $"Info Bar Size: {infoBarSize}\n";
            status += $"Info Bar Position: {infoBarPosition}";
            
            return status;
        }
    }
}
