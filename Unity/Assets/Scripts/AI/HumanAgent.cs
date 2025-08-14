using UnityEngine;
using System.Collections.Generic;
using SplendorUnity.Core;
using SplendorUnity.Models;
using SplendorUnity.UI;

namespace SplendorUnity.AI
{
    /// <summary>
    /// 人类玩家代理 - 与GameManager的玩家输入系统集成
    /// </summary>
    public class HumanAgent : BaseAgent
    {
        [Header("Human Agent Settings")]
        
        private GameManager gameManager;
        private SplendorGameState currentGameState;
        private bool isWaitingForPlayerInput = false;
        
        private void Start()
        {
            // 查找GameManager
            gameManager = FindObjectOfType<GameManager>();
        }
        
        /// <summary>
        /// 实现BaseAgent的抽象方法
        /// HumanAgent返回null，让GameManager通过其玩家输入系统处理
        /// </summary>
        public override object SelectAction(List<object> actions, object gameState)
        {
            // 尝试转换游戏状态
            if (gameState is SplendorGameState splendorGameState)
            {
                currentGameState = splendorGameState;
                isWaitingForPlayerInput = true;
                
                if (gameManager != null && gameManager.enableDebugLog)
                {
                    Debug.Log($"HumanAgent: 回合开始，等待玩家输入，可用动作数量: {actions.Count}");
                }
                
                // 通知GameManager这是玩家回合，重置输入标志
                if (gameManager != null)
                {
                    gameManager.SetPlayerInputFlag(false);
                }
            }
            
            // 返回null，让GameManager通过其玩家输入系统处理
            return null;
        }
        
        /// <summary>
        /// 显示可用的动作选项
        /// </summary>
        private void ShowAvailableActions(SplendorGameState gameState)
        {
            
            // 获取合法动作
            var gameRule = FindObjectOfType<SplendorGameRule>();
            if (gameRule != null)
            {
                var legalActions = gameRule.GetLegalActions(gameState, this.id);
                
                
                // 显示UI面板让玩家选择动作
                ShowActionSelectionUI(gameState, legalActions);
            }
        }
        
        /// <summary>
        /// 显示动作选择UI
        /// </summary>
        private void ShowActionSelectionUI(SplendorGameState gameState, List<object> legalActions)
        {
            
            // 这里可以显示各种UI面板，让玩家选择动作
            // 例如：宝石收集面板、卡牌购买面板、卡牌保留面板等
            
            // 自动显示宝石收集面板（如果有收集宝石的动作）
            ShowGemCollectionPanel(gameState);
            
            // 可以添加其他UI面板的显示逻辑
            // ShowCardPurchasePanel(gameState);
            // ShowCardReservePanel(gameState);
        }
        
        /// <summary>
        /// 显示宝石收集面板
        /// </summary>
        private void ShowGemCollectionPanel(SplendorGameState gameState)
        {
            var gemCollectionPanel = FindObjectOfType<GemCollectionPanel>();
            if (gemCollectionPanel != null)
            {
                gemCollectionPanel.ShowPanel(gameState, gameState.agents[this.id]);
            }
        }
        
        /// <summary>
        /// 玩家选择收集宝石动作 - 通过GameManager设置
        /// </summary>
        public void PlayerChooseCollectGems(Dictionary<string, int> selectedGems)
        {
            if (!isWaitingForPlayerInput)
            {
                return;
            }
            
            
            // 创建收集宝石动作
            var action = Action.CreateCollectAction(selectedGems, new Dictionary<string, int>());
            
            // 通过GameManager设置玩家选择的动作
            if (gameManager != null)
            {
                gameManager.SetPlayerAction(action);
            }
        }
        
        /// <summary>
        /// 玩家选择购买卡牌动作 - 通过GameManager设置
        /// </summary>
        public void PlayerChooseBuyCard(Card card, Dictionary<string, int> returnedGems)
        {
            if (!isWaitingForPlayerInput)
            {
                return;
            }
            
            
            // 创建购买卡牌动作
            var action = Action.CreateBuyAction(card, false, returnedGems, null);
            
            // 通过GameManager设置玩家选择的动作
            if (gameManager != null)
            {
                gameManager.SetPlayerAction(action);
            }
        }
        
        /// <summary>
        /// 玩家选择保留卡牌动作 - 通过GameManager设置
        /// </summary>
        public void PlayerChooseReserveCard(Card card, Dictionary<string, int> collectedGems, Dictionary<string, int> returnedGems)
        {
            if (!isWaitingForPlayerInput)
            {
                return;
            }
            
            
            // 创建保留卡牌动作
            var action = Action.CreateReserveAction(card, collectedGems, returnedGems);
            
            // 通过GameManager设置玩家选择的动作
            if (gameManager != null)
            {
                gameManager.SetPlayerAction(action);
            }
        }
        
        /// <summary>
        /// 获取宝石选择字符串
        /// </summary>
        private string GetGemSelectionString(Dictionary<string, int> selectedGems)
        {
            var gemStrings = new List<string>();
            foreach (var kvp in selectedGems)
            {
                if (kvp.Value > 0)
                {
                    gemStrings.Add($"{kvp.Key}:{kvp.Value}");
                }
            }
            return string.Join(", ", gemStrings);
        }
        
        /// <summary>
        /// 检查是否是玩家的回合
        /// </summary>
        public bool IsPlayerTurn()
        {
            return isWaitingForPlayerInput;
        }
        
        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public SplendorGameState GetCurrentGameState()
        {
            return currentGameState;
        }
        
        /// <summary>
        /// 重置等待状态（供外部调用）
        /// </summary>
        public void ResetWaitingState()
        {
            isWaitingForPlayerInput = false;
        }
        
        
        /// <summary>
        /// 手动显示宝石收集面板
        /// </summary>
        [ContextMenu("Show Gem Collection Panel")]
        public void ShowGemCollectionPanelManually()
        {
            if (currentGameState != null)
            {
                ShowGemCollectionPanel(currentGameState);
            }
        }
        
        /// <summary>
        /// 手动隐藏宝石收集面板
        /// </summary>
        [ContextMenu("Hide Gem Collection Panel")]
        public void HideGemCollectionPanelManually()
        {
            var gemCollectionPanel = FindObjectOfType<GemCollectionPanel>();
            if (gemCollectionPanel != null)
            {
                gemCollectionPanel.HidePanel();
            }
        }
    }
}
