using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SplendorUnity.Core;
using SplendorUnity.Models;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 购买卡片UI - 实现购买卡片时的宝石选择界面
    /// </summary>
    public class BuyUI : MonoBehaviour
    {
        [Header("UI Components")]
        public GameObject panelContainer;           // 面板容器
        public Button confirmButton;               // 确认按钮
        public Button cancelButton;                // 取消按钮
        public TextMeshProUGUI invalidMessageText; // 无效提示文本
        public TextMeshProUGUI cardInfoText;       // 卡片信息文本
        

        
        // 私有字段
        private SplendorGameState gameState;           // 游戏状态
        private SplendorGameState.AgentState currentPlayer; // 当前玩家
        private Card targetCard;                       // 要购买的卡片
        
        private void Awake()
        {
            InitializePanel();
        }
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        private void InitializePanel()
        {
            // 创建UI结构
            CreateUIStructure();
            
            // 设置按钮事件
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
            
            // 隐藏面板
            if (panelContainer != null)
                panelContainer.SetActive(false);
            
        }
        
        /// <summary>
        /// 创建UI结构
        /// </summary>
        private void CreateUIStructure()
        {
            // 创建面板容器
            if (panelContainer == null)
            {
                panelContainer = new GameObject("PanelContainer");
                panelContainer.transform.SetParent(transform, false);
                
                // 添加Image组件作为背景
                var panelImage = panelContainer.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // 半透明黑色背景
                
                // 设置面板大小和位置
                var panelRect = panelContainer.GetComponent<RectTransform>();
                panelRect.sizeDelta = new Vector2(1000, 800);
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
            }
            
            // 创建卡片信息文本
            if (cardInfoText == null)
            {
                var cardInfoGO = new GameObject("CardInfoText");
                cardInfoGO.transform.SetParent(panelContainer.transform, false);
                
                cardInfoText = cardInfoGO.AddComponent<TextMeshProUGUI>();
                cardInfoText.text = "Card Info";
                cardInfoText.fontSize = 16;  // 缩小字体
                cardInfoText.color = Color.white;
                cardInfoText.alignment = TextAlignmentOptions.Left;
                
                var cardInfoRect = cardInfoGO.GetComponent<RectTransform>();
                cardInfoRect.sizeDelta = new Vector2(500, 300);  // 增加高度以容纳更多内容
                cardInfoRect.anchorMin = new Vector2(0.5f, 0.8f);  // 调整位置
                cardInfoRect.anchorMax = new Vector2(0.5f, 0.8f);
                cardInfoRect.anchoredPosition = Vector2.zero;
                
                // 添加背景面板让文本更易读
                var backgroundGO = new GameObject("CardInfoBackground");
                backgroundGO.transform.SetParent(cardInfoGO.transform, false);
                backgroundGO.transform.SetAsFirstSibling(); // 放在文本后面作为背景
                
                var backgroundImage = backgroundGO.AddComponent<Image>();
                backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                var backgroundRect = backgroundGO.GetComponent<RectTransform>();
                backgroundRect.sizeDelta = new Vector2(500, 300);  // 增加背景高度
                backgroundRect.anchorMin = Vector2.zero;
                backgroundRect.anchorMax = Vector2.one;
                backgroundRect.anchoredPosition = Vector2.zero;
                backgroundRect.offsetMin = new Vector2(-10, -10);
                backgroundRect.offsetMax = new Vector2(10, 10);
            }
            
            // 创建确认按钮
            if (confirmButton == null)
            {
                var confirmGO = new GameObject("ConfirmButton");
                confirmGO.transform.SetParent(panelContainer.transform, false);
                
                confirmButton = confirmGO.AddComponent<Button>();
                var confirmImage = confirmGO.AddComponent<Image>();
                confirmImage.color = new Color(0.2f, 0.8f, 0.2f); // 绿色
                
                var confirmRect = confirmGO.GetComponent<RectTransform>();
                confirmRect.sizeDelta = new Vector2(120, 50);
                confirmRect.anchorMin = new Vector2(0.3f, 0.1f);
                confirmRect.anchorMax = new Vector2(0.3f, 0.1f);
                confirmRect.anchoredPosition = Vector2.zero;
                
                // 添加确认按钮文本
                var confirmTextGO = new GameObject("ConfirmText");
                confirmTextGO.transform.SetParent(confirmGO.transform, false);
                
                var confirmText = confirmTextGO.AddComponent<TextMeshProUGUI>();
                confirmText.text = "Confirm";
                confirmText.fontSize = 18;
                confirmText.color = Color.white;
                confirmText.alignment = TextAlignmentOptions.Center;
                
                var confirmTextRect = confirmTextGO.GetComponent<RectTransform>();
                confirmTextRect.sizeDelta = new Vector2(120, 50);
                confirmTextRect.anchorMin = Vector2.zero;
                confirmTextRect.anchorMax = Vector2.one;
                confirmTextRect.anchoredPosition = Vector2.zero;
            }
            
            // 创建取消按钮
            if (cancelButton == null)
            {
                var cancelGO = new GameObject("CancelButton");
                cancelGO.transform.SetParent(panelContainer.transform, false);
                
                cancelButton = cancelGO.AddComponent<Button>();
                var cancelImage = cancelGO.AddComponent<Image>();
                cancelImage.color = new Color(0.8f, 0.2f, 0.2f); // 红色
                
                var cancelRect = cancelGO.GetComponent<RectTransform>();
                cancelRect.sizeDelta = new Vector2(120, 50);
                cancelRect.anchorMin = new Vector2(0.7f, 0.1f);
                cancelRect.anchorMax = new Vector2(0.7f, 0.1f);
                cancelRect.anchoredPosition = Vector2.zero;
                
                // 添加取消按钮文本
                var cancelTextGO = new GameObject("CancelText");
                cancelTextGO.transform.SetParent(cancelGO.transform, false);
                
                var cancelText = cancelTextGO.AddComponent<TextMeshProUGUI>();
                cancelText.text = "Cancel";
                cancelText.fontSize = 18;
                cancelText.color = Color.white;
                cancelText.alignment = TextAlignmentOptions.Center;
                
                var cancelTextRect = cancelTextGO.GetComponent<RectTransform>();
                cancelTextRect.sizeDelta = new Vector2(120, 50);
                cancelTextRect.anchorMin = Vector2.zero;
                cancelTextRect.anchorMax = Vector2.one;
                cancelTextRect.anchoredPosition = Vector2.zero;
            }

        }
        
        /// <summary>
        /// 显示购买面板
        /// </summary>
        public void ShowPanel(SplendorGameState gameState, SplendorGameState.AgentState currentPlayer, Card targetCard)
        {
            
            this.gameState = gameState;
            this.currentPlayer = currentPlayer;
            this.targetCard = targetCard;
            
            // 显示面板
            if (panelContainer != null)
            {
                panelContainer.SetActive(true);
                
                // 更新卡片信息
                UpdateCardInfo();

            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            if (panelContainer != null)
            {
                panelContainer.SetActive(false);

            }
        }
        

        /// <summary>
        /// 更新卡片信息
        /// </summary>
        private void UpdateCardInfo()
        {
            if (cardInfoText != null && targetCard != null)
            {
                string cardInfo = $"<b>Card: {targetCard.Code}</b>\n";
                cardInfo += $"<b>Points: {targetCard.Points}</b>\n\n";
                cardInfo += "<b>Required Resources:</b>\n";
                cardInfo += GetCardCostText();
                
                // 添加支付代价信息
                cardInfo += "\n<b>Payment Cost:</b>\n";
                cardInfo += GetPaymentCostText();
                
                cardInfoText.text = cardInfo;

            }
        }
        
        /// <summary>
        /// 获取卡片成本文本
        /// </summary>
        private string GetCardCostText()
        {
            if (targetCard?.Cost == null) return "No cost";
            
            var costParts = new List<string>();
            foreach (var kvp in targetCard.Cost)
            {
                if (kvp.Value > 0)
                {
                    // 使用更友好的宝石名称显示
                    costParts.Add($"• {kvp.Value} {kvp.Key}");
                }
            }
            
            if (costParts.Count == 0)
                return "Free";
                
            return string.Join("\n", costParts);
        }
        
        /// <summary>
        /// 获取支付代价文本
        /// </summary>
        private string GetPaymentCostText()
        {
            if (targetCard == null || currentPlayer == null || gameState == null)
                return "Unable to calculate payment cost";
            
            // 获取GameManager和valid actions
            var gameManager = FindObjectOfType<Core.GameManager>();
            if (gameManager == null)
                return "GameManager not found";
            
            // 获取valid actions
            var validActions = gameManager.gameRule?.GetLegalActions((object)gameState, currentPlayer.id);
            if (validActions == null || validActions.Count == 0)
                return "No valid actions available";
            
            // 查找匹配的buy action
            Core.Action buyAction = null;
            foreach (var action in validActions)
            {
                if (action is Core.Action actionObj && (actionObj.Type == "buy" || actionObj.Type == "buy_reserved"))
                {
                    if (actionObj.Card != null && actionObj.Card.Code == targetCard.Code)
                    {
                        buyAction = actionObj;
                        break;
                    }
                }
            }
            
            if (buyAction == null)
                return "No valid buy action found";
            
            // 检查是否有需要返回的宝石
            if (buyAction.ReturnedGems == null || buyAction.ReturnedGems.Count == 0)
                return "No gems need to be returned";
            
            // 格式化返回宝石信息
            var returnParts = new List<string>();
            foreach (var kvp in buyAction.ReturnedGems)
            {
                if (kvp.Value > 0)
                {
                    returnParts.Add($"• Return {kvp.Value} {kvp.Key}");
                }
            }
            
            if (returnParts.Count == 0)
                return "No gems need to be returned";
                
            return string.Join("\n", returnParts);
        }
        

        

        

        

        

        
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OnConfirmClicked()
        {

            
            // 直接尝试购买卡片
            if (TryBuyCard())
            {
                HidePanel();
            }
        }
        
        /// <summary>
        /// 尝试购买卡片
        /// </summary>
        private bool TryBuyCard()
        {

            
            if (targetCard == null || currentPlayer == null || gameState == null)
            {

                return false;
            }
            
            // 获取GameManager和valid actions
            var gameManager = FindObjectOfType<Core.GameManager>();
            if (gameManager == null)
            {

                return false;
            }
            
            // 获取valid actions
            var validActions = gameManager.gameRule?.GetLegalActions((object)gameState, currentPlayer.id);
            if (validActions == null || validActions.Count == 0)
            {
                ShowInvalidActionMessage("Cannot buy this card - no valid actions available");
                return false;
            }
            
            // 查找匹配的buy action
            Core.Action buyAction = null;
            foreach (var action in validActions)
            {
                if (action is Core.Action actionObj && (actionObj.Type == "buy" || actionObj.Type == "buy_reserved"))
                {
                    if (actionObj.Card != null && actionObj.Card.Code == targetCard.Code)
                    {
                        buyAction = actionObj;
                        break;
                    }
                }
            }
            
            if (buyAction == null)
            {
                ShowInvalidActionMessage("Cannot buy this card - insufficient resources");
                return false;
            }
            

            
            // 执行购买action
            gameManager.SetPlayerAction(buyAction);

            return true;
        }

        /// <summary>
        /// 显示无效操作提示消息
        /// </summary>
        private void ShowInvalidActionMessage(string message)
        {

        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void OnCancelClicked()
        {

            
            HidePanel();
        }
        

        

    }
}
