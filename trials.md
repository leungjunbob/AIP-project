### Heuristic Function Comparison Table

Heuristic 1: 
100 * win_loss + 2 * points + 2 * nobleProgress + prestige + gems

Heuristic 2: 
(100 * win_loss + 1.5 * points + 2.5 * nobleProgress + prestige + gems)

Heuristic 3 (2 with a depth decay):
(100 * win_loss + 1.5 * points + 2.5 * nobleProgress + prestige + gems) * 0.9^depth

| Trial | Participant | Avg. Score (over 20 games) | Wins (over 20 games) |
|-----------|-------------|----------------------------|----------------------|
| 1 | minimax(Heuristic 1) | 13.90 | 9 (45%)  |
| 1 | bfs_highScoreAiming  | 12.45 | 11 (55%) |
| - | ---                  | ---   | ---      |
| 2 | minimax(Heuristic 2) | 13.40 | 12 (60%) |
| 2 | bfs_highScoreAiming  | 12.45 | 8 (40%)  | 
| 3 | minimax(Heuristic 2) | 13.60 | 13 (65%) |
| 3 | bfs_highScoreAiming  | 12.20 | 7 (35%)  |
| 4 | leung_bfs            | 13.75 | 11 (55%) |
| 4 | minimax(Heuristic 2) | 12.38 | 9 (45%)  | 
| - | ---                  | ---   | ---      |
| 5 | minimax(Heuristic 3) | 12.45 | 9 (45%)  | 
| 5 | leung_bfs            | 13.55 | 11 (55%) |
| 6 | minimax(Heuristic 3) | 13.25 | 10 (50%) |
| 6 | bfs_highScoreAiming  | 11.75 | 10 (50%) |
| - | ---                  | ---   | ---      |
| 7 | minimax(Heuristic 3, MAX_TIME=0.99) | 14.32 | 13 (65%) |
| 7 | leung_bfs            | 13.80 | 7 (35%)  |
| 8 | minimax(Heuristic 3, MAX_TIME=0.99) | 14.25 | 12(60%) |
| 8 | minimax(Heuristic 2, MAX_TIME=0.99) | 12.50 | 8(40%)  |
| 9 | minimax(H3 with gem filter) | 14.10 | 11(55%) |
| 9 | minimax(H3 pure)            | 12.85 | 9 (45%) |
| 10 | minimax(H3 pure)           | 14.25 | 12 (60%) |
| 10 | minimax(H3 with gem filter)| 12.50 | 8 (40%) |
| 11 | minimax(H3 pure)           | 12.00 | 8 (40%) |
| 11 | minimax(H3 with gem filter)| 13.10 | 12 (60%) |

| | leung_sarsa | 16.55 | 19 (95%) |
| | cui_mcts | 9.88 | 1 (5%) |
| | sarsa | 9.95 | 2 (10%) |
| | minimax | 14.90 | 18 (90%) |
| | mcts | 6.88 | 1 (5%) |
| | minimax | 16.35 | 19 (95%) |
| | mcts | 9.1 | 0 (0%) |
| | bfs | 16.1 | 20 (100%) |
| | sarsa | 14.25 | 13 (65%) |
| | bfs | 10.75 | 7 (35%) |



Result on Online server
| Trial | Participant | Avg. Score (over 40 games) | Wins (over 40 games) |
|-----------|-------------|----------------------------|----------------------|
| 9  | minimax(H3 with gem filter) | 11.86 | 11 (27.5%) |
| 10 | minimax(H3 pure)           |  12.89 | 18 (45.0%) |
| 11 | minimax(H3 with gem filter) | 12.53 | 17 (42.5%) |
| 12 | SARSA | 13.20 | 20 (50.0%) |