using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Keeps info about the gameobject and the script object of a ball
/// </summary>
public class BallOnBoard
{
    /// <summary>
    /// Reference to the Unity gameobject the ball script and the balls rigidbody
    /// to make fast usage possible
    /// </summary>
    public GameObject Object { get; private set; }
    /// <summary>
    /// Reference to the Unity ball script 
    /// </summary>
    public Ball Script { get; private set; }
    /// <summary>
    /// reference to the rigidbody of the ball gameobject
    /// </summary>
    public Rigidbody RigidBody { get; private set; }

    /// <summary>
    /// The name of the ball
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Keep track of how many times in the gameplay this ball entered the arc
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="name">The nam eof the ball</param>
    /// <param ame="ballGameObject">The Unity gameobject</param>
    public BallOnBoard(string name, GameObject ballGameObject)
    {
        this.Name = name;
        this.Object = ballGameObject;

        this.RigidBody = ballGameObject.GetComponent<Rigidbody>();
        this.RigidBody.velocity = new Vector3(0f, 0f, 0f);

        this.Script = ballGameObject.GetComponent<Ball>();

        this.Score = 0;
    }

    public void IncreaseScore()
    {
        this.Score++;
    }
}
