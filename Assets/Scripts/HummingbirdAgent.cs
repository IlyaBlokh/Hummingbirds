using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HummingbirdAgent : Agent
{
    private const float MaxPitchAngle = 80f;
    private const float BeakTipRadius = 0.008f;
    private const float BirdRadius = 0.05f;
    
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

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (frozen)
            return;
        ApplyMovement(new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]));
        ApplyRotation(actions.ContinuousActions[3], actions.ContinuousActions[4]);
    }

    public void SetArea(FlowerArea flowerArea) => 
        this.flowerArea = flowerArea;

    private void SetRandomSafePosition(bool spawnInFrontOfFlower)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100;
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = Quaternion.identity;

        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;
            (potentialPosition, potentialRotation) = spawnInFrontOfFlower 
                ? FindPositionRotationInFrontOfFlower() 
                : FindRandomPositionRotation();

            Collider[] results = { };
            int collisionAmount = Physics.OverlapSphereNonAlloc(potentialPosition, BirdRadius, results);
            safePositionFound = collisionAmount == 0;
        }
        
        Debug.Assert(safePositionFound, "Could not find a safe position for hummingbird");
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }

    private void UpdateNearestFlower()
    {
        foreach (Flower flower in flowerArea.Flowers.Where(f => f.HasNectar))
        {
            if (nearestFlower == null)
                nearestFlower = flower;

            if (!nearestFlower.HasNectar || IsFlowerCloserCurrentNearest(flower))
                nearestFlower = flower;
        }
    }

    private bool IsFlowerCloserCurrentNearest(Flower flower) => 
        beakTip.position.SqrMagnitudeTo(flower.transform.position) < beakTip.position.SqrMagnitudeTo(nearestFlower.transform.position);

    private (Vector3, Quaternion) FindPositionRotationInFrontOfFlower()
    {
        Flower randomFlower = flowerArea.Flowers[Random.Range(0, flowerArea.Flowers.Count)];
        float distanceToFlower = Random.Range(0.1f, 0.2f);
        Vector3 potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distanceToFlower;
        Vector3 toFlower = randomFlower.transform.position - potentialPosition;
        Quaternion potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
        return (potentialPosition, potentialRotation);
    }

    private (Vector3, Quaternion) FindRandomPositionRotation()
    {
        float height = Random.Range(1.2f, 2.5f);
        float radius = Random.Range(2f, 7f);
        Quaternion direction = Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f);
        Vector3 potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;
        float pitch = Random.Range(-MaxPitchAngle, MaxPitchAngle);
        float yaw = Random.Range(-180f, 180f);
        Quaternion potentialRotation = Quaternion.Euler(pitch, yaw, 0f);
        return (potentialPosition, potentialRotation);
    }

    private void ApplyMovement(Vector3 move) => 
        rigidbody.AddForce(move * moveForce);

    private void ApplyRotation(float pitchChange, float yawChange)
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        float pitch = currentRotation.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f)
            pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

        float yaw = currentRotation.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
