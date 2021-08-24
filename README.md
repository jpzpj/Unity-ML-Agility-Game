# Unity-ML-Agility-Game
An agility game playable by Unity Machine Learning

My first Unity project using ML-Agents.

The game is a agility game, where you have to try to get a ball into an arc by moving the board. 

![Game](https://user-images.githubusercontent.com/83223936/130071820-df63b6a7-0c0c-44d3-87b4-894f395eac6f.png)

This can be very frustrating so I wanted to give it a try with Unity ML-Agents after following the excellent Hummingbirds tutorial by Immersive Limit LLC (see https://learn.unity.com/course/ml-agents-hummingbirds).

The agent moves the board, rotating it either to the left, right, forward or backward and gets a reward when a ball enters the arc.

After several tries I finally succeeded. ML-Agents is able to play the game, with 1 ball, in an average time of less then 6 seconds, amazing. See a video of a game at https://youtu.be/gKZBCa-mwug.

I learned to keep to rewards simpel, I started with a difficult reward schema but the Agent failed to learn. 
The key to succes was to use the 'Ray Perception Sensor 3D' component. That made a huge difference for the better.

The training run of 4 million episodes takes about 1 hour on my machine.

**Two balls**

![2Balls](https://user-images.githubusercontent.com/83223936/130447547-ec0f3785-a643-4de3-970d-f509b95037a1.png)

Training (4 million steps) took 147 minutes.  It takes the agent an average of 73 seconds.
See a video of a game at https://youtu.be/hMxvOkps6Cw (first win is at 00:11)

**Three balls**

![3Balls](https://user-images.githubusercontent.com/83223936/130617119-5b1075fe-010c-4cca-911d-da597d9ea489.png)

Training (12 million steps) took 500 minutes. It takes the agent an average of 1054 seconds to win. 
See a video of a game at https://youtu.be/4n_t4zwsSpg. The first 00:05:30 are trimmed. The first win is at 00:10 (actual time at 05:40).
Not bad, but Would be nice to improve this. Still I can't win the 3-ball game in less than this time, too frustrating. To improve the ML Agents performance in playing the game it may need a bit of curiosity.
