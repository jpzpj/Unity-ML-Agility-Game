using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BallEvent : UnityEvent<Ball>
{
}

/// <summary>
/// Script belonging to a ball game object
/// </summary>
public class Ball : MonoBehaviour
{
    // Events to be used to get informed whether the ball entered or left the arc
    [HideInInspector] public BallEvent OnArcEnter = new BallEvent();
    [HideInInspector] public BallEvent OnArcExit = new BallEvent();

    // Return true if the ball is inside a collider (inside the arc)
    public bool IsInsideArc { get; private set; }
    public int ID { get; internal set; }

    public void Initialize()
    {
        this.IsInsideArc = false;
    }

    /// <summary>
    /// The ball entered the arc
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            IsInsideArc = true;
            if (OnArcEnter != null) OnArcEnter.Invoke(this);
        }
    }

    /// <summary>
    /// The ball left the arc
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            IsInsideArc = false;
            if (OnArcExit != null) OnArcExit.Invoke(this);
        }
    }

}
