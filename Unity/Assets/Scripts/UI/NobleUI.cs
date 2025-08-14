using UnityEngine;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 贵族界面
    /// </summary>
    public class NobleUI : MonoBehaviour
    {
        [Header("贵族UI配置")]
        public bool enableDebugLog = true;
        
        /// <summary>
        /// 更新贵族显示
        /// </summary>
        public virtual void UpdateNobles(object nobleData)
        {
        }
        
        /// <summary>
        /// 显示贵族信息
        /// </summary>
        public virtual void ShowNobleInfo(object nobleData)
        {
        }
        
        /// <summary>
        /// 隐藏贵族信息
        /// </summary>
        public virtual void HideNobleInfo()
        {
        }
        
        /// <summary>
        /// 高亮贵族
        /// </summary>
        public virtual void HighlightNoble(object nobleData)
        {
        }
        
        /// <summary>
        /// 取消高亮贵族
        /// </summary>
        public virtual void UnhighlightNoble(object nobleData)
        {
        }
        
        /// <summary>
        /// 显示贵族访问动画
        /// </summary>
        public virtual void ShowNobleVisitAnimation(object nobleData)
        {
        }
    }
}