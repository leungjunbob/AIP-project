from template import Agent
import random, time, math
from copy import deepcopy
from Splendor.splendor_model import SplendorGameRule

NUM_OF_AGENT = 2
THINKTIME = 0.95
WIN_SCORE = 15
MAX_GEM = 10
NUM_PLAYERS = 2
COLOURS = {'B':'black', 'r':'red', 'y':'yellow', 'g':'green', 'b':'blue', 'w':'white'}

MAX_SIM_DEPTH = 5
BUY_ACTION_TYPE = ["buy_available", "buy_reserve"]
RESERVE_ACTION_TYPE = "reserve"
GEM_ACTION_TYPE = ['collect_diff', 'collect_same']
POINT_WEIGHT = 6
RESERVED_WEIGHT = 0.1
GOLDEN_CARD_WEIGHT = 1.5
CARD_GEM_WEIGHT = 3 

def card_cost(game_state, player_id, card, gem_weight = None):
    card = deepcopy(card)
    cost = card.cost
    cards = game_state.agents[player_id].cards
    card_counts = {color: len(cards) for color, cards in cards.items()}
    for colour in cost.keys():
        cost[colour] -= card_counts[colour]
        
    if gem_weight is None:
        return sum(cost.values())
    else:
        return sum([v*gem_weight[c] for c, v in cost.items()])

def evaluate_gemcolor_weight(gems, self_cards, dealt_cards):

    dealt_cost_sum = {c:0 for c in COLOURS.values()}
    self_cards_counts = {color: len(cards) for color, cards in self_cards.items()}
    for dealt_card in dealt_cards:
        for c, cost in dealt_card.cost.items():
            dealt_cost_sum[c] += (cost - self_cards_counts[c])
    gem_weight = {}
    for c in COLOURS.values():
        if c == 'yellow':
            gem_weight[c] = GOLDEN_CARD_WEIGHT
        else:
            if gems[c] >= (dealt_cost_sum[c] * 0.3): 
                gem_weight[c] = 0.5
            else:
                gem_weight[c] = 1
    return gem_weight, self_cards_counts
        
def evaluate_player_score(game_rule, game_state, player_id):
    current_state = deepcopy(game_state)
    point_weight = POINT_WEIGHT
    reserved_weight = RESERVED_WEIGHT
    
    gems = current_state.agents[player_id].gems
    self_cards = current_state.agents[player_id].cards
    self_nobles = current_state.agents[player_id].nobles
    dealt_cards = current_state.board.dealt_list() 
    gem_weight, self_cards_counts = evaluate_gemcolor_weight(gems, self_cards, dealt_cards)
    
    player_score = game_rule.calScore(current_state, player_id) * point_weight
    for c in COLOURS.values():
        player_score += (gems[c] * gem_weight[c])
        if c == 'yellow':
            for reserved_card in self_cards[c]:
                player_score += (reserved_card.points * reserved_weight)
        else:
            player_score += (self_cards_counts[c] * gem_weight[c] * CARD_GEM_WEIGHT)
            if self_cards_counts[c] >= 2:
                player_score += 3
            elif self_cards_counts[c] > 4:
                player_score -= 3
    return player_score, gem_weight

def SelectAction_v1(game_rule, actions, game_state, current_player_id):
    current_score, _ = evaluate_player_score(game_rule, game_state, current_player_id)
    mx_score = -float('inf')
    mx_action = actions[0]
    gemNum = sum(game_state.agents[current_player_id].gems.values())
    for action in actions:
        if gemNum > MAX_GEM - 3:
            if action["type"] in GEM_ACTION_TYPE:
                continue
        current_state = deepcopy(game_state)
        current_state = game_rule.generateSuccessor(current_state, action, current_player_id)
        score_improve = evaluate_player_score(game_rule, current_state, current_player_id)[0] - current_score
        if score_improve > mx_score:
            mx_score = score_improve
            mx_action = action
    return mx_action

def SelectAction_v2(game_rule, actions, game_state, current_player_id):
    current_score, gem_weight = evaluate_player_score(game_rule, game_state, current_player_id)
    mx_score = -float('inf')
    mx_action = actions[0]
    gemNum = sum(game_state.agents[current_player_id].gems.values())
    for action in actions:
        if action["type"] in GEM_ACTION_TYPE:
            if gemNum > MAX_GEM - 3:
                continue
            score_improve = sum([gem_weight[c] * n for c, n in action['collected_gems'].items()])
        if action["type"] in BUY_ACTION_TYPE:
            card = action['card']
            score_improve = card.points * POINT_WEIGHT - card_cost(game_state, current_player_id, card, gem_weight) + gem_weight[card.colour] * CARD_GEM_WEIGHT
        if action["type"] == RESERVE_ACTION_TYPE:
            card = action['card']
            score_improve = card.points * POINT_WEIGHT * RESERVED_WEIGHT + GOLDEN_CARD_WEIGHT
        
        if score_improve > mx_score:
            mx_score = score_improve
            mx_action = action
    return mx_action


