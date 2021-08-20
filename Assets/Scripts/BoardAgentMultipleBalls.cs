using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;

public class BoardAgentMultipleBalls : Agent
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
    /// The maximum speed the ball is moving, needed for normalization of the speed value
    /// </summary>
    private const float maxSpeed = 30;  // Derived during various game plays

    /// <summary>
    /// Start is called by Unity before the first frame update
    /// </summary>
    void Start()
    {
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
    /// OnEpisodeBegin is called by ML during the start of a new enpisode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Set the start rotation of the board, so the balls start moving.
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        // The board may be given an initial rotation, however this is not needed
        // Training with a level board works fine even when the initial board is rotated
        // during normal play
        //transform.Rotate(new Vector3(1, 0, 0), UnityEngine.Random.Range(-10f, 10f));
        //transform.Rotate(new Vector3(0, 0, 1), UnityEngine.Random.Range(-10f, 10f));

        // Put the balls on the board
        foreach (var ball in ballsOnBoard)
        {
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
        sensor.AddObservation(transform.rotation.z);    // 1 observation value, [-1, +1]
        sensor.AddObservation(transform.rotation.x);    // 1 observation value, [-1, +1]
        
        sensor.AddObservation(ballsOnBoard[0].Object.transform.position - transform.position); // 3 observation values [-12 - +12] 3 times ?
        float distance = Vector3.Distance(ballsOnBoard[0].Object.transform.position, entryPosition.transform.position);
        sensor.AddObservation(distance / maxDistance);    // 1 observation value [0 - +1]
        sensor.AddObservation(ballsOnBoard[0].RigidBody.velocity.magnitude / maxSpeed); // 1 observation value [-? - ?]

        // Total 7 observations: this value must match the Space Size value in the Behavior Parameters
        // property of the agent
    }

    /// <summary>
    /// Called by ML to perform an action
    /// </summary>
    /// <param name="vectorAction"></param>
    public override void OnActionReceived(float[] vectorAction)
    {
        float actionZ = Mathf.Clamp(vectorAction[0], -1f, 1f);
        float actionX = Mathf.Clamp(vectorAction[1], -1f, 1f);

        // Save the current rotation
        Quaternion currentQuaternionRotation = transform.rotation;

        transform.Rotate(actionX * rotationSpeed * Vector3.right);
        transform.Rotate(actionZ * rotationSpeed * Vector3.back);

        // If the rotation would be clipped, restore the original rotation
        // Note: if we would perform clipping based on Euler angles, unwanted side effects might occur 
        if (Helper.IsRotationClipped(transform.rotation))
        {
            transform.rotation = currentQuaternionRotation;
            AddReward(-0.02f);
        }

        // ML specific: if one of the balls falls off the board, then add a huge penalty and stop
        // the mission.
        foreach (var ball in ballsOnBoard)
        {
            if (ball.Value.Object.transform.position.y - transform.position.y < -20f)
            {
                AddReward(-2f);
                EndEpisode();
            }

            // Give a penalty when the ball is not moving
            if (ball.Value.RigidBody.velocity.magnitude < 0.01f)
            {
                AddReward(-0.01f);
            }
        }
    }

    /// <summary>
    /// Called by ML in case the Behavior Type is Heuristic or in case there is no model
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");  // RotateZ
        actionsOut[1] = Input.GetAxis("Vertical");    // RotateX
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
            AddReward(-1f);
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
            AddReward(1f);
            if (BallsInArc() == ballsInGame)
            {
                b.IncreaseScore();
                // All done: end the scenario
                EndEpisode();
            }
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
    /// Count the number of balls inside the hotspot
    /// </summary>
    /// <returns>The number of balls that are inside the hotspot</returns>
    private int BallsInHotspot()
    {
        return ballsOnBoard.Count(x => x.Value.Script.IsInsideHotspot == true);
    }
    #endregion
}
