using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 游戏主界面
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("UI配置")]
        public bool enableDebugLog = true;
        
        /// <summary>
        /// 初始化游戏UI
        /// </summary>
        public virtual void Initialize()
        {
        }
        
        /// <summary>
        /// 开始新回合
        /// </summary>
        public virtual void StartNewRound()
        {
        }
        
        /// <summary>
        /// 更新游戏状态
        /// </summary>
        public virtual void UpdateGameState(object gameState)
        {
        }
        
        /// <summary>
        /// 显示可用动作
        /// </summary>
        public virtual void ShowAvailableActions(int agentId, List<object> actions)
        {
        }
        
        /// <summary>
        /// 执行动作
        /// </summary>
        public virtual void ExecuteAction(int agentId, object action)
        {
        }
        
        /// <summary>
        /// 获取用户输入
        /// </summary>
        public virtual object GetUserInput(List<object> actions)
        {
            return null;
        }
        
        /// <summary>
        /// 检查是否有用户输入
        /// </summary>
        public virtual bool HasUserInput()
        {
            // 默认情况下，GameUI没有用户输入
            // 子类可以重写此方法来实现实际的用户输入检查
            return false;
        }
        
        /// <summary>
        /// 显示超时警告
        /// </summary>
        public virtual void ShowTimeoutWarning(int agentId)
        {
            if (enableDebugLog)
                Debug.LogWarning($"GameUI: 代理 {agentId} 超时警告");
        }
        
        /// <summary>
        /// 显示非法动作警告
        /// </summary>
        public virtual void ShowIllegalWarning(int agentId)
        {
        }
        
        /// <summary>
        /// 显示游戏结束
        /// </summary>
        public virtual void ShowGameEnd(List<double> scores)
        {
        }
        
        /// <summary>
        /// 播放动画
        /// </summary>
        public virtual void PlayAnimation(string animationName, object data = null)
        {
        }
        
        /// <summary>
        /// 显示消息
        /// </summary>
        public virtual void ShowMessage(string message, float duration = 3f)
        {
        }
    }
}