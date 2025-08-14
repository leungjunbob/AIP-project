// RandomAgent.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splendor.Agents.Generic
{
    public class RandomAgent : Agent
    {
        private Random random;

        public RandomAgent(int id) : base(id)
        {
            random = new Random();
        }

        public override object SelectAction(List<object> actions, GameState gameState)
        {
            if (actions == null || actions.Count == 0)
                return new Action("pass", new Dictionary<string, int>(), new Dictionary<string, int>());
            
            return actions[random.Next(actions.Count)];
        }
    }
} 