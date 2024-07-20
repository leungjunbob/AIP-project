from template import Agent
from Splendor.splendor_model import SplendorGameRule
from collections import deque
from copy import deepcopy
import copy
import random
import time
import heapq

# The maximum computation time for each action is 1 second, allowing 0.2 seconds remaining
MAX_TIME = 0.9
WIN_SCORE = 15
# Maximum depth for the minimax algorithm
MAX_DEPTH = 15
MAX_GEM = 10
NEG_INFINITY = float('-inf')
POS_INFINITY = float('inf')




TIMELIMIT = 0.95

# action types string
BUY_AVAILABLE = 'buy_available'
BUY_RESERVE = 'buy_reserve'
COLLECT_DIFF = 'collect_diff'
COLLECT_SAME = 'collect_same'
RESERVE = 'reserve'

COLLECTED_GEMS = 'collected_gems'
RETURNED_GEMS = 'returned_gems'
GEM_COLOURS = ['black', 'red', 'yellow', 'green', 'blue', 'white']

class PriorityQueue:
    """ 
    Priority queue implementation using a heap. 
    Referenced from UC-Berkeley-Pacman-Project Assignment 1
    """
    def __init__(self):
        self.heap = []
        self.count = 0

    def push(self, item, priority):
        entry = (priority, self.count, item)
        heapq.heappush(self.heap, entry)
        self.count += 1

    def pop(self):
        (_, _, item) = heapq.heappop(self.heap)
        return item

    def isEmpty(self):
        return len(self.heap) == 0

    def update(self, item, priority):
        """ Update the priority of an existing item or add it if not present. """
        for index, (p, c, i) in enumerate(self.heap):
            if i == item:
                if p <= priority:
                    break
                del self.heap[index]
                self.heap.append((priority, c, item))
                heapq.heapify(self.heap)
                break
        else:
            self.push(item, priority)

    def getLowestPriorityItems(self, n=5):
        """ Get items with the lowest priority, limited by a minimum count. """
        if self.isEmpty():
            return []

        items = []
        lowest_priority = self.heap[0][0]
        # Allowable priority range
        threshold = lowest_priority + 4 

        while not self.isEmpty() and self.heap[0][0] <= threshold:
            _, _, item = self.heap[0]
            if self.heap[0][0] <= threshold:
                items.append(item)
                self.pop() 
            else:
                break 

        # return top 5 if there are more than 5 actions
        if len(items) > n:
            items = items[:n]

        return items




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
        """ 
        Evaluate the score of a player using a heuristic function. 
        This heuristic function is referenced from (Joshua Hepworth, 2016).
        Hepworth, J. (2016). The states of splendor: searching game trees with partial information.. https://doi.org/10.13140/rg.2.1.2268.4401
        """
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

    def get_all_agent_gems_state(self, game_state):
        """Get the gem state of all agents in the game, including gems they holding and cards's colour, excluding yellow."""
        
        def calculate_gem_state(agent):
            return {
                gem: agent.gems[gem] + (len(agent.cards[gem]) if gem != 'yellow' else 0)
                for gem in GEM_COLOURS
            }
        
        return [calculate_gem_state(agent) for agent in game_state.agents]
    
    def get_nobles_requirement(self, game_state):
        """ Get the requirements of nobles on the board. """
        nobles_requirement = []
        for _, req in game_state.board.nobles:
            nobles_requirement.append(req)
        return nobles_requirement
    
    def get_cards_count(self, game_state, agent_id):
        """Get the card counts of a specific agent."""
        return {
            color: len(cards) for color, cards in game_state.agents[agent_id].cards.items() if color != 'yellow'
        }

    
    def get_action_heuristic_value(self, game_state, agents_gem_state, nobles_state, action):
        """ Evaluate the heuristic value of an action. """
        max_score = 160 #  # Maximum possible score: 100 + 3 * 20 = 160
        action_score = self.get_action_score(game_state, agents_gem_state, nobles_state, action)
        return max_score - action_score
    
    def analyze_action_effect(self, action):
        """ Analyze an action to determine its effects on points, gems, and cards. """

        has_noble = bool(action['noble'])
        get_points = 3 if has_noble else 0
        get_gems = {color: 0 for color in GEM_COLOURS}
        get_card = {}
        # if buy card, get points, card, gems
        if action['type'] in [BUY_AVAILABLE, BUY_RESERVE]:
            get_points += action['card'].points
            get_card[action['card'].colour] = 1
            for colour, value in action[RETURNED_GEMS].items():
                get_gems[colour] -= value
        # COLLECT_DIFF, COLLECT_SAME, or RESERVE:
        else: 
            for colour, value in action[COLLECTED_GEMS].items():
                get_gems[colour] += value
            for colour, value in action[RETURNED_GEMS].items():
                get_gems[colour] -= value
        
        return has_noble, get_points, get_gems, get_card
    
    def calculate_noble_score(self, has_noble):
        """Calculate the score for acquiring a noble."""
        return 100 if has_noble else 0

    def calculate_point_score(self, points):
        """Calculate the score based on points acquired."""
        return points * 20

    def calculate_card_score(self, get_card, nobles_prefer_colours, action_type):
        """Calculate the score based on cards acquired and reserved."""
        if not get_card:
            return 0
        get_card_colour = list(get_card.keys())[0]
        card_score = 20 * len(get_card) + nobles_prefer_colours.get(get_card_colour, 0) * 2
        if action_type == BUY_RESERVE:
            card_score += 2
        return card_score
    
    def calculate_gem_score(self, get_gems):
        """Calculate the score based on gems acquired."""
        return sum(value * 2 for value in get_gems.values())
    
    def calculate_reserve_score(self, action, game_state, agents_gem_state, nobles_prefer_colours, nobles_requirement):
        """Calculate the score for reserving a card."""
        opponent_agent_id = 1 - self.id
        # check if opponent can afford this card given his current state
        opponent_can_afford = all(agents_gem_state[opponent_agent_id][colour] >= price for colour, price in action['card'].cost.items())
        if opponent_can_afford:
            reserve_oppo_noble = 0
            agent_card_count = self.get_cards_count(game_state, opponent_agent_id)
            agent_card_count[action['card'].colour] += 1
            iseffort = all(agent_card_count[colour] >= value for noble in nobles_requirement for colour, value in noble.items())
            reserve_oppo_noble = 60 if iseffort else 0
            # 7 is the reserve points, 10 is the points for point for reserve card
            return 7 + action['card'].points * 10 + nobles_prefer_colours[action['card'].colour] + reserve_oppo_noble
        return 0
    
    def calculate_progress_to_card_score(self, action, game_state, agents_gem_state, nobles_prefer_colours):
        """Calculate the score for being close to acquiring a card."""
        opera_gem_state = copy.deepcopy(agents_gem_state[self.id])
        # print(action)
        for colour, price in action[COLLECTED_GEMS].items():
            opera_gem_state[colour] += price
        for colour, price in action[RETURNED_GEMS].items():
            opera_gem_state[colour] -= price

        close_to_card_score = 0
        board_cards = game_state.board.dealt_list()
        reserve_cards = game_state.agents[self.id].cards['yellow']
        available_cards = board_cards + reserve_cards

        for card in available_cards:
            can_afford = all(opera_gem_state[colour] >= price for colour, price in card.cost.items())
            gap = sum(max(0, price - opera_gem_state[colour]) for colour, price in card.cost.items())

            if can_afford:
                close_to_card_score = max(close_to_card_score, 3 + nobles_prefer_colours[card.colour])
            elif gap < 3:
                close_to_card_score = max(close_to_card_score, (3 + nobles_prefer_colours[card.colour]) / 2)
        
        return close_to_card_score

    def get_action_score(self, game_state, agents_gem_state, nobles_requirement, action):
        """ 
        Calculate the score of an action based on various heuristics. 
        includes: nobles, points, cards, gems, reserve, progress to get a card
        """

        # Calculate nobles preferences
        nobles_prefer_colours = {color: sum(noble.get(color, 0) for noble in nobles_requirement) for color in ["red", "green", "blue", "black", "white"]}
        # Analyzation of action effect
        has_noble, get_points, get_gems, get_card = self.analyze_action_effect(action)
        # calculate scores for each components
        noble_score = self.calculate_noble_score(has_noble)
        point_score = self.calculate_point_score(get_points)
        card_score = self.calculate_card_score(get_card, nobles_prefer_colours, action['type'])
        gem_score = self.calculate_gem_score(get_gems)
        reserve_score = self.calculate_reserve_score(action, game_state, agents_gem_state, nobles_prefer_colours, nobles_requirement) if action['type'] == RESERVE else 0
        progress_to_card_score = self.calculate_progress_to_card_score(action, game_state, agents_gem_state, nobles_prefer_colours) if action['type'] in [COLLECT_DIFF, COLLECT_SAME] else 0
        
        return noble_score + point_score + card_score + gem_score + reserve_score + progress_to_card_score
    
    def get_strategicActions(self, game_state, agent_id):
        legalActions = self.game_rule.getLegalActions(game_state, agent_id)
        action_queue = PriorityQueue()
        # Pick up current gem state from current game_state
        agents_gem_state = self.get_all_agent_gems_state(game_state)
        # self_gem_state = agents_gem_state[self.agent_id]
        # Pick up current nobles prices on board from game_state, [{'':},{'':},{'':}]
        nobles_requirement = self.get_nobles_requirement(game_state)
        # evaluate each action
        for action in legalActions:
            action_queue.push(action, self.get_action_heuristic_value(game_state, agents_gem_state, \
                                                                      nobles_requirement, action))

        best_action_list = action_queue.getLowestPriorityItems()
        # for action in best_action_list:
        #     print(action)
        return best_action_list
    

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

        for _, requirements in nobles:
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

    def SelectAction(self, actions, game_state):
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

        # print("Minimax Depth Reached:", depth_reached)

        # thinkingTime = (time.time() - self.startTime)
        # print("Used Thinking Time:", thinkingTime)
        return best_action
    
    def minimax_decision(self, game_state, depth):
        """ Decide the best action using the minimax algorithm with alpha-beta pruning. """

        def max_value(state, agent_id, current_depth, alpha, beta):
            if current_depth == 0:
                return self.evaluate_state(state, depth=depth)
            
            max_eval = NEG_INFINITY
            actions = self.get_strategicActions(state, agent_id)
            for action in actions:
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
            actions = self.get_strategicActions(state, agent_id)
            for action in actions:
                if not self.haveTime():
                    break
                successor_state = self.game_rule.generateSuccessor(deepcopy(state), action, agent_id)
                eval_score = max_value(successor_state, self.id, current_depth-1, alpha, beta)
                min_eval = min(min_eval, eval_score)
                beta = min(beta, eval_score)
                if beta <= alpha:
                    break
    
            return min_eval
        
        # print(f"Starting minimax decision for depth: {depth}")
        agent_id = self.id
        opponent_id = 1 - agent_id
        best_action = None
        best_eval = NEG_INFINITY
        actions = self.get_strategicActions(game_state, agent_id)
        # print(actions)
        numActions = len(actions)
        count = 0
        for action in actions:
            count+=1
            print(f"Evaluating action {count}/{numActions}: {action}")
            if not self.haveTime():
                break
            successor_state = self.game_rule.generateSuccessor(deepcopy(game_state), action, agent_id)
            eval_score = min_value(successor_state, opponent_id, current_depth=depth-1, alpha = NEG_INFINITY, beta=POS_INFINITY)
            if eval_score > best_eval:
                best_eval = eval_score
                best_action = action
        return best_action
