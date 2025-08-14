// Template.cs
// 完整翻译自 a3-wam85/template.py
using System;
using System.Collections.Generic;

namespace Splendor
{
    public class GameState
    {
        public GameState(int num_of_agent, int agent_id)
        {
            // 这里可根据实际需求实现
        }
        
        public GameState()
        {
            // 默认构造函数
        }
    }

    public class GameRule
    {
        public int CurrentAgentIndex { get; set; }
        public int NumOfAgent { get; set; }
        public GameState CurrentGameState { get; set; }
        public int ActionCounter { get; set; }
        public object? PrivateInformation { get; set; }

        public GameRule(int num_of_agent = 2)
        {
            this.CurrentAgentIndex = 0;
            this.NumOfAgent = num_of_agent;
            this.CurrentGameState = InitialGameState();
            this.ActionCounter = 0;
        }

        public virtual GameState InitialGameState()
        {
            Utils.RaiseNotDefined();
            return new GameState(2, 0);
        }

        public virtual GameState GenerateSuccessor(GameState game_state, object action, int agent_id)
        {
            Utils.RaiseNotDefined();
            return game_state;
        }

        public int GetNextAgentIndex()
        {
            return (this.CurrentAgentIndex + 1) % this.NumOfAgent;
        }

        public virtual List<object> GetLegalActions(GameState game_state, int agent_id)
        {
            Utils.RaiseNotDefined();
            return new List<object>();
        }

        public virtual double CalScore(GameState game_state, int agent_id)
        {
            Utils.RaiseNotDefined();
            return 0.0;
        }

        public virtual bool GameEnds()
        {
            Utils.RaiseNotDefined();
            return false;
        }

        public void Update(object action)
        {
            var temp_state = this.CurrentGameState;
            this.CurrentGameState = GenerateSuccessor(temp_state, action, this.CurrentAgentIndex);
            this.CurrentAgentIndex = GetNextAgentIndex();
            this.ActionCounter += 1;
        }

        public int GetCurrentAgentIndex()
        {
            return this.CurrentAgentIndex;
        }
    }

    public class Agent
    {
        public int Id { get; set; }
        
        public Agent(int id)
        {
            this.Id = id;
        }
        
        // 给定可用动作和当前游戏状态，选择一个动作
        public virtual object? SelectAction(List<object> actions, GameState game_state)
        {
            var rand = new Random();
            return actions[rand.Next(actions.Count)];
        }
    }

    public class Displayer
    {
        public Displayer() { }
        
        // 首次显示
        public virtual void InitDisplayer(Runner runner) { }
        
        public virtual void StartRound(GameState gameState) { }
        
        public virtual void DisplayState(GameState game_state) { }
        
        public virtual void DisplayAvailableActions(int agentId, List<object> actions) { }
        
        public virtual void ExecuteAction(int i, object move, GameState game_state) 
        {
            Utils.RaiseNotDefined();
        }
        
        public virtual object? UserInput(List<object> actions) 
        {
            return actions.Count > 0 ? actions[0] : null;
        }
        
        public virtual void TimeOutWarning(Runner runner, int id) 
        {
            Utils.RaiseNotDefined();
        }
        
        public virtual void IllegalWarning(Runner runner, int id) 
        {
            Utils.RaiseNotDefined();
        }
        
        public virtual void EndGame(GameState game_state, List<double> scores) 
        {
            Utils.RaiseNotDefined();
        }
    }

    public class Runner
    {
        public List<Agent> Agents { get; set; }
        public List<string> AgentsNamelist { get; set; }
        public GameRule? GameRule { get; set; }
        public Dictionary<int, int> Warnings { get; set; }
        public int WarningLimit { get; set; }

        public Runner()
        {
            Agents = new List<Agent>();
            AgentsNamelist = new List<string>();
            Warnings = new Dictionary<int, int>();
            WarningLimit = 3;
        }
    }
} 