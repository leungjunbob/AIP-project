using UnityEngine;

namespace SplendorUnity.UI
{
    /// <summary>
    /// 玩家界面
    /// </summary>
    public class PlayerUI : MonoBehaviour
    {
        
        /// <summary>
        /// 更新玩家显示
        /// </summary>
        public virtual void UpdatePlayer(int playerId, object playerData)
        {
        }
        
        /// <summary>
        /// 显示玩家信息
        /// </summary>
        public virtual void ShowPlayerInfo(int playerId)
        {
        }
        
        /// <summary>
        /// 隐藏玩家信息
        /// </summary>
        public virtual void HidePlayerInfo(int playerId)
        {
        }
        
        /// <summary>
        /// 高亮当前玩家
        /// </summary>
        public virtual void HighlightCurrentPlayer(int playerId)
        {
        }
        
        /// <summary>
        /// 取消高亮玩家
        /// </summary>
        public virtual void UnhighlightPlayer(int playerId)
        {
        }
        
        /// <summary>
        /// 更新玩家分数
        /// </summary>
        public virtual void UpdatePlayerScore(int playerId, double score)
        {
        }
        
        /// <summary>
        /// 更新玩家卡牌
        /// </summary>
        public virtual void UpdatePlayerCards(int playerId, object cardsData)
        {
        }
        
        /// <summary>
        /// 更新玩家宝石
        /// </summary>
        public virtual void UpdatePlayerGems(int playerId, object gemsData)
        {
        }
    }
}