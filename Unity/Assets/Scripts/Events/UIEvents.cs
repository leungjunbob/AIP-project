using UnityEngine;
using UnityEngine.Events;
using System;

namespace SplendorUnity.Events
{
    /// <summary>
    /// UI事件系统，用于处理UI相关的各种事件
    /// </summary>
    public static class UIEvents
    {
        // UI交互事件
        public static event Action<object> OnCardClicked;
        public static event Action<object> OnCardHovered;
        public static event Action<object> OnCardDragged;
        public static event Action<object> OnCardDropped;
        
        public static event Action<object> OnGemClicked;
        public static event Action<object> OnGemHovered;
        public static event Action<object> OnGemDragged;
        public static event Action<object> OnGemDropped;
        
        public static event Action<object> OnNobleClicked;
        public static event Action<object> OnNobleHovered;
        
        public static event Action<int> OnPlayerPanelClicked;
        public static event Action<int> OnPlayerPanelHovered;
        
        // UI状态事件
        public static event Action<bool> OnUIEnabled;
        public static event Action<string> OnUIMessageShown;
        public static event Action<string> OnUIMessageHidden;
        public static event Action<string> OnAnimationStarted;
        public static event Action<string> OnAnimationCompleted;
        
        // 按钮事件
        public static event Action OnStartGameClicked;
        public static event Action OnPauseGameClicked;
        public static event Action OnResumeGameClicked;
        public static event Action OnRestartGameClicked;
        public static event Action OnQuitGameClicked;
        public static event Action OnSettingsClicked;
        public static event Action OnHelpClicked;
        
        // 设置事件
        public static event Action<bool> OnSoundToggled;
        public static event Action<bool> OnMusicToggled;
        public static event Action<float> OnVolumeChanged;
        public static event Action<string> OnLanguageChanged;
        
        // 触发卡牌点击事件
        public static void TriggerCardClicked(object card)
        {
            OnCardClicked?.Invoke(card);
        }
        
        // 触发卡牌悬停事件
        public static void TriggerCardHovered(object card)
        {
            OnCardHovered?.Invoke(card);
        }
        
        // 触发卡牌拖拽事件
        public static void TriggerCardDragged(object card)
        {
            OnCardDragged?.Invoke(card);
        }
        
        // 触发卡牌放置事件
        public static void TriggerCardDropped(object card)
        {
            OnCardDropped?.Invoke(card);
        }
        
        // 触发宝石点击事件
        public static void TriggerGemClicked(object gem)
        {
            OnGemClicked?.Invoke(gem);
        }
        
        // 触发宝石悬停事件
        public static void TriggerGemHovered(object gem)
        {
            OnGemHovered?.Invoke(gem);
        }
        
        // 触发宝石拖拽事件
        public static void TriggerGemDragged(object gem)
        {
            OnGemDragged?.Invoke(gem);
        }
        
        // 触发宝石放置事件
        public static void TriggerGemDropped(object gem)
        {
            OnGemDropped?.Invoke(gem);
        }
        
        // 触发贵族点击事件
        public static void TriggerNobleClicked(object noble)
        {
            OnNobleClicked?.Invoke(noble);
        }
        
        // 触发贵族悬停事件
        public static void TriggerNobleHovered(object noble)
        {
            OnNobleHovered?.Invoke(noble);
        }
        
        // 触发玩家面板点击事件
        public static void TriggerPlayerPanelClicked(int playerId)
        {
            OnPlayerPanelClicked?.Invoke(playerId);
        }
        
        // 触发玩家面板悬停事件
        public static void TriggerPlayerPanelHovered(int playerId)
        {
            OnPlayerPanelHovered?.Invoke(playerId);
        }
        
        // 触发UI启用/禁用事件
        public static void TriggerUIEnabled(bool enabled)
        {
            OnUIEnabled?.Invoke(enabled);
        }
        
        // 触发UI消息显示事件
        public static void TriggerUIMessageShown(string message)
        {
            OnUIMessageShown?.Invoke(message);
        }
        
        // 触发UI消息隐藏事件
        public static void TriggerUIMessageHidden(string message)
        {
            OnUIMessageHidden?.Invoke(message);
        }
        
        // 触发动画开始事件
        public static void TriggerAnimationStarted(string animationName)
        {
            OnAnimationStarted?.Invoke(animationName);
        }
        
        // 触发动画完成事件
        public static void TriggerAnimationCompleted(string animationName)
        {
            OnAnimationCompleted?.Invoke(animationName);
        }
        
        // 触发开始游戏按钮点击事件
        public static void TriggerStartGameClicked()
        {
            OnStartGameClicked?.Invoke();
        }
        
        // 触发暂停游戏按钮点击事件
        public static void TriggerPauseGameClicked()
        {
            OnPauseGameClicked?.Invoke();
        }
        
        // 触发恢复游戏按钮点击事件
        public static void TriggerResumeGameClicked()
        {
            OnResumeGameClicked?.Invoke();
        }
        
        // 触发重新开始游戏按钮点击事件
        public static void TriggerRestartGameClicked()
        {
            OnRestartGameClicked?.Invoke();
        }
        
        // 触发退出游戏按钮点击事件
        public static void TriggerQuitGameClicked()
        {
            OnQuitGameClicked?.Invoke();
        }
        
        // 触发设置按钮点击事件
        public static void TriggerSettingsClicked()
        {
            OnSettingsClicked?.Invoke();
        }
        
        // 触发帮助按钮点击事件
        public static void TriggerHelpClicked()
        {
            OnHelpClicked?.Invoke();
        }
        
        // 触发声音开关事件
        public static void TriggerSoundToggled(bool enabled)
        {
            OnSoundToggled?.Invoke(enabled);
        }
        
        // 触发音乐开关事件
        public static void TriggerMusicToggled(bool enabled)
        {
            OnMusicToggled?.Invoke(enabled);
        }
        
        // 触发音量变化事件
        public static void TriggerVolumeChanged(float volume)
        {
            OnVolumeChanged?.Invoke(volume);
        }
        
        // 触发语言变化事件
        public static void TriggerLanguageChanged(string language)
        {
            OnLanguageChanged?.Invoke(language);
        }
        
        // 清除所有UI事件
        public static void ClearAllUIEvents()
        {
            OnCardClicked = null;
            OnCardHovered = null;
            OnCardDragged = null;
            OnCardDropped = null;
            OnGemClicked = null;
            OnGemHovered = null;
            OnGemDragged = null;
            OnGemDropped = null;
            OnNobleClicked = null;
            OnNobleHovered = null;
            OnPlayerPanelClicked = null;
            OnPlayerPanelHovered = null;
            OnUIEnabled = null;
            OnUIMessageShown = null;
            OnUIMessageHidden = null;
            OnAnimationStarted = null;
            OnAnimationCompleted = null;
            OnStartGameClicked = null;
            OnPauseGameClicked = null;
            OnResumeGameClicked = null;
            OnRestartGameClicked = null;
            OnQuitGameClicked = null;
            OnSettingsClicked = null;
            OnHelpClicked = null;
            OnSoundToggled = null;
            OnMusicToggled = null;
            OnVolumeChanged = null;
            OnLanguageChanged = null;
        }
    }
}