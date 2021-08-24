using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using Unity.MLAgents.Actuators;

/// <summary>
/// This uses release 18 of ML-Agents.
/// To install: 
/// Github via Package Manager open the Package Manager, hit the "+" button, and select "Add package from git URL".
/// if the dialog that appears, enter: git+https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents#release_18
/// You can also edit your project's manifest.json directly and add the following line to the dependencies section:
/// "com.unity.ml-agents": "git+https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents#release_18",
/// <see cref="https://docs.unity3d.com/Packages/com.unity.ml-agents@2.1/manual/index.html"/>
/// </summary>
public class BoardAgent : Agent
{
    /// <summary>
    /// Object to instantiate
    /// </summary>
    [SerializeField] GameObject prefabBall;
    /// <summary>
    /// The position of object holding the perception sensor 
    /// </summary>
    [SerializeField] GameObject entryPosition;
    /// <summary>
    /// The amoutn of movement of the board
    /// </summary>
    [SerializeField] float rotationSpeed = 5f;
    /// <summary>
    /// The number of balls in the game to play with
    /// </summary>
    private int ballsInGame = 1;
    /// <summary>
    /// Dictionary holding the balls on the board, with references to among others 
    /// the instance of this script
    /// </summary>
    private Dictionary<int, BallOnBoard> ballsOnBoard = null;
    /// <summary>
    /// The maximum distace, needed for normalization of the distance
    /// </summary>
    private const float maxDistance = 20f;
    /// <summary>
    /// The maximum position on the board, needed for normalization of the position
    /// </summary>
    private const float maxPosition = 12f;
    /// <summary>
    /// The maximum speed the ball is moving, needed for normalization of the speed value
    /// </summary>
    private const float maxSpeed = 30;  // Derived during various game plays
    /// <summary>
    /// Allows for changing the color
    /// </summary>
    private MeshRenderer meshRenderer;
    /// <summary>
    /// The oroginal color of the board
    /// </summary>
    private Color originalColor;
    /// <summary>
    /// Pause the execution for a while only during playing, not during learning
    /// </summary>
    private bool paused = false;
    /// <summary>
    /// When recording a video or a manual play set this to true, must be false when training
    /// </summary>
    bool isRecording = false;

    /// <summary>
    /// Start is called by Unity before the first frame update
    /// </summary>
    void Start()
    {
        if (isRecording)
        {
            Debug.Log("The board will become blue at a winning game.");
        }
        else
        {
            Debug.Log("The board will not become blue at a winning game.");
        }
        GameManager.Instance.AddAgent();
    }

    /// <summary>
    /// OnApplicationQuit is called by Unity when the game stops playing
    /// </summary>
    private void OnApplicationQuit()
    {
        GameManager.Instance.AddScore(ballsOnBoard);
    }

    #region ML
    /// <summary>
    /// Initialize is called by ML during the start of a new game play
    /// </summary>
    public override void Initialize()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;

