# Game Introduction(游戏介绍)

## Game Rule（游戏规则）
[English](https://github.com/leungjunbob/AIP-project/blob/main/Unity/Splendor_English.pdf)

[中文版](https://github.com/leungjunbob/AIP-project/blob/main/Unity/Splendor_Chinese.pdf)

## Main Menu(主界面)

<img width="1132" height="603" alt="image" src="https://github.com/user-attachments/assets/afa15b48-1c89-4d0f-836d-48dd56171b57" />

This is the game start screen, where you can select the agent for player 1 and 2 (human, random, preferred) and set the time for each round.

这是游戏的开始界面，可以通过设置选择 player1和2的 agent（人类， 随机， 首选）和设定每回合的时间。

## Gem Collect pannel(宝石收集界面)

<img width="1425" height="820" alt="image" src="https://github.com/user-attachments/assets/73d2df8a-9a8f-4a64-bd1d-5aac6aee0e77" />

This is the Gem Collection screen. Click any Gem in the right frame to enter. Click the "Cancel" button to exit this screen.
Use the "+" and "-" buttons to select the Gems you want to collect/return,  Click the "Confirm" button to confirm your selection. Please note that the Gems you select must comply with the game rules.

这是宝石收集界面，可以通过点击右边框内的任意宝石唤出。在界面内点击“cancel”按钮退出该界面。通过“+”，“-”按钮选择收集/退还的宝石， 点击“confirm”按钮确认选择，注意宝石的选择要符合游戏规则。

## Top Information Bar(顶部信息显示栏)

<img width="1158" height="79" alt="image" src="https://github.com/user-attachments/assets/f4518848-17a8-4bd8-afd5-e9ad20bcca22" />

This is the top information display bar, which shows which player's turn it is in the current round, the scores of Player 1 and Player 2, and the remaining time of the current round.

这是顶部信息显示栏，显示当前回合轮到哪个玩家操作，玩家1和玩家2的分数，当前回合剩余时间。

## Buy Card Logit(买卡逻辑)

<img width="1055" height="739" alt="image" src="https://github.com/user-attachments/assets/947029fa-e8c3-492c-b87a-2ced4b859c7b" />

Clicking on any card on the table will pop up the "reserve" (add the card to the player's reserve area) and "buy" (pay the price to buy the gem) buttons.

点击牌桌任意一张卡都会弹出“reserve”（把卡加入到玩家的保留区域）和“buy”（支付代价购买宝石）按钮。

<img width="1001" height="799" alt="image" src="https://github.com/user-attachments/assets/d2238210-3f88-4460-ba2a-37c7cc6bfd0f" />

This is the card purchase interface, which will use text to describe the information of this card and calculate the price required to purchase the current card (after deducting the gems provided by the player for the purchased card).

这是买卡界面，上面会用文字说明这张卡的信息，还会计算购买当前卡需要支付什么样的代价（扣除玩家已购买卡提供的宝石后）。

## Noble Area(贵族卡区域)

<img width="114" height="467" alt="image" src="https://github.com/user-attachments/assets/3eca00c1-f56d-47e4-94e1-2a89e62a586c" />

This is the noble card area. After meeting the nobility conditions, it will be automatically added to the player information display area without any additional operations.

这是贵族卡区域，在满足贵族的条件后，无需额外操作，会自动加入到玩家信息展示区。

## Play Info Display Area(玩家信息展示区)

<img width="338" height="841" alt="image" src="https://github.com/user-attachments/assets/7e5bf88a-19c5-4f38-8f69-c1d152893791" />

This is the player information display area, showing the player's name, the player's score, the cards the player has purchased and retained, and the nobles the player has attracted.
When you click on the reserved card in your area, if you have enough resources to purchase it, the card purchase interface will pop up.

这是玩家信息展示区，展示玩家的名称，玩家的分数，玩家购买和保留的卡，玩家吸引的贵族。点击自己区域保留卡时，如果资源足够购买则会弹出买卡界面。

## Victory (胜利结算画面)

<img width="1263" height="900" alt="image" src="https://github.com/user-attachments/assets/3463a576-bef6-4805-bfab-505edc9180b7" />

When the winner appears, click the middle button to return to the main interface and start the next round of the game.

游戏胜利，点击中间按钮可以回到主界面开始下一轮的游戏。
