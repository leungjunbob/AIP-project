using System;
using System.Collections.Generic;
using System.Linq;
using Splendor;

namespace A3Wam85
{
    public class SimpleDisplayer : Displayer
    {
        private StreamWriter logWriter;
        private string logFileName;

        public SimpleDisplayer()
        {
            logFileName = $"myteam_vs_bfs_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            logWriter = new StreamWriter(logFileName, false, System.Text.Encoding.UTF8);
        }

        private void LogMessage(string message)
        {
            logWriter.WriteLine(message);
            logWriter.Flush();
        }

        public override void StartRound(GameState gameState)
        {
            var state = (SplendorState)gameState;
            LogMessage($"\n=== 回合开始 ===");
            LogMessage($"当前玩家: Agent {state.AgentToMove}");
            LogMessage($"游戏状态:");
            LogMessage(state.ToString());
        }

        public override void ExecuteAction(int agentId, object action, GameState gameState)
        {
            var splendorAction = (Splendor.Action)action;
            LogMessage($"\nAgent {agentId} 执行动作: {SplendorUtils.ActionToString(agentId, splendorAction)}");
        }

        public override void TimeOutWarning(Runner runner, int id)
        {
            LogMessage($"\n⚠️ Agent {id} 超时警告！");
        }

        public override void IllegalWarning(Runner runner, int id)
        {
            LogMessage($"\n❌ Agent {id} 非法动作警告！");
        }

        public override void EndGame(GameState gameState, List<double> scores)
        {
            LogMessage("\n=== 游戏结束 ===");
            for (int i = 0; i < scores.Count; i++)
            {
                LogMessage($"Agent {i} 最终分数: {scores[i]}");
            }
            
            if (scores[0] > scores[1])
                LogMessage("🏆 MyTeamAgent 获胜！");
            else if (scores[1] > scores[0])
                LogMessage("🏆 BfsLAgent 获胜！");
            else
                LogMessage("🤝 平局！");
            
            logWriter.Close();
            Console.WriteLine($"游戏日志已保存到: {logFileName}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== MyTeamAgent vs BfsLAgent 对战测试 ===");
            
            // 创建游戏规则
            var gameRule = new SplendorGameRule(2);
            
            // 创建 Agent
            var myTeamAgent = new agents.t_085.MyTeamAgent(0);
            var bfsAgent = new agents.t_085.otherAgents.BfsLAgent(1);
            
            var agents = new List<dynamic> { myTeamAgent, bfsAgent };
            var agentNames = new List<string> { "MyTeamAgent", "BfsLAgent" };
            
            // 创建简单的显示器
            var simpleDisplayer = new SimpleDisplayer();
            
            // 创建游戏
            var game = new Game(gameRule, agents, 2, seed: 42, time_limit: 1.0, warning_limit: 3, displayer: simpleDisplayer, agents_namelist: agentNames);
            
            // 运行游戏
            var result = game.Run();
            
            // 显示结果
            Console.WriteLine("\n=== 游戏结果 ===");
            var scores = (Dictionary<int, double>)result["scores"];
            Console.WriteLine($"MyTeamAgent (Agent 0) 分数: {scores[0]}");
            Console.WriteLine($"BfsLAgent (Agent 1) 分数: {scores[1]}");
            
            if (scores[0] > scores[1])
                Console.WriteLine("MyTeamAgent 获胜！");
            else if (scores[1] > scores[0])
                Console.WriteLine("BfsLAgent 获胜！");
            else
                Console.WriteLine("平局！");
                
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
} 