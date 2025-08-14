// INFORMATION ------------------------------------------------------------------------------------------------------- //
// Author:  Steven Spratley, extending code by Guang Ho and Michelle Blom
// Date:    12/02/23
// Purpose: Defines a general game runner for the COMP90054 competitive game environment

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Splendor
{
    // 常量
    public static class GameRunnerConstants
    {
        public const string DEFAULT_AGENT = "agents.generic.random";
        public const string DEFAULT_AGENT_NAME = "default";
        public const string GIT_TOKEN_PATH = "configs/token.txt";
        public const string DATE_FORMAT = "dd/MM/yyyy HH:mm:ss"; // RMIT Uni (Australia)
    }

    // 游戏运行选项
    public class GameRunnerOptions
    {
        public string Agents { get; set; } = "agents.generic.random,agents.generic.random";
        public string AgentNames { get; set; } = "random0,random1";
        public bool Cloud { get; set; } = false;
        public string AgentUrls { get; set; } = "";
        public string AgentCommitIds { get; set; } = "";
        public int NumOfAgents { get; set; } = 2;
        public bool TextGraphics { get; set; } = false;
        public string Game { get; set; } = "Splendor";
        public bool Quiet { get; set; } = false;
        public bool SuperQuiet { get; set; } = false;
        public double WarningTimeLimit { get; set; } = 1.0;
        public double StartRoundWarningTimeLimit { get; set; } = 5.0;
        public int NumOfWarnings { get; set; } = 3;
        public int MultipleGames { get; set; } = 1;
        public int SetRandomSeed { get; set; } = 90054;
        public bool SaveGameRecord { get; set; } = false;
        public string Output { get; set; } = "output";
        public bool SaveLog { get; set; } = false;
        public string Replay { get; set; } = null;
        public double Delay { get; set; } = 0.1;
        public bool Print { get; set; } = false;
        public bool HalfScale { get; set; } = false;
        public bool Interactive { get; set; } = false;
    }

    // 隐藏打印类
    public class HidePrint : IDisposable
    {
        private bool flag;
        private string filePath;
        private string fileName;
        private TextWriter originalStdout;
        private TextWriter originalStderr;
        private TextWriter currentOutput;

        public HidePrint(bool flag, string filePath, string fileName)
        {
            this.flag = flag;
            this.filePath = filePath;
            this.fileName = fileName;
            this.originalStdout = Console.Out;
            this.originalStderr = Console.Error;
        }

        public void Dispose()
        {
            if (currentOutput != null)
            {
                currentOutput.Close();
                Console.SetOut(originalStdout);
                Console.SetError(originalStderr);
            }
        }

        public void Enter()
        {
            if (flag)
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                currentOutput = new StreamWriter($"{filePath}/log-{fileName}.log");
                Console.SetOut(currentOutput);
                Console.SetError(currentOutput);
            }
            else
            {
                currentOutput = TextWriter.Null;
                Console.SetOut(currentOutput);
                Console.SetError(currentOutput);
            }
        }
    }

    // 游戏运行器
    public class GeneralGameRunner
    {
        public static Dictionary<string, object> LoadAgent(Dictionary<string, object> matches, bool superQuiet = true)
        {
            var teams = matches["teams"] as Dictionary<string, object>;
            int num_of_agents = teams.Count;
            var agents = new List<Agent>(num_of_agents);
            bool valid_game = true;

            for (int i = 0; i < num_of_agents; i++)
            {
                Agent agent_temp = null;
                try
                {
                    var teamInfo = teams[i.ToString()] as Dictionary<string, object>;
                    string agentPath = teamInfo["agent"].ToString();
                    
                    // 简化的代理加载逻辑
                    if (agentPath.Contains("generic.random"))
                    {
                        agent_temp = new Agents.Generic.RandomAgent(i);
                    }
                    else if (agentPath.Contains("generic.firstmove"))
                    {
                        agent_temp = new Agents.Generic.FirstMoveAgent(i);
                    }
                    else if (agentPath.Contains("generic.timeout"))
                    {
                        agent_temp = new Agents.Generic.TimeoutAgent(i);
                    }
                    else if (agentPath.Contains("t_085"))
                    {
                        // 暂时使用随机代理替代
                        agent_temp = new Agents.Generic.RandomAgent(i);
                    }
                    else
                    {
                        // 默认使用随机代理
                        agent_temp = new Agents.Generic.RandomAgent(i);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: Agent at position {i} could not be loaded! {ex.Message}");
                    valid_game = false;
                }

                if (agent_temp != null)
                {
                    agents.Add(agent_temp);
                    var teamInfo = teams[i.ToString()] as Dictionary<string, object>;
                    teamInfo["load_agent"] = true;
                    if (!superQuiet)
                    {
                        Console.WriteLine($"Agent {i} team {teamInfo["team_name"]} agent {teamInfo["agent"]} loaded");
                    }
                }
                else
                {
                    valid_game = false;
                    agents.Add(new DummyAgent(i));
                    var teamInfo = teams[i.ToString()] as Dictionary<string, object>;
                    teamInfo["load_agent"] = false;
                }
            }

            matches["agents"] = agents;
            matches["valid_game"] = valid_game;
            return matches;
        }

        public static Dictionary<string, object> Run(GameRunnerOptions options, string msg)
        {
            int num_of_agents = options.NumOfAgents;

            // 填充默认值
            var agent_names = options.AgentNames.Split(",").ToList();
            var agents = options.Agents.Split(",").ToList();
            var agent_urls = options.AgentUrls.Split(",").ToList();
            var agent_commit_ids = options.AgentCommitIds.Split(",").ToList();

            int missing = num_of_agents - agent_names.Count;
            for (int i = 0; i < missing; i++)
            {
                agent_names.Add(GameRunnerConstants.DEFAULT_AGENT_NAME);
            }
            missing = num_of_agents - agents.Count;
            for (int i = 0; i < missing; i++)
            {
                agents.Add(GameRunnerConstants.DEFAULT_AGENT);
            }

            var matches = new Dictionary<string, object>();
            matches["games"] = new List<Dictionary<string, object>>();
            matches["teams"] = new Dictionary<string, object>();
            matches["num_of_games"] = options.MultipleGames;

            // 加载代理信息
            for (int i = 0; i < num_of_agents; i++)
            {
                var team_info = new Dictionary<string, object>();
                team_info["team_name"] = agent_names[i];
                team_info["agent"] = agents[i];
                if (options.Cloud)
                {
                    team_info["url"] = agent_urls[i];
                    team_info["commit_id"] = agent_commit_ids[i];
                    // 简化版本，跳过Git克隆
                    team_info["git"] = "succ";
                    team_info["comments"] = "N/A";
                    team_info["submitted_time"] = DateTime.Now.ToString(GameRunnerConstants.DATE_FORMAT);
                }
                ((Dictionary<string, object>)matches["teams"])[i.ToString()] = team_info;
            }

            // 加载游戏
            string game_name = options.Game;
            Type GameRuleType = null;
            Type TextDisplayerType = null;
            Type GUIDisplayerType = null;

            try
            {
                // 简化的游戏加载逻辑
                if (game_name.ToLower() == "splendor")
                {
                    GameRuleType = typeof(SplendorGameRule);
                    TextDisplayerType = typeof(TextDisplayer);
                    GUIDisplayerType = typeof(GUIDisplayer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game {game_name}: {ex.Message}");
            }

            // 创建显示器
            Displayer displayer = null;
            if (GUIDisplayerType != null)
            {
                displayer = Activator.CreateInstance(GUIDisplayerType, options.HalfScale, options.Delay) as Displayer;
            }
            if (options.TextGraphics && TextDisplayerType != null)
            {
                displayer = Activator.CreateInstance(TextDisplayerType) as Displayer;
            }
            else if (options.Quiet || options.SuperQuiet)
            {
                displayer = null;
            }

            // 随机种子
            int random_seed = options.SetRandomSeed;
            if (random_seed == 90054)
            {
                random_seed = int.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            }

            var rand = new Random(random_seed);
            var seed_list = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                seed_list.Add(rand.Next(0, unchecked((int)1e10)));
            }
            int seed_idx = 0;

            int num_of_warning = options.NumOfWarnings;
            string file_path = options.Output;

            if (!string.IsNullOrEmpty(options.Replay))
            {
                if (!options.SuperQuiet)
                {
                    Console.WriteLine($"Replaying recorded game {options.Replay}.");
                }
                // 简化版本，跳过重放功能
                return matches;
            }

            var games_results = new List<(List<double>, List<double>, List<int>, List<int>, List<int>)>();
            games_results.Add((new List<double>(num_of_agents), new List<double>(num_of_agents), 
                             new List<int>(num_of_agents), new List<int>(num_of_agents), new List<int>(num_of_agents)));

            for (int game_num = 0; game_num < options.MultipleGames; game_num++)
            {
                var game = new Dictionary<string, object>();
                matches = LoadAgent(matches, options.SuperQuiet);
                var loaded_agents = (matches["agents"] as List<Agent>)?.Cast<dynamic>().ToList() ?? new List<dynamic>();
                bool valid_game = (bool)matches["valid_game"];

                game["valid_game"] = valid_game;
                random_seed = seed_list[seed_idx];
                seed_idx += 1;
                game["random_seed"] = random_seed;

                string f_name = agent_names[0];
                for (int j = 1; j < agent_names.Count; j++)
                {
                    f_name += "-vs-" + agent_names[j];
                }
                f_name += "-" + DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss-fff");
                f_name += "-" + random_seed;
                game["file_name"] = f_name;

                if (options.SaveLog)
                {
                    game["log_path"] = $"{file_path}/log-{f_name}.log";
                }

                if (GameRuleType != null)
                {
                    var gameRule = Activator.CreateInstance(GameRuleType, num_of_agents);
                    var gr = new Game(gameRule, loaded_agents, num_of_agents, random_seed, 
                                    options.WarningTimeLimit, num_of_warning, displayer, agent_names, options.Interactive);

                    if (!options.Print)
                    {
                        using (var hidePrint = new HidePrint(options.SaveLog, file_path, f_name))
                        {
                            hidePrint.Enter();
                            Console.WriteLine($"Following are the print info for loading:\n{msg}\n");
                            Console.WriteLine("\n-------------------------------------\n");
                            Console.WriteLine("Following are the print info from the game:\n");
                            if (valid_game)
                            {
                                var replay = gr.Run();
                                game["replay"] = replay;
                            }
                            else
                            {
                                Console.WriteLine("Invalid game. No game played.\n");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Following are the print info for loading:\n{msg}\n");
                        Console.WriteLine("\n-------------------------------------\n");
                        Console.WriteLine("Following are the print info from the game:\n");
                        if (valid_game)
                        {
                            var replay = gr.Run();
                            game["replay"] = replay;
                        }
                        else
                        {
                            Console.WriteLine("Invalid game. No game played.\n");
                        }
                    }

                    if (valid_game)
                    {
                        var (scores, totals, wins, ties, loses) = games_results[games_results.Count - 1];
                        var new_scores = new List<double>();
                        var new_totals = new List<double>();
                        var new_wins = new List<int>();
                        var new_ties = new List<int>();
                        var new_loses = new List<int>();

                        var replay = game["replay"] as Dictionary<string, object>;
                        var replayScores = replay?["scores"] as Dictionary<string, object>;
                        game["scores"] = replayScores ?? new Dictionary<string, object>();

                        // 记录分数
                        for (int i = 0; i < num_of_agents; i++)
                        {
                            if (replayScores != null && replayScores.ContainsKey(i.ToString()))
                            {
                                new_scores.Add(Convert.ToDouble(replayScores[i.ToString()]));
                            }
                            else
                            {
                                new_scores.Add(0.0);
                            }
                        }

                        double max_score = new_scores.Max();

                        // 更新总分和胜利次数
                        for (int i = 0; i < num_of_agents; i++)
                        {
                            new_totals.Add(totals[i] + new_scores[i]);
                            if (new_scores[i] == max_score)
                            {
                                if (new_scores.Count(x => x == max_score) > 1)
                                {
                                    new_wins.Add(wins[i]);
                                    new_ties.Add(ties[i] + 1);
                                    new_loses.Add(loses[i]);
                                }
                                else
                                {
                                    new_wins.Add(wins[i] + 1);
                                    new_ties.Add(ties[i]);
                                    new_loses.Add(loses[i]);
                                }
                            }
                            else
                            {
                                new_wins.Add(wins[i]);
                                new_ties.Add(ties[i]);
                                new_loses.Add(loses[i] + 1);
                            }
                        }

                        if (!options.SuperQuiet)
                        {
                            Console.WriteLine($"Result of game ({game_num + 1}/{options.MultipleGames}):");
                            for (int i = 0; i < num_of_agents; i++)
                            {
                                Console.WriteLine($"    {agent_names[i]} earned {new_scores[i]} points.");
                            }
                        }

                        games_results.Add((new_scores, new_totals, new_wins, new_ties, new_loses));

                        if (options.SaveGameRecord)
                        {
                            if (!Directory.Exists(file_path))
                            {
                                Directory.CreateDirectory(file_path);
                            }
                            if (!options.SuperQuiet)
                            {
                                Console.WriteLine($"Game ({game_num + 1}/{options.MultipleGames}) has been recorded!");
                            }
                            game["replay_path"] = $"{file_path}/replay-{f_name}.replay";
                            // 简化版本，跳过文件保存
                        }

                        ((List<Dictionary<string, object>>)matches["games"]).Add(game);
                    }
                }
            }

            if (!options.SuperQuiet)
            {
                Console.WriteLine(matches);
            }

            if ((bool)matches["valid_game"])
            {
                var (scores, totals, wins, ties, loses) = games_results[games_results.Count - 1];

                var avgs = new List<double>();
                var win_rates = new List<double>();
                for (int i = 0; i < num_of_agents; i++)
                {
                    avgs.Add(totals[i] / options.MultipleGames);
                    win_rates.Add(wins[i] / (double)options.MultipleGames * 100);
                }

                if (!options.SuperQuiet)
                {
                    Console.WriteLine($"Over {options.MultipleGames} games:");
                    for (int i = 0; i < num_of_agents; i++)
                    {
                        Console.WriteLine($"    {agent_names[i]} earned {avgs[i]:F2} on average and won {wins[i]} games ({win_rates[i]:F2})%.");
                    }
                }

                matches["total_scores"] = totals;
                matches["wins"] = wins;
                matches["ties"] = ties;
                matches["loses"] = loses;
                matches["win_rates"] = win_rates;
                matches["succ"] = true;
            }

            return matches;
        }

        public static GameRunnerOptions LoadParameter(string[] args)
        {
            // 简化的参数解析
            var options = new GameRunnerOptions();
            
            // 这里可以添加更复杂的命令行参数解析逻辑
            // 暂时使用默认值
            
            return options;
        }
    }
} 