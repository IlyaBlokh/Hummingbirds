using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay
{
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
        public Camera agentCamera;
        public bool trainingMode;
        [Header("Movement")]
        public float moveForce = 2f;
        public float pitchSpeed = 100f;
        public float yawSpeed = 100f;
    
        public float NectarObtained { get; private set; }

        public void SetArea(FlowerArea flowerArea) => 
            this.flowerArea = flowerArea;

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

        public override void CollectObservations(VectorSensor sensor)
        {
            if (nearestFlower == null)
            {
                sensor.AddObservation(new float[10]);
                return;
            }
        
            sensor.AddObservation(transform.localRotation.normalized);

            Vector3 toFlower = nearestFlower.FlowerCenterPosition - beakTip.position;
            sensor.AddObservation(toFlower.normalized);
        
            sensor.AddObservation(Vector3.Dot(toFlower.normalized, -nearestFlower.FlowerUpVector.normalized));
            sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -nearestFlower.FlowerUpVector.normalized));

            sensor.AddObservation(toFlower.magnitude / FlowerArea.AreaDiameter);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Vector3 forward = Vector3.zero;
            Vector3 left = Vector3.zero;
            Vector3 up = Vector3.zero;
            float pitch = 0f;
            float yaw = 0f;

            if (Input.GetKey(KeyCode.W))
                forward = transform.forward;
            if (Input.GetKey(KeyCode.S))
                forward = -transform.forward;
            if (Input.GetKey(KeyCode.A))
                left = -transform.right;
            if (Input.GetKey(KeyCode.D))
                left = transform.right;
            if (Input.GetKey(KeyCode.E))
                up = transform.up;
            if (Input.GetKey(KeyCode.Q))
                up = -transform.up;
            if (Input.GetKey(KeyCode.UpArrow))
                pitch = 1f;
            if (Input.GetKey(KeyCode.DownArrow))
                pitch = -1f;
            if (Input.GetKey(KeyCode.LeftArrow))
                yaw = -1f;
            if (Input.GetKey(KeyCode.RightArrow))
                yaw = 1f;

            Vector3 movement = (forward + left + up).normalized;
            ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = movement.x;
            continuousActionsOut[1] = movement.y;
            continuousActionsOut[2] = movement.z;
            continuousActionsOut[3] = pitch;
            continuousActionsOut[4] = yaw;
            ;    }

        public void FreezeAgent()
        {
            Debug.Assert(trainingMode == false, "Freeze/Unfreeze is not supported for training mode");
            frozen = true;
            rigidbody.Sleep();
        }

        public void UnfreezeAgent()
        {
            Debug.Assert(trainingMode == false, "Freeze/Unfreeze is not supported for training mode");
            frozen = false;
            rigidbody.WakeUp();
        }

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterOrStay(other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTriggerEnterOrStay(other);
        }

        private void OnTriggerEnterOrStay(Collider collider)
        {
            if (collider.CompareTag("Nectar"))
            {
                Vector3 closestPointToBeakTip = collider.ClosestPoint(beakTip.position);
                if (beakTip.position.SqrMagnitudeTo(closestPointToBeakTip) <= BeakTipRadius * BeakTipRadius)
                {
                    Flower flower = flowerArea.GetFlowerWithCollider(collider);
                    float nectarReceived = flower.Feed(0.01f);
                    NectarObtained += nectarReceived;
                
                    if (trainingMode)
                    {
                        float bonus = 0.2f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestFlower.FlowerUpVector.normalized));
                        AddReward(0.1f + bonus);
                    }
                
                    if (!flower.HasNectar)
                        UpdateNearestFlower();
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (trainingMode && other.collider.CompareTag("Boundary"))
                AddReward(-0.5f);
        }

        private void Update()
        {
            if (nearestFlower != null)
                Debug.DrawLine(beakTip.position, nearestFlower.FlowerCenterPosition, Color.green);
        }

        private void FixedUpdate()
        {
            //update the target flower in case when the nectar was stolen by opponent and the nectar collider was turned off
            if (nearestFlower != null && !nearestFlower.HasNectar)
                UpdateNearestFlower();
        }

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
}