class Node:
    def __init__(self, state, action=None, parent=None, id=-1):
        self.state = state
        self.id = id
        self.game_rule = SplendorGameRule(NUM_OF_AGENT)
        self.parent = parent
        self.action = action 
        self.untried_moves = self.getStrategicActions(state)
        
        self.children = []
        self.visits = 1
        self.score = 0

    def select_child(self):
        c = math.sqrt(2) * 0.3
        exploration_factor = c
        scores_ori = [child.score / child.visits for child in self.children]
        scores_adj = [child.score / child.visits + exploration_factor * math.sqrt(math.log(child.parent.visits) / child.visits) for child in self.children]
        print('UCTs: ', scores_ori, scores_adj)
        return max(self.children, key=lambda child: 
                   child.score / child.visits + exploration_factor * math.sqrt(math.log(child.parent.visits) / child.visits))

    def is_fully_expanded(self):
        return len(self.untried_moves) == 0
    
    def expand(self):
        best_action = self.untried_moves.pop(0)
        current_state = deepcopy(self.state)
        next_state = self.game_rule.generateSuccessor(current_state, best_action, self.id)
        next_action_agent_id = 1 - self.id 
        new_node = Node(next_state, best_action, self, next_action_agent_id)
        self.children.append(new_node)
        return new_node

    def getStrategicActions(self, game_state):
        gemNum = sum(game_state.agents[self.id].gems.values())
        legalActions = self.game_rule.getLegalActions(game_state, self.id)
        sorted_actions = []
        buy_actions = [action for action in legalActions if action["type"] in BUY_ACTION_TYPE]
        if len(buy_actions) > 0:
            buy_actions.sort(key=lambda x:x['card'].points, reverse=True)

        if gemNum <= MAX_GEM - 3:
            gem_actions = [action for action in legalActions if action["type"] in GEM_ACTION_TYPE]
            if len(gem_actions) > 0:
                gem_actions.sort(key=lambda action: sum(action['collected_gems'].values()), reverse=True)
        else:
            gem_actions = []
        
        reserve_actions = [action for action in legalActions if action["type"] == RESERVE_ACTION_TYPE]
        
        if game_state.agents[self.id].score > 8:
            sorted_actions += buy_actions
            sorted_actions += gem_actions
        else:
            sorted_actions += gem_actions
            sorted_actions += buy_actions
        sorted_actions += reserve_actions
        return sorted_actions
    

