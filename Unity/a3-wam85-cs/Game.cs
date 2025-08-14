// INFORMATION ------------------------------------------------------------------------------------------------------- //
// Author:  Steven Spratley, extending code by Guang Ho and Michelle Blom
// Date:    04/01/2021
// Purpose: Implements a Game class to run implemented games for this framework.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Splendor
{
    // 常量
    public static class GameConstants
    {
        public const bool FREEDOM = false; // 是否惩罚超时/非法动作，调试用
        public const int WARMUP = 15;      // 首次行动的预热时间
    }

    // Game 类
    public class Game : Runner
    {
        private int seed;
        private List<int> seed_list;
        private int seed_idx;
        private dynamic game_rule;
        private dynamic gamemaster;
        private List<dynamic> agents;
        private List<string> agents_namelist;
        private double time_limit;
        private int warning_limit;
        private List<int> warnings;
        private List<(int, int)> warning_positions;
        private dynamic? displayer = null;
        private bool interactive;
        private Func<dynamic, List<dynamic>, bool>? valid_action;

        public Game(dynamic GameRule,
                    List<dynamic> agent_list,
                    int num_of_agent,
                    int seed = 1,
                    double time_limit = 1,
                    int warning_limit = 3,
                    dynamic? displayer = null,
                    List<string>? agents_namelist = null,
                    bool interactive = false) : base()
        {
            this.seed = seed;
            var rand = new Random(seed);
            this.seed_list = new List<int>();
            for (int i = 0; i < 1000; i++)
                this.seed_list.Add(rand.Next(0, unchecked((int)1e10)));
            this.seed_idx = 0;

            // 检查 agent id 是否连续
            for (int i = 0; i < agent_list.Count; i++)
            {
                // Debug.Assert(agent_list[i].id == i);
                if (agent_list[i].Id != i)
                {
                    Console.WriteLine($"警告: Agent {i} 的ID不匹配，期望 {i}，实际 {agent_list[i].Id}");
                }
            }

            this.game_rule = Activator.CreateInstance(GameRule.GetType(), num_of_agent);
            this.gamemaster = new DummyAgent(num_of_agent);

            // 设置基类属性
            this.Agents = agent_list.Cast<Agent>().ToList();
            this.AgentsNamelist = agents_namelist ?? new List<string> { "Alice", "Bob" };
            this.GameRule = this.game_rule;
            this.WarningLimit = warning_limit;
            this.Warnings = new Dictionary<int, int>();
            for (int i = 0; i < agent_list.Count; i++)
            {
                this.Warnings[i] = 0;
            }

            // 处理 validAction
            var type = this.game_rule.GetType();
            var method = type.GetMethod("validAction");
            if (method != null)
                this.valid_action = (a, b) => (bool)method.Invoke(this.game_rule, new object[] { a, b });
            else
                this.valid_action = null;

            this.agents = agent_list;
            this.agents_namelist = agents_namelist ?? new List<string> { "Alice", "Bob" };
            this.time_limit = time_limit;
            this.warning_limit = warning_limit;
            this.warnings = Enumerable.Repeat(0, agent_list.Count).ToList();
            this.warning_positions = new List<(int, int)>();
            this.displayer = displayer;
            if (this.displayer != null)
            {
                this.displayer.InitDisplayer(this);
            }
            this.interactive = interactive;
        }

        private Dictionary<string, object> _EndGame(int num_of_agent, Dictionary<string, object> history, bool isTimeOut = true, int? id = null)
        {
            history["seed"] = this.seed;
            history["num_of_agent"] = num_of_agent;
            history["agents_namelist"] = this.agents_namelist;
            history["warning_positions"] = this.warning_positions;
            history["warning_limit"] = this.warning_limit;
            var scores = new Dictionary<int, double>();
            for (int i = 0; i < num_of_agent; i++)
            {
                scores[i] = 0;
            }
            if (isTimeOut)
            {
                scores[id.Value] = -1;
            }
            else
            {
                for (int i = 0; i < num_of_agent; i++)
                {
                    scores[i] = this.game_rule.CalScore(this.game_rule.CurrentGameState, i);
                }
            }
            history["scores"] = scores;

            if (this.displayer != null)
            {
                this.displayer.EndGame(this.game_rule.CurrentGameState, scores.Values.ToList());
            }
            return history;
        }

        public Dictionary<string, object> Run()
        {
            var history = new Dictionary<string, object> { ["actions"] = new List<Dictionary<string, object>>() };
            int action_counter = 0;
            
            while (!this.game_rule.GameEnds())
            {
                // 每轮开始时调用 StartRound
                if (this.displayer != null)
                {
                    this.displayer.StartRound(this.game_rule.CurrentGameState);
                }
                
                int agent_index = this.game_rule.GetCurrentAgentIndex();
                dynamic agent = agent_index < this.agents.Count ? this.agents[agent_index] : this.gamemaster;
                dynamic game_state = this.game_rule.CurrentGameState;
                
                // 设置当前玩家（如果游戏状态支持的话）
                // 暂时跳过特定游戏状态的设置
                var actions = this.game_rule.GetLegalActions(game_state, agent_index);
                var actions_copy = (List<dynamic>)DeepCopy(actions);
                var gs_copy = (dynamic)DeepCopy(game_state);

                // 非完全信息游戏，删除指定属性
                if (this.game_rule.PrivateInformation != null)
                {
                    // 对于Splendor游戏，暂时跳过私有信息处理
                    // gs_copy.deck.GetType().GetProperty("cards")?.SetValue(gs_copy.deck, null);
                    for (int i = 0; i < gs_copy.Agents.Count; i++)
                    {
                        if (gs_copy.Agents[i].Id != agent_index)
                        {
                            foreach (var attr in this.game_rule.PrivateInformation)
                            {
                                gs_copy.Agents[i].GetType().GetProperty(attr)?.SetValue(gs_copy.Agents[i], null);
                            }
                        }
                    }
                }

                // 首次行动动画
                if (action_counter == 0 && this.displayer != null)
                {
                    this.displayer.DisplayState(this.game_rule.CurrentGameState);
                }

                object selected = null;
                if (this.interactive && agent_index == 1)
                {
                    this.displayer.DisplayState(this.game_rule.CurrentGameState);
                    selected = this.displayer.UserInput(actions_copy);
                }
                else
                {
                    // 由于 GameConstants.FREEDOM 为 false，直接进入超时检查逻辑
                    // if (GameConstants.FREEDOM)
                    // {
                    //     selected = agent.SelectAction(actions_copy, gs_copy);
                    // }
                    // else
                    {
                        try
                        {
                            // 超时机制（简化为 Task）
                            var task = Task.Run(() => agent.SelectAction(actions_copy, gs_copy));
                            if (!task.Wait(TimeSpan.FromSeconds(action_counter < this.agents.Count ? GameConstants.WARMUP : this.time_limit)))
                                throw new TimeoutException();
                            selected = task.Result;
                        }
                        catch
                        {
                            selected = "timeout";
                        }

                        // 验证动作的有效性
                        if (!selected.Equals("timeout"))
                        {
                            if (this.valid_action != null)
                            {
                                if (!this.valid_action(selected, actions))
                                    selected = "illegal";
                            }
                            else 
                            {
                                // 检查selected是否在actions列表中
                                bool found = false;
                                foreach (var action in actions)
                                {
                                    if (action.Equals(selected))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    selected = "illegal";
                                }
                            }
                        }
                        if (selected.Equals("timeout") || selected.Equals("illegal"))
                        {
                            this.warnings[agent_index] += 1;
                            this.warning_positions.Add((agent_index, action_counter));
                            if (this.displayer != null)
                            {
                                if (selected.Equals("timeout"))
                                    this.displayer.TimeOutWarning(this, agent_index);
                                else
                                    this.displayer.IllegalWarning(this, agent_index);
                            }
                            var rand = new Random();
                            selected = actions[rand.Next(actions.Count)];
                        }
                    }
                }

                var randSeed = new Random(this.seed_list[this.seed_idx]);
                this.seed_idx += 1;
                ((List<Dictionary<string, object>>)history["actions"]).Add(new Dictionary<string, object>
                {
                    [action_counter.ToString()] = new Dictionary<string, object>
                    {
                        ["agent_id"] = this.game_rule.CurrentAgentIndex,
                        ["action"] = selected
                    }
                });
                action_counter += 1;

                this.game_rule.Update(selected);
                randSeed = new Random(this.seed_list[this.seed_idx]);
                this.seed_idx += 1;

                if (this.displayer != null)
                {
                    this.displayer.DisplayAvailableActions(agent_index, actions_copy);
                    this.displayer.ExecuteAction(agent_index, selected, this.game_rule.CurrentGameState);
                }

                if ((agent_index != this.game_rule.NumOfAgent) && (this.warnings[agent_index] == this.warning_limit))
                {
                    history = this._EndGame(this.game_rule.NumOfAgent, history, isTimeOut: true, id: agent_index);
                    return history;
                }
            }

            // 计算代理分数
            return this._EndGame(this.game_rule.NumOfAgent, history, isTimeOut: false);
        }

        private object DeepCopy(object obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();

            // 基本类型和字符串直接返回
            if (type.IsPrimitive || type == typeof(string))
                return obj;

            // List
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var newList = (System.Collections.IList)Activator.CreateInstance(type);
                foreach (var item in (System.Collections.IEnumerable)obj)
                {
                    newList.Add(DeepCopy(item));
                }
                return newList;
            }

            // Dictionary
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var newDict = (System.Collections.IDictionary)Activator.CreateInstance(type);
                foreach (System.Collections.DictionaryEntry kvp in (System.Collections.IDictionary)obj)
                {
                    newDict.Add(DeepCopy(kvp.Key), DeepCopy(kvp.Value));
                }
                return newDict;
            }

            // Array
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType != null)
                {
                    var array = (Array)obj;
                    var newArray = Array.CreateInstance(elementType, array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        var value = array.GetValue(i);
                        if (value != null)
                        {
                            newArray.SetValue(DeepCopy(value), i);
                        }
                    }
                    return newArray;
                }
            }

            // 对于SplendorState，使用其DeepCopy方法
            if (type == typeof(SplendorState))
            {
                return ((SplendorState)obj).DeepCopy();
            }

            // 对于Action，使用其DeepCopy方法
            if (type == typeof(Action))
            {
                return ((Action)obj).DeepCopy();
            }

            // 对于其他复杂对象，暂时返回原对象（浅复制）
            // 在实际使用中，这应该根据具体类型实现相应的DeepCopy
            return obj;
        }
    }

    public class GameReplayer
    {
        private dynamic replay;
        private int seed;
        private List<int> seed_list;
        private int seed_idx;
        private int num_of_agent;
        private List<string> agents_namelist;
        private int warning_limit;
        private List<int> warnings;
        private List<(int, int)> warning_positions;
        private dynamic game_rule;
        private Dictionary<int, double> scores;
        private dynamic displayer;

        public GameReplayer(dynamic GameRule, dynamic replay, dynamic? displayer = null)
        {
            this.replay = replay;

            this.seed = this.replay["seed"];
            var rand = new Random(this.seed);
            this.seed_list = new List<int>();
            for (int i = 0; i < 1000; i++)
                this.seed_list.Add(rand.Next(0, unchecked((int)1e10)));
            this.seed_idx = 0;

            this.num_of_agent = this.replay["num_of_agent"];
            this.agents_namelist = this.replay["agents_namelist"];
            this.warning_limit = this.replay["warning_limit"];
            this.warnings = Enumerable.Repeat(0, this.num_of_agent).ToList();
            this.warning_positions = this.replay["warning_positions"];
            this.game_rule = GameRule(this.num_of_agent);
            this.scores = this.replay["scores"];

            this.displayer = displayer;
            if (this.displayer != null)
            {
                this.displayer.InitDisplayer(this);
            }
        }

        public void Run()
        {
            foreach (var item in this.replay["actions"])
            {
                // 处理嵌套的字典结构
                var actionInfo = item.Values.First() as Dictionary<string, object>;
                var selected = actionInfo["action"];
                int agent_index = (int)actionInfo["agent_id"];
                this.game_rule.CurrentAgentIndex = agent_index;

                var rand = new Random(this.seed_list[this.seed_idx]);
                this.seed_idx += 1;
                this.game_rule.Update(selected);
                rand = new Random(this.seed_list[this.seed_idx]);
                this.seed_idx += 1;
                if (this.displayer != null)
                {
                    // 检查警告位置
                    var actionIndex = int.Parse(item.Keys.First());
                    if (this.warning_positions.Contains((agent_index, actionIndex)))
                    {
                        this.warnings[agent_index] += 1;
                        this.displayer.TimeOutWarning(this, agent_index);
                    }
                    this.displayer.ExecuteAction(agent_index, selected, this.game_rule.CurrentGameState);
                }
            }

            if (this.displayer != null)
            {
                this.displayer.EndGame(this.game_rule.CurrentGameState, this.scores.Values.ToList());
            }
        }
    }

    public class DummyAgent : Agent
    {
        public DummyAgent(int id) : base(id)
        {
        }

        public override object SelectAction(List<object> actions, GameState game_state)
        {
            var rand = new Random();
            return actions[rand.Next(actions.Count)];
        }
    }
} 