        // Initialize the dictionary
        ballsOnBoard = new Dictionary<int, BallOnBoard>();
        for (int i = 0; i < ballsInGame; i++)
        {
            // Instantiate a new ball
            GameObject ball = Instantiate(prefabBall,
                                          Helper.RandomBallPosition() + transform.position,
                                          prefabBall.transform.rotation);
            ball.SetActive(false);
            ball.transform.SetParent(this.transform);

            BallOnBoard bob = new BallOnBoard(Helper.PrettyName(this.name) + $"[{i}]", ball);
            ballsOnBoard.Add(i, bob);
            Ball oBallScript = bob.Script;
            if (oBallScript != null)
            {
                oBallScript.ID = i;
                oBallScript.OnArcEnter.AddListener(BallEventEnterArc);
                oBallScript.OnArcExit.AddListener(BallEventExitArc);
            }
        }
    }

    /// <summary>
    /// OnEpisodeBegin is called by ML during the start of a new episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset the start rotation of the board
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        // reset the original color
        meshRenderer.material.color = originalColor;

        // The board may be given an initial rotation, however this is not needed
        // Training with a level board works fine even when the initial board is rotated
        // during normal play
        //transform.Rotate(new Vector3(1, 0, 0), UnityEngine.Random.Range(-10f, 10f));
        //transform.Rotate(new Vector3(0, 0, 1), UnityEngine.Random.Range(-10f, 10f));

        // Put the balls on the board
        foreach (var ball in ballsOnBoard)
        {
            ball.Value.Script.Initialize();
            ball.Value.Object.SetActive(false);
            ball.Value.Object.transform.position = Helper.RandomBallPosition() + transform.position;
            ball.Value.Object.SetActive(true);
        }
    }

    /// <summary>
    /// Called by ML to get the sensors to be used
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation.z);    // 1 observation value
        sensor.AddObservation(transform.rotation.x);    // 1 observation value
        sensor.AddObservation(Helper.NormalizePosition(ballsOnBoard[0].Object.transform.position - transform.position, maxPosition)); // 3 observation values
        float distance = Vector3.Distance(ballsOnBoard[0].Object.transform.position, entryPosition.transform.position);
        sensor.AddObservation(distance / maxDistance);    // 1 observation value
        sensor.AddObservation(ballsOnBoard[0].RigidBody.velocity.magnitude / maxSpeed); // 1 observation value

        // Total 7 observations: this value must match the Space Size value in the Behavior Parameters
        // property of the agent
    }

    /// <summary>
    /// Called by ML to perform an action
    /// </summary>
    /// <param name="vectorAction"></param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Allow for a pause in activity e.g. when recording
        if (paused) return;

        float actionZ = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float actionX = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Save the current rotation
        Quaternion currentQuaternionRotation = transform.rotation;

        transform.Rotate(actionX * rotationSpeed * Vector3.right);
        transform.Rotate(actionZ * rotationSpeed * Vector3.back);

        // If the rotation would be clipped, restore the original rotation
        // Note: if we would perform clipping based on Euler angles, unwanted side effects might occur 
        if (Helper.IsRotationClipped(transform.rotation))
        {
            transform.rotation = currentQuaternionRotation;
            SetReward(-0.02f);
        }

        // ML specific: if one of the balls falls off the board, then add a huge penalty and stop
        // the mission.
        foreach (var ball in ballsOnBoard)
        {
            if (ball.Value.Object.transform.position.y - transform.position.y < -20f)
            {
                SetReward(-2f);
                EndEpisode();
            }

            // Give a penalty when the ball is not moving
            if (ball.Value.RigidBody.velocity.magnitude < 0.01f)
            {
                SetReward(-0.01f);
            }
        }
    }

    /// <summary>
    /// Called by ML in case the Behavior Type is Heuristic or in case there is no model
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");  // RotateZ
        continuousActionsOut[1] = Input.GetAxis("Vertical");    // RotateX
    }

    #endregion

    #region Ball events and helpers
    /// <summary>
    /// Called when a ball enters the arc
    /// </summary>
    /// <param name="sender">The ball that contains the event data</param>
    private void BallEventExitArc(Ball sender)
    {
        // The ball exits the arc, this is not good, so give a penalty
        BallOnBoard b = ballsOnBoard[sender.ID];
        if (b != null)
        {
            Debug.Log($"Arc Exit");
            SetReward(-1f);
        }
    }

    /// <summary>
    /// Called when a ball exits the arc
    /// </summary>
    /// <param name="sender">The ball that contains the event data</param>
    private void BallEventEnterArc(Ball sender)
    {
        BallOnBoard b = ballsOnBoard[sender.ID];
        if (b != null)
        {
            Debug.Log($"Arc Enter");

            // ML specific: add a huge reward
            SetReward(1f);
            if (BallsInArc() == ballsInGame)
            {
                b.IncreaseScore();
                // All done: end the scenario
                EndGame();
            }
        }
    }

    /// <summary>
    /// End the current game
    /// </summary>
    private void EndGame()
    {
        AddReward(1f);
        Debug.Log($"Play Win");
        // All done: end the scenario, which is done in the coroutine
        // but to prevent overhaed also here...
        if (isRecording)
        {
            StartCoroutine(DelayActivationCoroutine());
        }
        else
        {
            EndEpisode();
        }
    }

    /// <summary>
    /// Count the number of balls inside the arc
    /// </summary>
    /// <returns>The number of balls that are inside the arc</returns>
    private int BallsInArc()
    {
        return ballsOnBoard.Count(x => x.Value.Script.IsInsideArc == true);
    }

    /// <summary>
    /// Delay execution for a given number of seconds
    /// </summary>
    /// <returns></returns>
    public IEnumerator DelayActivationCoroutine()
    {
        if (isRecording)
            paused = true;
        // change color to blue
        meshRenderer.material.color = Color.blue;
        yield return new WaitForSeconds(5);
        paused = false;
        EndEpisode();
    }
    #endregion
}
