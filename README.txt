Please refer to Othello.AI.TengfeiMa as my code.



I first tried the random AI that came with the project, but they were acting stupid because they kept giving the opponent chances to take corners. I looked at the code and realized that it is completely random and has no logic.

According to my own experience with Othello, I know that corners are the most important spots. So I made a position weight table and gave each spot a number. Corners have the highest value (100), and the spots next to corners get negative values (like -20 or -50) because putting a piece there can help the enemy grab a corner. Other squares have smaller numbers depending on how safe they are.

Only considering one move is not enough in any chess-like game, so I used the NegaMax algorithm with alpha‑beta pruning. I set the search depth to 4 – I tried 5 but it was too slow, and 3 wasn't "smart" enough. Depth 4 gives a good balance.

In the evaluation function, besides the position weights, I also added a "mobility" bonus: the difference between how many moves my AI has and how many the opponent has, multiplied by 5. This encourages my AI to keep more options open. I also added an extra 200 points for controlling corners (on top of the 100 from the weight table), because once you have a corner it's extremely valuable.

I played a few matches against the random AI and won all of them. One match ended 46 to 18, with all four corners taken by my AI. That means my position weight strategy is working! I also tried to playing my AI against itself, and the scores were tight. I attached a few screenshots of the matches. Match screenshot 4 is my AI playing against itself.