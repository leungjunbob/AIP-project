using UnityEngine;
using SplendorUnity.Core;
using SplendorUnity.Display;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 宝石界面
    /// </summary>
    public class GemUI : MonoBehaviour
    {
        
        [Header("宝石堆管理器")]
        public GemPileManager gemPileManager;
        
        protected virtual void Awake()
        {
            // 自动查找GemPileManager
            if (gemPileManager == null)
                gemPileManager = FindObjectOfType<GemPileManager>();
        }
        
        /// <summary>
        /// 更新宝石显示
        /// </summary>
        public virtual void UpdateGems(object gemData)
        {
                
            // 如果传入的是SplendorGameState，更新宝石堆
            if (gemData is SplendorGameState gameState && gemPileManager != null)
            {
                gemPileManager.UpdateGemPiles(gameState);
            }
        }
        
        /// <summary>
        /// 显示宝石信息
        /// </summary>
        public virtual void ShowGemInfo(object gemData)
        {
        }
        
        /// <summary>
        /// 隐藏宝石信息
        /// </summary>
        public virtual void HideGemInfo()
        {
        }
        
        /// <summary>
        /// 高亮宝石
        /// </summary>
        public virtual void HighlightGem(object gemData)
        {
        }
        
        /// <summary>
        /// 取消高亮宝石
        /// </summary>
        public virtual void UnhighlightGem(object gemData)
        {
        }
        
        /// <summary>
        /// 显示宝石收集动画
        /// </summary>
        public virtual void ShowGemCollectionAnimation(object gemData)
        {
        }
    }
}