using UnityEngine;
using System.Collections.Generic;
using SplendorUnity.UI;

namespace SplendorUnity.Display
{
    /// <summary>
    /// UI显示器，提供图形界面显示
    /// </summary>
    public class UIDisplayer : MonoBehaviour
    {
        [Header("UI配置")]
        public bool enableUI = true;
        public bool enableAnimations = true;
        
        [Header("UI组件引用")]
        public GameUI gameUI;
        public GemUI gemUI;
        public NobleUI nobleUI;
        public PlayerUI playerUI;
        
        protected virtual void Awake()
        {
            // 初始化UI组件引用
            if (gameUI == null)
                gameUI = FindObjectOfType<GameUI>();
            if (gemUI == null)
                gemUI = FindObjectOfType<GemUI>();
            if (nobleUI == null)
                nobleUI = FindObjectOfType<NobleUI>();
            if (playerUI == null)
                playerUI = FindObjectOfType<PlayerUI>();
        }
        
        /// <summary>
        /// 初始化显示器
        /// </summary>
        public virtual void InitDisplayer(object runner)
        {
            if (!enableUI) return;
                        
            if (gameUI != null)
                gameUI.Initialize();
        }
        
        /// <summary>
        /// 开始回合
        /// </summary>
        public virtual void StartRound(object gameState)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.StartNewRound();
        }
        
        /// <summary>
        /// 显示游戏状态
        /// </summary>
        public virtual void DisplayState(object gameState)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.UpdateGameState(gameState);
        }
        
        /// <summary>
        /// 显示可用动作
        /// </summary>
        public virtual void DisplayAvailableActions(int agentId, List<object> actions)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.ShowAvailableActions(agentId, actions);
        }
        
        /// <summary>
        /// 执行动作
        /// </summary>
        public virtual void ExecuteAction(int agentId, object action, object gameState)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.ExecuteAction(agentId, action);
        }
        
        /// <summary>
        /// 用户输入
        /// </summary>
        public virtual object UserInput(List<object> actions)
        {
            if (!enableUI) return null;
            
            if (gameUI != null)
                return gameUI.GetUserInput(actions);
                
            return null;
        }
        
        /// <summary>
        /// 检查是否有用户输入
        /// </summary>
        public virtual bool HasUserInput()
        {
            if (!enableUI) return false;
            
            if (gameUI != null)
                return gameUI.HasUserInput();
                
            return false;
        }
        
        /// <summary>
        /// 超时警告
        /// </summary>
        public virtual void TimeOutWarning(object runner, int agentId)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.ShowTimeoutWarning(agentId);
        }
        
        /// <summary>
        /// 非法动作警告
        /// </summary>
        public virtual void IllegalWarning(object runner, int agentId)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.ShowIllegalWarning(agentId);
        }
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        public virtual void EndGame(object gameState, List<double> scores)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.ShowGameEnd(scores);
        }
        

        
        /// <summary>
        /// 更新宝石显示
        /// </summary>
        public virtual void UpdateGemDisplay(object gemData)
        {
            if (!enableUI || gemUI == null) return;
            
            gemUI.UpdateGems(gemData);
        }
        
        /// <summary>
        /// 更新贵族显示
        /// </summary>
        public virtual void UpdateNobleDisplay(object nobleData)
        {
            if (!enableUI || nobleUI == null) return;
            
            nobleUI.UpdateNobles(nobleData);
        }
        
        /// <summary>
        /// 更新玩家显示
        /// </summary>
        public virtual void UpdatePlayerDisplay(int playerId, object playerData)
        {
            if (!enableUI || playerUI == null) return;
            
            playerUI.UpdatePlayer(playerId, playerData);
        }
        
        /// <summary>
        /// 播放动画
        /// </summary>
        public virtual void PlayAnimation(string animationName, object data = null)
        {
            if (!enableUI || !enableAnimations) return;
            
            if (gameUI != null)
                gameUI.PlayAnimation(animationName, data);
        }
        
        /// <summary>
        /// 显示消息
        /// </summary>
        public virtual void ShowMessage(string message, float duration = 3f)
        {
            if (!enableUI) return;
            
            if (gameUI != null)
                gameUI.ShowMessage(message, duration);
        }
    }
}