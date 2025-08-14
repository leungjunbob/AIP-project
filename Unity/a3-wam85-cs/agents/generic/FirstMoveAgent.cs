// FirstMoveAgent.cs
using System;
using System.Collections.Generic;

namespace Splendor.Agents.Generic
{
    public class FirstMoveAgent : Agent
    {
        public FirstMoveAgent(int id) : base(id)
        {
        }

        public override object SelectAction(List<object> actions, GameState gameState)
        {
            if (actions == null || actions.Count == 0)
                return new Action("pass", new Dictionary<string, int>(), new Dictionary<string, int>());
            
            return actions[0];
        }
    }
} 