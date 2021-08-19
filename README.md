# Unity-ML-Agility-Game
 An agility game playable by Unity Machine Learning

My first Unity project using ML-Agents

The game is a agility game, where you have to try to get a ball into an arc by moving the board. 

![Game](https://user-images.githubusercontent.com/83223936/130071820-df63b6a7-0c0c-44d3-87b4-894f395eac6f.png)

This can be very frustrating so I wanted to give it a try with Unity ML-Agents after following the excellent Hummingbirds tutorial by Immersive Limt LLC (see https://learn.unity.com/course/ml-agents-hummingbirds)

After several tries I finally succeeded. ML-Agents is able to play the game in an average time less then 6 seconds.

I learned to keep to rewards simpel, I started with a difficult reward schema ut the Agent failed to learn. 
The key to succes was to use the 'Ray Perception Sensor 3D' component. That really made it working.

The training run of 4 milion episodes took about 1 hour on my machine.

Next goal is to add more balls to the game.
