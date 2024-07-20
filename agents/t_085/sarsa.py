from template import Agent
from Splendor.splendor_utils import *
import random
from copy import deepcopy
import time
from collections import deque
from Splendor.splendor_model import SplendorGameRule
import json
import os

# The maximum computation time for each action is 1 second, allowing 0.02 seconds remaining
LEARNING_RATE = 0.01
REWARD_DECAY = 0.9
E_GREEDY = 0.9
MAX_TIME = 0.3
# Program mode
TRAINING = False
PATH = "agents/t_085/weight.json"
LEVEL_1_CARDS = ['1g1w1r1b', '1g1w1r2b', '2b1r2w', '2g1r', '2w2g', '3g', '3r1B1g', '4b', '1r1w1B1g', '1w2B', '2g2B', '2g2r1w', '2r1w1B1g', '3B', '3g1r1b', '4r', '1r1w1B1b',
                  '1r1w2B1b', '2b2r', '2r2B1b', '2w1b', '3b1g1w', '3r', '4B', '1g1w1B1b', '1g2B2w', '1g2w1B1b', '1r3B1w', '2b1g', '2w2r', '3w', '4w', '1b1B3w', '1r1b1B1g',
                    '1r1b1B2g', '2b2B', '2g1B2b', '2r1B', '3b', '4g']
LEVEL_2_CARDS = ['2b2g3w', '3g2B3w', '4g2r1b', '5g3r', '5w', '6B', '1r4B2w', '2g3r2b', '3g3B2b', '5b', '5w3b', '6b', '2b1B4w', '2g3r3w', '3b2B2w', '5b3g', '5g', '6g',
                  '2r3B2w', '2r3B3b', '3w5B', '4b2g1w', '5B', '6r', '2r2B3g', '3b3r2w', '4r2B1g', '5r', '5r3B', '6w']
LEVEL_3_CARDS = ['5g3w3r3b', '6r3B3g', '7r', '7r3B', '3b3B6w', '3r3w5B3g', '7w', '7w3b', '3r5w3B3b', '6b3g3w', '7b', '7b3g', '3g3w3B5b', '6g3r3b', '7g', '7g3r',
                  '3r6B3w', '3w7B', '5r3b3B3g', '7B']

class myAgent(object):
    def __init__(self, _id):
        self.id = _id
        self.game_rule = SplendorGameRule(2)
        super().__init__()
        self.lr = LEARNING_RATE
        self.gamma = REWARD_DECAY
        self.epsilon = E_GREEDY
        self.weight = self._load_weight()

    def _load_weight(self):
        if os.path.exists(PATH):
            return json.load(open(PATH, "r"))
        else:
            json.dump({}, open(PATH, "w"))

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
                return filter_gem_actions
            else:
                return gem_actions  
            
        # colour = ['black', 'red', 'yellow', 'green', 'blue', 'white']
    def greedyAction(self, game_state, actions):
        reserve_actions = [action for action in actions if action["type"] == 'reserve']
        total_gem = sum(game_state.agents[self.id].gems.values())
        buy_actions = [action for action in actions if action["type"] == 'buy_available' or action["type"] == 'buy_reserve']
        if buy_actions:
            return buy_actions
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
                        return res
        if(self.gemAction(game_state)):
            return self.gemAction(game_state)
        elif(reserve_actions):
            return reserve_actions
        elif(len(actions) == 1):
            return actions

    def SelectAction(self,actions,game_state):
        if not TRAINING:
            return self.onPolicy(actions, game_state)
        startTime = time.time()
        path = []
        new_path = self.train(game_state,startTime, path)
        return new_path[0] if new_path else random.choice(actions)
    
    def onPolicy(self, actions, game_state):
        new_actions = self.greedyAction(game_state, actions)
        best_Q = - 10000
        for action in new_actions:
            q = self.getQ(game_state, action)
            if q > best_Q:
                best_Q = q
                best_action = action
        return best_action
    
    def train(self, game_state, startTime, path):
        new_path = []
        while time.time() - startTime < MAX_TIME and self.game_rule.calScore(game_state, self.id) < 15:
            actions = self.game_rule.getLegalActions(game_state, self.id)
            if random.random() < self.epsilon:
                new_actions = self.greedyAction(game_state, actions)
                new_action = random.choice(new_actions)
                next_state = self.game_rule.generateSuccessor(deepcopy(game_state), new_action, self.id)
                reward = self.calReward(game_state, next_state, new_action)
                next_path = path + [new_action]
                self.updateAll(game_state, next_state, new_action, reward)
                new_path = self.train(next_state, startTime, next_path)
            else:
                new_action = random.choice(actions)
                next_path = path + [new_action] 
                next_state = self.game_rule.generateSuccessor(deepcopy(game_state), new_action, self.id)
                new_path = self.train(next_state, startTime, next_path)
        return new_path if new_path else path
    
    def updateAll(self, game_state, next_state, action, reward):
        actions = self.game_rule.getLegalActions(game_state, self.id)
        next_actions = self.greedyAction(game_state, actions)
        next_action = random.choice(next_actions)
        delta = self.lr * (reward + self.gamma * self.getQ (next_state, next_action) - self.getQ(game_state, action))
        for feature, feature_value in self.featureScore(game_state, action).items():
            old_weight = self.weight.get(feature, 0)
            gradient = feature_value * delta
            self.weight[feature] = old_weight + gradient
        self.lr = max(0.0001, self.lr * 0.9)
        json.dump(self.weight, open(PATH, "w"))
    
    def getQ(self, game_state, action):
        sum = 0
        for feature, feature_value in self.featureScore(game_state, action).items():
            sum += self.weight.get(feature, 0) * feature_value
        return sum
    
    def featureScore(self, game_state, action):
        feature = {}
        feature['score'] = self.game_rule.calScore(game_state, self.id) 
        feature['gems'] = sum(game_state.agents[self.id].gems.values())
        feature['noble'] = len(game_state.agents[self.id].nobles)
        feature['card_point'] = 0
        feature['card_cost'] = 0
        if 'card' in action:
            feature['card_point'] = action['card'].points
            feature['card_cost'] = - sum(action['card'].cost.values())
        count1 = 0
        count2 = 0
        count3 = 0
        for key, items in game_state.agents[self.id].cards.items():
            for card in items:
                if card.code in LEVEL_1_CARDS:
                    count1 += 1
                if card.code in LEVEL_2_CARDS:
                    count2 += 1
                if card.code in LEVEL_3_CARDS:
                    count3 += 1
        feature['level_1_cards'] = count1
        feature['level_2_cards'] = count2
        feature['level_3_cards'] = count3
        return feature
    
    #reward for different outcomes, range 0 to 1
    def calReward(self, game_state, next_state, action):
        reward = 0
        # reward for obtain nobles in one action
        reward += 1 * (len(next_state.agents[self.id].nobles) - len(game_state.agents[self.id].nobles))
        # reward for obtain a permenant gem from card
        reward += 0.5 * (len(next_state.agents[self.id].cards) - len(game_state.agents[self.id].cards))
        # reward for reducing gem
        reward += 0.1 * (sum(next_state.agents[self.id].gems.values()) - sum(game_state.agents[self.id].gems.values()))
        # reward for obtain scores
        reward += 0.3 * (self.game_rule.calScore(next_state, self.id) - self.game_rule.calScore(game_state, self.id))
        # reward for pass
        if action['type'] == 'pass':
            reward -= 1
        return reward

