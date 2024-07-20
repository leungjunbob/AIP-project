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
| - | set minimax to MAX_TIME = 0.99                 | ---   | ---      |
| 7 | minimax(Heuristic 3, MAX_TIME=0.99) | 14.32 | 13 (65%) |
| 7 | leung_bfs            | 13.80 | 7 (35%)  |
| 8 | minimax(Heuristic 3, MAX_TIME=0.99) | 14.25 | 12(60%) |
| 8 | minimax(Heuristic 2, MAX_TIME=0.99) | 12.50 | 8(40%)  |
| - | ---                  | ---   | ---      |
| 9 | minimax(Heuristic 3) | 14.25 | 12(60%) |
| 9 | minimax(Heuristic 3, leung's gemAction filtering) | 12.50 | 8(40%)  |
| 10 | minimax(Heuristic 3) | 12.00 | 8(40%) |
| 10 | minimax(Heuristic 3, leung's gemAction filtering) | 13.10 | 12(60%)  |
| - | ---                  | ---   | ---      |
| 11 | minimax(Heuristic 3) | 13.40 | 8(40%) |
| 11 | minimax(Heuristic 3, leung's gemAction filtering, mcts) | 15.10 | 12(60%)  |
