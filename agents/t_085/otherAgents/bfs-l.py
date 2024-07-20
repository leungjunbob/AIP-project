from template import Agent
from Splendor.splendor_utils import *
import random
from copy import deepcopy
import time
from collections import deque
from Splendor.splendor_model import SplendorGameRule

Max_Time = 0.9


class myAgent(object):
    def __init__(self, _id):
        self.id = _id
        self.game_rule = SplendorGameRule(2)
        super().__init__()

    # Given a set of available actions for the agent to execute, and
    # a copy of the current game state (including that of the agent),
    # select one of the actions to execute. 
    def ifCanBuy(self, game_state):
        actions = self.game_rule.getLegalActions(game_state, self.id)
        for action in actions:
            if action["type"] == 'buy_available':
                return True
        return False
    
    def gemAction(self, game_state):
        actions = self.game_rule.getLegalActions(game_state, self.id)
        filter_gem_actions = []
        gem_actions = [action for action in actions if action["type"] == 'collect_diff' or action["type"] == 'collectd_gems']
        if (gem_actions):
            for action in gem_actions:
                next_state = self.game_rule.generateSuccessor(deepcopy(game_state), action, self.id)
                if(self.ifCanBuy(next_state)):
                    filter_gem_actions.append(action)
            if (filter_gem_actions):
                return filter_gem_actions, "gem"
            else:
                return gem_actions, "gem"     
               
    # colour = ['black', 'red', 'yellow', 'green', 'blue', 'white']
    def greedyAction(self, game_state, actions):
        reserve_actions = [action for action in actions if action["type"] == 'reserve']
        total_gem = sum(game_state.agents[self.id].gems.values())
        buy_actions = [action for action in actions if action["type"] == 'buy_available' or action["type"] == 'buy_reserve']
        if buy_actions:
            return buy_actions, "buy"
        # print(game_state.board.dealt)
        if (total_gem <= 8):
            return self.gemAction(game_state)
        else:
            opponent_actions = self.game_rule.getLegalActions(game_state, (self.id+1) % 2)
            opponent_buy_actions = [action for action in opponent_actions if action["type"] == 'buy_available']
            if reserve_actions and len(opponent_buy_actions) == 1:
                res = []
                for action in reserve_actions:
                    if action["card"] == opponent_buy_actions[0]["card"]:
                        res.append(action)
                        return res, "reserve"
        if(self.gemAction(game_state)):
            return self.gemAction(game_state)
        elif(reserve_actions):
            return reserve_actions, "reserve"
        elif(len(actions) == 1):
            return actions, "pass"

    def SelectAction(self, actions, game_state):
        startTime = time.time()
        path = []
        new_actions = []
        queue = deque([(deepcopy(game_state), path)])
        originScore = self.game_rule.calScore(game_state, self.id)
        while(len(queue) > 0 and time.time() - startTime < Max_Time):
            state, path = queue.popleft()
            score = self.game_rule.calScore(state, self.id)
            
            if score >= 15 or score - originScore >= 3:
                return path[0]
            selectd_action = self.greedyAction(state, self.game_rule.getLegalActions(state, self.id))

            new_actions, actionType = selectd_action
            self.operateAction(new_actions, state, queue, path, actionType)
                                         
        return path[0] if path else random.choice(new_actions)
    
    def operateAction(self, actions, state, queue, path, actionType):
        if actionType == "buy":
            actions.sort(key=lambda x: x['card'].points, reverse = True)
        elif actionType == "gem":
            random.shuffle(actions)
        elif actionType == "reserve":
            actions = [actions[0]]
        for action in actions:
            next_state = self.game_rule.generateSuccessor(deepcopy(state), action, self.id)
            next_path = path + [action]
            queue.append((next_state, next_path))
            return
        