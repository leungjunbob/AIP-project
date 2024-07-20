from template import Agent
from Splendor.splendor_model import SplendorGameRule
from collections import deque
from copy import deepcopy
import random
import time
import math
# from agents.t_085.leung_sarsa import myAgent as SarsaAgent
from agents.t_085.cui_mcts import MCTS

# The maximum computation time for each action is 1 second, allowing 0.2 seconds remaining
MAX_TIME = 0.9
WIN_SCORE = 15
# If using whole set of legalActions, the minimax approach even cannot get to depth 2 for most cases
# If using filtered actions(strategicActions), the minimax approach get to depth 4-5 easily, and depth 9 at most.
MAX_DEPTH = 15
MAX_GEM = 10
NEG_INFINITY = float('-inf')
POS_INFINITY = float('inf')
# action types string
BUY_ACTION_TYPE = ["buy_available", "buy_reserve"]
RESERVE_ACTION_TYPE = "reserve"
GEM_ACTION_TYPE = ['collect_diff', 'collect_same']
# MCTS Agent Constants
MAX_SIM_DEPTH = 10
POINT_WEIGHT = 8
RESERVED_WEIGHT = 0.1
GOLDEN_CARD_WEIGHT = 1
NUM_PLAYERS = 2
NUM_OF_AGENT = 2

