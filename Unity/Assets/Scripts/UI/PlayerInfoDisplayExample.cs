using UnityEngine;
using SplendorUnity.UI;
using SplendorUnity.Core;
using SplendorUnity.Models;

namespace SplendorUnity.Examples
{
    /// <summary>
    /// PlayerInfoDisplay示例 - 展示如何设置两个玩家的信息显示区域
    /// </summary>
    public class PlayerInfoDisplayExample : MonoBehaviour
    {
        [Header("Player 1 Settings")]
        public PlayerInfoDisplaySetup player1Setup;
        public Vector2 player1Position = new Vector2(25, -102.5f);
        
        [Header("Player 2 Settings")]
        public PlayerInfoDisplaySetup player2Setup;
        public Vector2 player2Position = new Vector2(25, -512.5f);
        
        [Header("Player 3 Settings")]
        public PlayerInfoDisplaySetup player3Setup;
        public Vector2 player3Position = new Vector2(25, -307.5f);
        
        [Header("Player 4 Settings")]
        public PlayerInfoDisplaySetup player4Setup;
        public Vector2 player4Position = new Vector2(25, -717.5f);
        
        [Header("Auto Setup")]
        public bool autoSetupOnStart = true;
        
        private void Start()
        {
        }
        
        /// <summary>
        /// 设置玩家信息显示区域
        /// </summary>
        [ContextMenu("Setup Player Info Displays")]
        public void SetupPlayerInfoDisplays()
        {
            
            // 检测当前游戏中的玩家数量
            int playerCount = GetCurrentPlayerCount();
            
            // 设置玩家1
            if (player1Setup == null)
            {
                player1Setup = gameObject.AddComponent<PlayerInfoDisplaySetup>();
            }
            
            player1Setup.playerName = "Player 1";
            player1Setup.playerId = 0;
            player1Setup.displayPosition = player1Position;
            player1Setup.createUIOnStart = false; // 手动控制创建
            
            // 设置玩家2
            if (player2Setup == null)
            {
                player2Setup = gameObject.AddComponent<PlayerInfoDisplaySetup>();
            }
            
            player2Setup.playerName = "Player 2";
            player2Setup.playerId = 1;
            player2Setup.displayPosition = player2Position;
            player2Setup.createUIOnStart = false; // 手动控制创建
            
            // 创建UI
            player1Setup.CreatePlayerInfoDisplayUI();
            player2Setup.CreatePlayerInfoDisplayUI();
            
            // 只有当玩家数量大于2时才创建Player 3和Player 4
            if (playerCount > 2)
            {
                // 设置玩家3
                if (player3Setup == null)
                {
                    player3Setup = gameObject.AddComponent<PlayerInfoDisplaySetup>();
                }
                
                player3Setup.playerName = "Player 3";
                player3Setup.playerId = 2;
                player3Setup.displayPosition = player3Position;
                player3Setup.createUIOnStart = false; // 手动控制创建
                
                // 设置玩家4
                if (player4Setup == null)
                {
                    player4Setup = gameObject.AddComponent<PlayerInfoDisplaySetup>();
                }
                
                player4Setup.playerName = "Player 4";
                player4Setup.playerId = 3;
                player4Setup.displayPosition = player4Position;
                player4Setup.createUIOnStart = false; // 手动控制创建
                
                // 创建UI
                player3Setup.CreatePlayerInfoDisplayUI();
                player4Setup.CreatePlayerInfoDisplayUI();
                
            }
            
        }
        
        /// <summary>
        /// 重新设置玩家信息显示区域
        /// </summary>
        [ContextMenu("Recreate Player Info Displays")]
        public void RecreatePlayerInfoDisplays()
        {
            
            if (player1Setup != null)
            {
                player1Setup.RecreateUI();
            }
            
            if (player2Setup != null)
            {
                player2Setup.RecreateUI();
            }
            
            if (player3Setup != null)
            {
                player3Setup.RecreateUI();
            }
            
            if (player4Setup != null)
            {
                player4Setup.RecreateUI();
            }
        }
        
        /// <summary>
        /// 销毁玩家信息显示区域
        /// </summary>
        [ContextMenu("Destroy Player Info Displays")]
        public void DestroyPlayerInfoDisplays()
        {
            Debug.Log("PlayerInfoDisplayExample: 销毁玩家信息显示区域");
            
            if (player1Setup != null)
            {
                player1Setup.DestroyPlayerInfoDisplayUI();
            }
            
            if (player2Setup != null)
            {
                player2Setup.DestroyPlayerInfoDisplayUI();
            }
            
            if (player3Setup != null)
            {
                player3Setup.DestroyPlayerInfoDisplayUI();
            }
            
            if (player4Setup != null)
            {
                player4Setup.DestroyPlayerInfoDisplayUI();
            }
        }
        
        /// <summary>
        /// 检测当前游戏中的玩家数量
        /// </summary>
        private int GetCurrentPlayerCount()
        {
            // 尝试从GameManager获取玩家数量
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.agents != null)
            {
                return gameManager.agents.Count;
            }
            
            // 尝试从SplendorGameState获取玩家数量
            var gameState = FindObjectOfType<SplendorGameState>();
            if (gameState != null && gameState.agents != null)
            {
                return gameState.agents.Count;
            }
            
            // 默认返回2个玩家
            return 2;
        }
        
    }
}
