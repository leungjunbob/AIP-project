from numpy import sort
from template import Agent
from copy import deepcopy
from Splendor.splendor_model import SplendorGameRule
from collections import deque
import random
import time


# The maximum computation time for each action is 1 second, allowing 0.02 seconds remaining
MAX_TIME = 0.8
WIN_SCORE = 15
MAX_GEM = 10
NUM_PLAYERS = 2
# action types string
BUY_ACTION_TYPE = ["buy_available", "buy_reserve"]
RESERVE_ACTION_TYPE = "reserve"
GEM_ACTION_TYPE = ['collect_diff', 'collect_same']

class myAgent(Agent):
    def __init__(self,_id):
        super().__init__(_id)
        # By default, this competition is always a 2-player game.
        self.game_rule = SplendorGameRule(num_of_agent=NUM_PLAYERS)
    
    # This function selects the action with strategic considerations from all legal actions.
    # Strategy includes:
    # - priority of action types: 1. buy action comes first, 2. take gem, 3. reserve
    # - priority for buy action: highest points
    # - priority for gem action: highest number of gem collected
    def getStrategicActions(self, game_state):
        gemNum = sum(game_state.agents[self.id].gems.values())
        legalActions = self.game_rule.getLegalActions(game_state, self.id)
        # print("legalActions")
        # print(legalActions)
        buy_actions = [action for action in legalActions if action["type"] in BUY_ACTION_TYPE]
        # print("HERE", buy_actions)
        if buy_actions:
            buy_actions.sort(key=lambda x:x['card'].points, reverse=True)
            return buy_actions


        if gemNum != MAX_GEM:
            gem_actions = [action for action in legalActions if action["type"] in GEM_ACTION_TYPE]
            if gem_actions:
                # sort it by the number of gems collected. 
                gem_actions.sort(key=lambda action: sum(action['collected_gems'].values()), reverse=True)
                return gem_actions

        
        reserve_actions = [action for action in legalActions if action["type"] == RESERVE_ACTION_TYPE]
        if reserve_actions:
            return reserve_actions

        return random.choice(legalActions)

    def isCostFree(self, game_state, card):
        """
        This function checks whether a buy action is cost-free.
        (i.e. using cards holding to buy a card, without costing extra gems)
        Return a boolean variable. 
        """
        free = True
        # card = (colour, cost, deck_id, points)
        cost = card.cost

        # Current card holdings for the player
        cards = game_state.agents[self.id].cards
        card_counts = {color: len(cards) for color, cards in cards.items()}
        for colour, n in cost.items():
            if card_counts[colour] < n:
                free = False
                break

        return free



    # This function is to check whether this agent still have sometime to think before he makes an action.
    # Return True if there is still time left, False otherwise
    def haveTime(self, startTime):
        return (time.time() - startTime) < MAX_TIME
    
    # A function to determine Late game status: AI has at least 10 scores
    def isLateGame(self, game_state):
        return self.game_rule.calScore(game_state, self.id) >= 10

    def SelectAction(self,actions,game_state):
        startTime = time.time()
        # print("startTime:", startTime)
        actions = self.getStrategicActions(game_state)
        # print("strategic action finished")
        queue = deque([(deepcopy(game_state),[])])
        # print(len(queue))
        # currentScore = self.game_rule.calScore(game_state, self.id)
        while len(queue) > 0 and self.haveTime(startTime):
            # print("enter")
            state, path = queue.popleft()
            score = self.game_rule.calScore(state, self.id)
            # if the player achieve winning score, return immediately
            if score >= WIN_SCORE and path:
                return path[0]

            for action in actions:
                next_state = self.game_rule.generateSuccessor(deepcopy(game_state), action, self.id)
                next_path = path + [action]
                queue.append((next_state, next_path))

        return path[0] if path else random.choice(action)