class MCTS:
    def __init__(self, game_state, agent_id):

        self.game_state  = game_state
        self.agent_id = agent_id
        self.game_rule = SplendorGameRule(NUM_OF_AGENT)
        self.root = Node(game_state, id=self.agent_id)

    def search(self, num_iterations, startTime):
        root = self.root
        while time.time() - startTime < THINKTIME and num_iterations > 0:
            num_iterations -= 1
            node = self.select(root)
            score = self.simulate(node, startTime)
            self.backpropagate(node, score)
        
        best_child = max(root.children, key=lambda child: child.visits) if (len(root.children) > 0) else None
        root_res = {str(child.action): child.visits for child in root.children}
        print(f'MCTs: {root_res}')
        if best_child:
            self.root = best_child
            self.root.parent = None 
        return best_child.action if best_child else None
    
    def select(self, node):

        while node.is_fully_expanded() and (len(node.children) > 0): 
            node = node.select_child()
        
        if not node.is_fully_expanded():
            node = node.expand()
            return node
        else:
            return node

    def simulate(self, node, startTime):
        current_state = deepcopy(node.state)
        current_player_id = node.id
        depth = 0
        
        current_weighted_score, _ = evaluate_player_score(self.game_rule, current_state, self.agent_id)
        max_depth = MAX_SIM_DEPTH
        
        while (not self.is_game_over(current_state)) and depth < max_depth: 
            actions = self.game_rule.getLegalActions(current_state, current_player_id)
            if not actions:
                break
        
            action = SelectAction_v2(self.game_rule, actions, current_state, current_player_id)
            
            current_state = self.game_rule.generateSuccessor(current_state, action, current_player_id)
            current_player_id = 1 - current_player_id
            depth += 1
        return evaluate_player_score(self.game_rule, current_state, self.agent_id)[0] - current_weighted_score

    def evaluate_state(self, state):
        weighted_score = self.game_rule.calScore(state, self.agent_id) * POINT_WEIGHT + sum(state.agents[self.agent_id].gems.values())
        return weighted_score
    
    def backpropagate(self, node, score):
        depth_factor = 1
        while node is not None:
            node.visits += 1
            adjusted_score = score * depth_factor
            node.score += adjusted_score
            node = node.parent
            depth_factor *= 0.99
    
    def is_game_over(self, state):
        for agent in state.agents:
            if agent.score >= 15:
                return True
        return False
    
    def GreedySelectAction(self, actions, game_state, current_player_id):
        total_gems = sum(game_state.agents[current_player_id].gems.values())
        gem_threshold = min(8, max(5,10 - len(game_state.board.nobles)))
        best_action = None
        best_score = -float('inf')
        if total_gems <= gem_threshold:
            gem_actions = [a for a in actions if a["type"] in ['collect_diff', 'collect_same']]
            for action in gem_actions:
                score = self.evaluate_gem_action(action, game_state)
                if score > best_score:
                    best_score = score
                    best_action = action
        buy_actions = [a for a in actions if a["type"] in ['buy_available', 'buy_reserve']]
        for action in buy_actions:
            score = self.evaluate_buy_action(action, game_state, current_player_id)
            if score > best_score:
                best_score = score
                best_action = action
        reserve_actions = [a for a in actions if a["type"] == 'reserve']
        for action in reserve_actions:
            score = self.evaluate_reserve_action(action, game_state)
            if score > best_score:
                best_score = score
                best_action = action
        return best_action if best_action else random.choice(actions)

    def evaluate_gem_action(self, action, game_state):
        return sum(action['collected_gems'].values())

    def evaluate_buy_action(self, action, game_state, current_player_id):
        card = action['card']
        return card.points * POINT_WEIGHT - card_cost(game_state, current_player_id, card) 

    def evaluate_reserve_action(self, action, game_state):
        card = action['card']
        return card.points * POINT_WEIGHT * RESERVED_WEIGHT + GOLDEN_CARD_WEIGHT

class myAgent(Agent):
    def __init__(self, _id):
        super().__init__(_id)
        self.game_rule = SplendorGameRule(NUM_OF_AGENT)
        self.round = 0

    def getAction(self, game_state):
        actions = self.game_rule.getLegalActions(game_state, self.id)
        return actions

    def GreedySelectAction(self, actions, game_state):
        total_gems = sum(game_state.agents[self.id].gems.values())
        gem_threshold = MAX_GEM - 3
        best_action = None
        best_score = -float('inf')
        if total_gems <= gem_threshold:
            gem_actions = [a for a in actions if a["type"] in ['collect_diff', 'collect_same']]
            for action in gem_actions:
                score = self.evaluate_gem_action(action, game_state)
                if score > best_score:
                    best_score = score
                    best_action = action
        buy_actions = [a for a in actions if a["type"] in ['buy_available', 'buy_reserve']]
        for action in buy_actions:
            score = self.evaluate_buy_action(action, game_state)
            if score > best_score:
                best_score = score
                best_action = action
        reserve_actions = [a for a in actions if a["type"] == 'reserve']
        for action in reserve_actions:
            score = self.evaluate_reserve_action(action, game_state)
            if score > best_score:
                best_score = score
                best_action = action
        return best_action if best_action else random.choice(actions)

    def evaluate_gem_action(self, action, game_state):
        return sum(action['collected_gems'].values())

    def evaluate_buy_action(self, action, game_state):
        card = action['card']
        return card.points - 0.5 * sum(card.cost.values())

    def evaluate_reserve_action(self, action, game_state):
        card = action['card']
        return card.points + 1.0

    print()
    def SelectAction(self, actions, game_state):
        self.startTime = time.time()
        self.round += 1
        best_action = SelectAction_v1(self.game_rule, actions, game_state, self.id)
        if time.time() - self.startTime < THINKTIME:
            mcts = MCTS(game_state, self.id)
            action_from_mcts = mcts.search(50, self.startTime)
            if action_from_mcts and action_from_mcts in actions:
                best_action = action_from_mcts
            else:
                pass
        return best_action