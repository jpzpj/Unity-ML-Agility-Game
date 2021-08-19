using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    private int totalScore = 0;
    private List<string> debugLogs = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
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
        foreach (var item in debugLogs.OrderBy(x => x))
        {
            Debug.Log(item);
        }
        Debug.Log($"Total score : {totalScore}");
        base.OnDestroy();
    }
}