class myAgent(Agent):
    def __init__(self,_id):
        super().__init__(_id)
        self.game_rule = SplendorGameRule(num_of_agent=2)

    def haveTime(self):
        """ Check if there's still time left for the agent to think. """
        return (time.time() - self.startTime) < MAX_TIME
    
    
    def evaluate_state(self, state, depth):
        """ Calculate the score difference between this agent and the opponent. """
        opponent_id = 1 - self.id

        score_self = self.evaluate_player_score(state, self.id, depth)
        score_opponent = self.evaluate_player_score(state, opponent_id, depth)

        return score_self - score_opponent

    def evaluate_player_score(self, state, agent_id, depth):
        # a heuristic to determine if the agent achieve a win/loss state this turn
        # 1 for a win, -1 for a loss, 0 otherwise
        win_loss = self.calculate_win_loss(state, agent_id)

        # points is the score of an agent in the game in this turn, [0, 15, or higher]
        points = self.game_rule.calScore(state, agent_id)

        # progress towards nobles this turn
        nobleProgress = self.calculate_progress_toward_nobles(state, agent_id)

        # number of cards this turn
        prestige = len(state.agents[agent_id].cards.values())

        # number of gems this turn
        gems = sum(state.agents[agent_id].gems.values())
        # print("decay_factor:", depth)
        # Apply decay for each depth increment
        decay_factor = 0.9 ** depth
       
        return (100 * win_loss + 1.5 * points + 2.5 * nobleProgress + prestige + gems) * decay_factor
    
    # This function is written by leung.
    # Given a set of available actions for the agent to execute, and
    # a copy of the current game state (including that of the agent),
    # select one of the actions to execute. 
    def ifCanBuy(self, game_state):
        actions = self.game_rule.getLegalActions(game_state, self.id)
        for action in actions:
            if action["type"] == 'buy_available':
                return True
        return False
    # This gemAction is from leung's bfs
    # to only keep the gem actions that will make the next buy possible
    def get_strategicGemActions(self, game_state, gem_actions, agent_id):
        filter_gem_actions = []
        if (gem_actions):
            for action in gem_actions:
                next_state = self.game_rule.generateSuccessor(deepcopy(game_state), action, agent_id)
                if(self.ifCanBuy(next_state)):
                    filter_gem_actions.append(action)
            if (filter_gem_actions):
                return filter_gem_actions
            else:
                return gem_actions 
            
    def get_strategicActions(self, state, agent_id):
        gemNum = sum(state.agents[agent_id].gems.values())
        legalActions = self.game_rule.getLegalActions(state, agent_id)

        buy_actions = [action for action in legalActions if action["type"] in BUY_ACTION_TYPE]
        # print("HERE", buy_actions)
        if buy_actions:
            buy_actions.sort(key=lambda x:x['card'].points, reverse=True)
            return buy_actions


        if gemNum != MAX_GEM:
            gem_actions = [action for action in legalActions if action["type"] in GEM_ACTION_TYPE]
            if gem_actions:
                # sort it by the number of gems collected. 
                # gem_actions.sort(key=lambda action: sum(action['collected_gems'].values()), reverse=True)
                # return gem_actions
                return self.get_strategicGemActions(state, gem_actions, agent_id)

        
        reserve_actions = [action for action in legalActions if action["type"] == RESERVE_ACTION_TYPE]
        if reserve_actions:
            return reserve_actions

        return legalActions

    def calculate_win_loss(self, state, agent_id):
        opponent_id = 1 - self.id
        self_score = self.game_rule.calScore(state, self.id)
        opponent_score = self.game_rule.calScore(state, opponent_id)

        if self_score >= WIN_SCORE:
            return 1
        elif opponent_score >= WIN_SCORE:
            return -1
        else:
            return 0

    def calculate_progress_toward_nobles(self, state, agent_id):
        """
        This function computes a normalized score representing how close the player is to 
        meeting the requirements for each noble tile. It iterates over the noble requirements
        and calculates the proportion of required cards that the player already possesses.
        Return a score (float) ranging from [0, numberOfNoblesOnBroad]
        """
        total_progress = 0
        player_cards = state.agents[agent_id].cards
        player_cards_counts = {color: len(cards) for color, cards in player_cards.items()}
        # nobles currently on the board
        nobles = state.board.nobles

        for noble_id, requirements in nobles:
            progress = 0
            for color, required_amount in requirements.items():
                player_card_count = player_cards_counts.get(color, 0)
                if player_card_count >= required_amount:
                    progress += 1
                else:
                    progress += player_card_count / required_amount
            # Normalized progress: progressForEachColour / NumberOfDistinctColour
            total_progress += progress / len(requirements)  
        return total_progress

    def SelectAction(self,actions,game_state):
        """ Select the best action based on minimax algorithm within time constraints. """
        self.startTime = time.time()
        best_action = None
        depth_reached = 0
        for depth in range(1, MAX_DEPTH + 1):
            if not self.haveTime():
                break
            best_action_at_depth = self.minimax_decision(game_state, depth)
            if best_action_at_depth is not None:
                best_action = best_action_at_depth 
                depth_reached = depth

        print("Minimax & MCTS Depth Reached:", depth_reached)

        # thinkingTime = (time.time() - self.startTime)
        # print("Used Thinking Time:", thinkingTime)
        # Choose the best action from the last completed search depth
        # TODO: add fallback
        return best_action
    
    def minimax_decision(self, game_state, depth):
        """ Decide the best action using the minimax algorithm with alpha-beta pruning. """

        def max_value(state, agent_id, current_depth, alpha, beta):
            if current_depth == 0:
                return self.evaluate_state(state, depth=depth)
            
            max_eval = NEG_INFINITY
            actions = self.get_strategicActions(state, agent_id)
            # numActions = len(actions)
            for count, action in enumerate(actions):
                # print(f"Depth {depth}, Agent {agent_id}, Evaluating action {count+1}/{numActions}: {action}")
                if not self.haveTime():
                    break
                successor_state = self.game_rule.generateSuccessor(deepcopy(state), action, agent_id)
                eval_score = min_value(successor_state, opponent_id, current_depth-1, alpha, beta)
                max_eval = max(max_eval, eval_score)
                alpha = max(alpha, eval_score)
                if beta <= alpha:
                    break
            return max_eval

        def min_value(state, agent_id, current_depth, alpha, beta):
            if current_depth == 0:
                return self.evaluate_state(state, depth=depth)
            min_eval = POS_INFINITY
            # actions = self.get_strategicActions(state, agent_id)

            # Use MCTS agent to predict opponent's move
            self.mcts_agent = MCTS(game_state=deepcopy(state), agent_id=agent_id)
            actions = self.mcts_agent.search(100, self.startTime)
            numActions = len(actions)

            if not actions:
            # Fallback to get_strategicActions if MCTS fails to return actions
                actions = self.get_strategicActions(state, agent_id)

            # print(mcts_action)
            for count, action in enumerate(actions):
                print(f"Depth {depth}, Agent {agent_id}, Evaluating action {count+1}/{numActions}: {action}")
                if not self.haveTime():
                    break
                
                print("mcts_action:", action)
                successor_state = self.game_rule.generateSuccessor(deepcopy(state), action, agent_id)

                eval_score = max_value(successor_state, self.id, current_depth-1, alpha, beta)
                min_eval = min(min_eval, eval_score)
                beta = min(beta, eval_score)
                if beta <= alpha:
                    break
    
            return min_eval
        
        agent_id = self.id
        opponent_id = 1 - agent_id
        best_action = None
        best_eval = NEG_INFINITY
        actions = self.get_strategicActions(game_state, agent_id)
        # numActions = len(actions)
        # count = 0
        for action in actions:
            # count+=1
            # print(f"Depth {depth}, Evaluating action {count}/{numActions}: {action}")
            if not self.haveTime():
                # print("Out of time!")
                break
            successor_state = self.game_rule.generateSuccessor(deepcopy(game_state), action, agent_id)
            eval_score = min_value(successor_state, opponent_id, current_depth=depth-1, alpha = NEG_INFINITY, beta=POS_INFINITY)
            if eval_score > best_eval:
                best_eval = eval_score
                best_action = action
        return best_action
