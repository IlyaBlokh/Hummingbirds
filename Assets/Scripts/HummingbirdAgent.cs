using Unity.MLAgents;
using UnityEngine;

public class HummingbirdAgent : Agent
{
    public Transform beakTip;
    public Camera playerCamera;
    public bool trainingMode;
    [Header("Movement")]
    public float moveForce = 2f;
    public float pitchSpeed = 100f;
    public float yawSpeed = 100f;
}
