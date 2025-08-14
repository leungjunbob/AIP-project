using UnityEngine;
using System.Collections.Generic;

namespace SplendorUnity.Display
{
    /// <summary>
    /// 游戏显示器，替代原Displayer类
    /// </summary>
    public class GameDisplayer : MonoBehaviour
    {
        [Header("显示器配置")]
        public bool enableLogging = true;
        public bool enableUI = true;
        
        [Header("UI引用")]
        public TextDisplayer textDisplayer;
        public UIDisplayer uiDisplayer;
        
        protected virtual void Awake()
        {
            // 初始化显示器组件
            if (textDisplayer == null)
                textDisplayer = GetComponent<TextDisplayer>();
            if (uiDisplayer == null)
                uiDisplayer = GetComponent<UIDisplayer>();
        }
        
        /// <summary>
        /// 初始化显示器
        /// </summary>
        public virtual void InitDisplayer(object runner)
        {                
            if (textDisplayer != null)
                textDisplayer.InitDisplayer(runner);
            if (uiDisplayer != null)
                uiDisplayer.InitDisplayer(runner);
        }
        
        /// <summary>
        /// 开始回合
        /// </summary>
        public virtual void StartRound(object gameState)
        {
               
            if (textDisplayer != null)
                textDisplayer.StartRound(gameState);
            if (uiDisplayer != null)
                uiDisplayer.StartRound(gameState);
        }
        
        /// <summary>
        /// 显示游戏状态
        /// </summary>
        public virtual void DisplayState(object gameState)
        {
                
            if (textDisplayer != null)
                textDisplayer.DisplayState(gameState);
            if (uiDisplayer != null)
                uiDisplayer.DisplayState(gameState);
        }
        
        /// <summary>
        /// 显示可用动作
        /// </summary>
        public virtual void DisplayAvailableActions(int agentId, List<object> actions)
        {
                
            if (textDisplayer != null)
                textDisplayer.DisplayAvailableActions(agentId, actions);
            if (uiDisplayer != null)
                uiDisplayer.DisplayAvailableActions(agentId, actions);
        }
        
        /// <summary>
        /// 执行动作
        /// </summary>
        public virtual void ExecuteAction(int agentId, object action, object gameState)
        {
                
            if (textDisplayer != null)
                textDisplayer.ExecuteAction(agentId, action, gameState);
            if (uiDisplayer != null)
                uiDisplayer.ExecuteAction(agentId, action, gameState);
        }
        
        /// <summary>
        /// 用户输入
        /// </summary>
        public virtual object UserInput(List<object> actions)
        {
                
            if (uiDisplayer != null)
                return uiDisplayer.UserInput(actions);
            if (textDisplayer != null)
                return textDisplayer.UserInput(actions);
                
            return null;
        }
        
        /// <summary>
        /// 检查是否有用户输入
        /// </summary>
        public virtual bool HasUserInput()
        {
            if (uiDisplayer != null)
                return uiDisplayer.HasUserInput();
            if (textDisplayer != null)
                return textDisplayer.HasUserInput();
                
            return false; // 默认没有用户输入
        }
        
        /// <summary>
        /// 超时警告
        /// </summary>
        public virtual void TimeOutWarning(object runner, int agentId)
        {
                
            if (textDisplayer != null)
                textDisplayer.TimeOutWarning(runner, agentId);
            if (uiDisplayer != null)
                uiDisplayer.TimeOutWarning(runner, agentId);
        }
        
        /// <summary>
        /// 非法动作警告
        /// </summary>
        public virtual void IllegalWarning(object runner, int agentId)
        {
                
            if (textDisplayer != null)
                textDisplayer.IllegalWarning(runner, agentId);
            if (uiDisplayer != null)
                uiDisplayer.IllegalWarning(runner, agentId);
        }
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        public virtual void EndGame(object gameState, List<double> scores)
        {
               
            if (textDisplayer != null)
                textDisplayer.EndGame(gameState, scores);
            if (uiDisplayer != null)
                uiDisplayer.EndGame(gameState, scores);
        }
    }
}