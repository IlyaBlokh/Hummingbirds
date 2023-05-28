using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HummingbirdAgent : Agent
{
    private const float MaxPitchAngle = 80f;
    private const float BeakTipRadius = 0.008f;
    
    [SerializeField]
    private FlowerArea flowerArea;
    
    private new Rigidbody rigidbody;
    private Flower nearestFlower;

    private float smoothPitchChange;
    private float smoothYawChange;
    private bool frozen;
    
    public Transform beakTip;
    public Camera playerCamera;
    public bool trainingMode;
    [Header("Movement")]
    public float moveForce = 2f;
    public float pitchSpeed = 100f;
    public float yawSpeed = 100f;
    
    public float NectarObtained { get; private set; }

    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (flowerArea == null)
            Debug.LogError("Flower area not set for hummingbird agent");

        if (!trainingMode)
            MaxStep = 0;
    }

    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            flowerArea.ResetFlowers();
        }

        NectarObtained = 0f;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        bool spawnInFrontOfFlower = true;
        if (trainingMode)
            spawnInFrontOfFlower = Random.value > 0.5f;

        SetRandomSafePosition(spawnInFrontOfFlower);
        UpdateNearestFlower();
    }

    public void SetArea(FlowerArea flowerArea) => 
        this.flowerArea = flowerArea;

    private void SetRandomSafePosition(bool spawnInFrontOfFlower)
    {
        
    }

    private void UpdateNearestFlower()
    {
        
    }
}
