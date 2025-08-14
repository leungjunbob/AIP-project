using UnityEngine;
using SplendorUnity.Core;
using SplendorUnity.AI;
using SplendorUnity.Models;

namespace SplendorUnity.Core
{
    /// <summary>
    /// 测试场景设置 - 点击run就能自动开始两个agents的对战
    /// 这是场景的唯一启动入口，简化了启动逻辑
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        [Header("自动对战设置")]
        public bool autoStartBattle = true;
        public float startDelay = 1.0f; // 启动延迟时间（秒）
        public float agentActionDelay = 0.1f; // 每个代理的操作延迟时间（秒）
        public double timeLimit = 30.0; // 每回合时间限制（秒）
        
        [Header("AI代理设置")]
        public BaseAgent agent1;
        public BaseAgent agent2;
        
        [Header("组件引用")]
        public GameManager gameManager;
        public GameStateListener gameStateListener;
        
        private bool battleStarted = false;
        
        private void Awake()
        {
            Debug.Log("TestSceneSetup: 场景开始，等待GameStartPage的Start Game按钮点击...");
            
            // 不再立即设置代理，等待用户点击Start Game按钮
            // SetupAIAgents();
        }
        
        private void Start()
        {
            // 不再自动启动，等待GameStartPage的Start Game按钮
            // if (autoStartBattle)
            // {
            //     Invoke(nameof(StartAutoBattle), startDelay);
            // }
            
            Debug.Log("TestSceneSetup: 等待GameStartPage的Start Game按钮点击...");
        }
        
        /// <summary>
        /// 开始自动对战
        /// </summary>
        private void StartAutoBattle()
        {
            if (battleStarted)
            {
                return;
            }
                        
            // 1. 确保必要的组件存在
            EnsureRequiredComponents();
            
            // 2. 设置玩家对战Random Agent
            SetupAIAgents();
            
            // 3. 开始游戏
            StartGame();
            
            battleStarted = true;
        }
        
        /// <summary>
        /// 确保必要的组件存在
        /// </summary>
        private void EnsureRequiredComponents()
        {            
            // 确保有GameManager
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                if (gameManager == null)
                {
                    CreateGameManager();
                }
            }
            
            // 确保有GameStateListener
            if (gameStateListener == null)
            {
                gameStateListener = FindObjectOfType<GameStateListener>();
                if (gameStateListener == null)
                {
                    CreateGameStateListener();
                }
            }
            
            // 不再设置agents，由GameStartPage管理
            
        }
        
        /// <summary>
        /// 设置AI代理（已禁用，由GameStartPage管理）
        /// </summary>
        public void SetupAIAgents()
        {            
            // 此方法已禁用，agents现在由GameStartPage管理
            Debug.Log("TestSceneSetup: SetupAIAgents已禁用，agents现在由GameStartPage管理");
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            
            if (gameManager != null)
            {
                // 设置代理操作延迟
                gameManager.agentActionDelay = agentActionDelay;
                
                // 设置回合时间限制
                gameManager.timeLimit = timeLimit;
                
                // 开始游戏
                gameManager.StartGame();
            }
        }
        
        /// <summary>
        /// 创建GameManager
        /// </summary>
        private void CreateGameManager()
        {
            var managerGO = new GameObject("GameManager");
            managerGO.transform.SetParent(transform);
            
            gameManager = managerGO.AddComponent<GameManager>();
            gameManager.agentActionDelay = agentActionDelay;
            gameManager.timeLimit = timeLimit;
            
        }
        
        /// <summary>
        /// 创建GameStateListener
        /// </summary>
        private void CreateGameStateListener()
        {
            var listenerGO = new GameObject("GameStateListener");
            listenerGO.transform.SetParent(transform);
            
            gameStateListener = listenerGO.AddComponent<GameStateListener>();
            gameStateListener.updateOnGameEvents = true;
            gameStateListener.enableDebugLog = true;
        }
        

        
    }
}
