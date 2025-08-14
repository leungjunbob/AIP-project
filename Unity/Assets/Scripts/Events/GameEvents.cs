using UnityEngine;
using UnityEngine.Events;
using System;

namespace SplendorUnity.Events
{
    /// <summary>
    /// 游戏事件系统，用于处理游戏中的各种事件
    /// </summary>
    public static class GameEvents
    {
        // 游戏生命周期事件
        public static event Action OnGameStarted;
        public static event Action OnGameEnded;
        public static event Action OnRoundStarted;
        public static event Action OnRoundEnded;
        
        // 玩家事件
        public static event Action<int> OnPlayerTurnStarted;
        public static event Action<int> OnPlayerTurnEnded;
        public static event Action<int, object> OnActionExecuted;
        public static event Action<int> OnPlayerScoreChanged;
        
        // 游戏状态事件
        public static event Action<object> OnGameStateChanged;
        public static event Action<int, object> OnPlayerStateChanged;
        public static event Action<object> OnBoardStateChanged;
        
        // 错误和警告事件
        public static event Action<int> OnPlayerTimeout;
        public static event Action<int> OnPlayerIllegalAction;
        public static event Action<int> OnPlayerWarning;
        
        // 游戏元素事件
        public static event Action<object> OnCardPurchased;
        public static event Action<object> OnCardReserved;
        public static event Action<object> OnGemCollected;
        public static event Action<object> OnNobleVisited;
        
        // 触发游戏开始事件
        public static void TriggerGameStarted()
        {
            OnGameStarted?.Invoke();
        }
        
        // 触发游戏结束事件
        public static void TriggerGameEnded()
        {
            OnGameEnded?.Invoke();
        }
        
        // 触发回合开始事件
        public static void TriggerRoundStarted()
        {
            OnRoundStarted?.Invoke();
        }
        
        // 触发回合结束事件
        public static void TriggerRoundEnded()
        {
            OnRoundEnded?.Invoke();
        }
        
        // 触发玩家回合开始事件
        public static void TriggerPlayerTurnStarted(int playerId)
        {
            OnPlayerTurnStarted?.Invoke(playerId);
        }
        
        // 触发玩家回合结束事件
        public static void TriggerPlayerTurnEnded(int playerId)
        {
            OnPlayerTurnEnded?.Invoke(playerId);
        }
        
        // 触发动作执行事件
        public static void TriggerActionExecuted(int playerId, object action)
        {
            OnActionExecuted?.Invoke(playerId, action);
        }
        
        // 触发玩家分数变化事件
        public static void TriggerPlayerScoreChanged(int playerId)
        {
            OnPlayerScoreChanged?.Invoke(playerId);
        }
        
        // 触发游戏状态变化事件
        public static void TriggerGameStateChanged(object gameState)
        {
            OnGameStateChanged?.Invoke(gameState);
        }
        
        // 触发玩家状态变化事件
        public static void TriggerPlayerStateChanged(int playerId, object playerState)
        {
            OnPlayerStateChanged?.Invoke(playerId, playerState);
        }
        
        // 触发棋盘状态变化事件
        public static void TriggerBoardStateChanged(object boardState)
        {
            OnBoardStateChanged?.Invoke(boardState);
        }
        
        // 触发玩家超时事件
        public static void TriggerPlayerTimeout(int playerId)
        {
            OnPlayerTimeout?.Invoke(playerId);
        }
        
        // 触发玩家非法动作事件
        public static void TriggerPlayerIllegalAction(int playerId)
        {
            OnPlayerIllegalAction?.Invoke(playerId);
        }
        
        // 触发玩家警告事件
        public static void TriggerPlayerWarning(int playerId)
        {
            OnPlayerWarning?.Invoke(playerId);
        }
        
        // 触发卡牌购买事件
        public static void TriggerCardPurchased(object card)
        {
            OnCardPurchased?.Invoke(card);
        }
        
        // 触发卡牌保留事件
        public static void TriggerCardReserved(object card)
        {
            OnCardReserved?.Invoke(card);
        }
        
        // 触发宝石收集事件
        public static void TriggerGemCollected(object gem)
        {
            OnGemCollected?.Invoke(gem);
        }
        
        // 触发贵族访问事件
        public static void TriggerNobleVisited(object noble)
        {
            OnNobleVisited?.Invoke(noble);
        }
        
        // 清除所有事件
        public static void ClearAllEvents()
        {
            OnGameStarted = null;
            OnGameEnded = null;
            OnRoundStarted = null;
            OnRoundEnded = null;
            OnPlayerTurnStarted = null;
            OnPlayerTurnEnded = null;
            OnActionExecuted = null;
            OnPlayerScoreChanged = null;
            OnGameStateChanged = null;
            OnPlayerStateChanged = null;
            OnBoardStateChanged = null;
            OnPlayerTimeout = null;
            OnPlayerIllegalAction = null;
            OnPlayerWarning = null;
            OnCardPurchased = null;
            OnCardReserved = null;
            OnGemCollected = null;
            OnNobleVisited = null;
        }
    }
}