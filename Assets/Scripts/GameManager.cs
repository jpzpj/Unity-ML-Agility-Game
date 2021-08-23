using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// The gamemanager
/// Does some houskeeping and some statistics
/// </summary>
public class GameManager : Singleton<GameManager>
{
    private int totalScore = 0;
    private int totalAgents = 0;
    private List<string> debugLogs = new List<string>();
    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        stopWatch.Start();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Keep track of the number of agents in the game play
    /// </summary>
    public void AddAgent()
    {
        totalAgents++;
    }

    /// <summary>
    /// Collect the scores and add the scores to the total score
    /// </summary>
    /// <param name="ballsOnBoard"></param>
    public void AddScore(Dictionary<int, BallOnBoard> ballsOnBoard)
    {
        foreach (var ball in ballsOnBoard)
        {
            totalScore += ball.Value.Score;
            debugLogs.Add($"{ball.Value.Name}\t{ball.Value.Score}");
        }
    }

    /// <summary>
    /// Log the scores
    /// </summary>
    protected override void OnDestroy()
    {
        stopWatch.Stop();
        foreach (var item in debugLogs.OrderBy(x => x))
        {
            Debug.Log(item);
        }
        Debug.Log($"Total score : {totalScore}");
        Debug.Log($"Session took: {stopWatch.Elapsed.TotalSeconds:F1} seconds");
        double secondsPerBoard = (double)totalScore / totalAgents; // Average per board
        secondsPerBoard /= stopWatch.Elapsed.TotalSeconds; // Average per board per second
        secondsPerBoard = 1.0 / secondsPerBoard; // Average per board

        Debug.Log($"Average play: {secondsPerBoard:F2} seconds per board");
        base.OnDestroy();
    }
}
