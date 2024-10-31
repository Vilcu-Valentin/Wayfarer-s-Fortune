using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody))]
public class PrimeMoverController : MonoBehaviour
{
    [Header("Car Properties")]
    [SerializeField] private float carTopSpeed = 30f;
    [SerializeField] private AnimationCurve powerCurve;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float maxBrakeForce = 3000f;

    [Header("Suspension Settings")]
    [SerializeField] private float suspensionRestDist = 0.5f;
    [SerializeField] private float springStrength = 20000f;
    [SerializeField] private float springDamper = 2000f;
    [SerializeField] private AnimationCurve tireGripCurve;
    [SerializeField] private float tireMass = 20f;

    [Header("Wheel Transforms")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    private Rigidbody carRigidbody;
    private float accelInput;
    private float steerInput;
    private float brakeInput;

    private List<Vector3> suspensionForces = new List<Vector3>();
    private List<Vector3> steeringForces = new List<Vector3>();
    private List<Vector3> driveForces = new List<Vector3>();

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();

        // Set the center of mass lower for better stability
        carRigidbody.centerOfMass = new Vector3(0, 1f, 0);

        // Ensure we have a power curve if none is set
        if (powerCurve == null || powerCurve.length == 0)
        {
            powerCurve = new AnimationCurve(
                new Keyframe(0, 3000),
                new Keyframe(0.5f, 2000),
                new Keyframe(1, 1000)
            );
        }

        // Ensure we have a grip curve if none is set
        if (tireGripCurve == null || tireGripCurve.length == 0)
        {
            tireGripCurve = new AnimationCurve(
                new Keyframe(0, 0.2f),
                new Keyframe(5, 0.7f),
                new Keyframe(10, 1f),
                new Keyframe(20, 1f)
            );
        }
    }

    private void Update()
    {
        // Get input
        accelInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        // Update visual wheel rotations
        UpdateWheelVisuals();
    }

    private void FixedUpdate()
    {
        // Clear force visualization lists
        suspensionForces.Clear();
        steeringForces.Clear();
        driveForces.Clear();

        // Apply forces to each wheel
        ApplyWheelForces(frontLeftWheel, true);
        ApplyWheelForces(frontRightWheel, true);
        ApplyWheelForces(rearLeftWheel, false);
        ApplyWheelForces(rearRightWheel, false);

        // Visualize the forces
        VisualizeForcesOnVehicle();
    }

    private void ApplyWheelForces(Transform tireTransform, bool isFrontWheel)
    {
        // Adjust wheel rotation based on steering
        if (isFrontWheel)
        {
            tireTransform.localRotation = Quaternion.Euler(
                tireTransform.localRotation.eulerAngles.x,
                steerInput * maxSteerAngle,
                tireTransform.localRotation.eulerAngles.z
            );
        }

        // Cast ray from wheel
        Ray tireRay = new Ray(tireTransform.position, -tireTransform.up);
        RaycastHit hit;
        bool rayDidHit = Physics.SphereCast(tireRay, 0.15f, out hit, suspensionRestDist * 1.5f);

        if (!rayDidHit) return;

        // Apply suspension force
        ApplySuspensionForce(tireTransform, hit);

        // Apply steering force
        ApplySteeringForce(tireTransform);

        // Apply acceleration/brake force
        ApplyDriveForce(tireTransform);
    }

    private void ApplySuspensionForce(Transform tireTransform, RaycastHit tireRay)
    {
        Vector3 springDir = tireTransform.up;
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(tireTransform.position);

        float offset = suspensionRestDist - tireRay.distance;
        float vel = Vector3.Dot(springDir, tireWorldVel);
        float force = (offset * springStrength) - (vel * springDamper);

        carRigidbody.AddForceAtPosition(springDir * force, tireTransform.position);
        suspensionForces.Add(springDir * force);
    }

    private void ApplySteeringForce(Transform tireTransform)
    {
        Vector3 steeringDir = tireTransform.right;
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(tireTransform.position);

        float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
        float gripFactor = tireGripCurve.Evaluate(Mathf.Abs(steeringVel));
        float desiredVelChange = -steeringVel * gripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

        carRigidbody.AddForceAtPosition(steeringDir * tireMass * desiredAccel, tireTransform.position);
        steeringForces.Add(steeringDir * tireMass * desiredAccel);
    }

    private void ApplyDriveForce(Transform tireTransform)
    {
        Vector3 accelDir = tireTransform.forward;

        // Handle acceleration
        if (accelInput != 0f)
        {
            float carSpeed = Vector3.Dot(transform.forward, carRigidbody.velocity);
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
            float availableTorque = powerCurve.Evaluate(normalizedSpeed) * accelInput;

            carRigidbody.AddForceAtPosition(accelDir * availableTorque, tireTransform.position);
            driveForces.Add(accelDir * availableTorque);
        }
        else
        {
            // Add rolling resistance
            float rollingResistanceCoefficient = 0.1f; // Adjust this value to change the rolling resistance
            float rollingResistanceForce = -carRigidbody.GetPointVelocity(tireTransform.position).magnitude * rollingResistanceCoefficient * carRigidbody.mass;
            carRigidbody.AddForceAtPosition(accelDir * rollingResistanceForce, tireTransform.position);
            driveForces.Add(accelDir * rollingResistanceForce);
        }

        // Handle braking
        if (brakeInput > 0f)
        {
            Vector3 brakeForce = -carRigidbody.GetPointVelocity(tireTransform.position).normalized * maxBrakeForce * brakeInput;
            carRigidbody.AddForceAtPosition(brakeForce, tireTransform.position);
            driveForces.Add(brakeForce);
        }
    }

    private void VisualizeForcesOnVehicle()
    {
        // Draw suspension forces
        foreach (var force in suspensionForces)
        {
            Debug.DrawLine(frontLeftWheel.position, frontLeftWheel.position + force * 0.1f, Color.green);
            Debug.DrawLine(frontRightWheel.position, frontRightWheel.position + force * 0.1f, Color.green);
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.green);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.green);
        }

        // Draw steering forces
        foreach (var force in steeringForces)
        {
            Debug.DrawLine(frontLeftWheel.position, frontLeftWheel.position + force * 0.1f, Color.blue);
            Debug.DrawLine(frontRightWheel.position, frontRightWheel.position + force * 0.1f, Color.blue);
        }

        // Draw drive forces
        foreach (var force in driveForces)
        {
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.red);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.red);
        }
    }

    private void UpdateWheelVisuals()
{
    // Assuming wheel radius of 0.75m
    float wheelRadius = 0.75f;
    
    // Calculate rotation angle based on the actual forward speed at each wheel
    RotateWheel(frontLeftWheel, wheelRadius);
    RotateWheel(frontRightWheel, wheelRadius);
    RotateWheel(rearLeftWheel, wheelRadius);
    RotateWheel(rearRightWheel, wheelRadius);
}

private void RotateWheel(Transform wheel, float radius)
{
    // Calculate the forward speed at the wheel position
    Vector3 wheelVelocity = carRigidbody.GetPointVelocity(wheel.position);
    float forwardSpeed = Vector3.Dot(wheelVelocity, wheel.forward); // Project velocity onto the wheel's forward direction

    // Convert forward speed to RPM (revolutions per minute)
    float rpm = -(forwardSpeed / (2f * Mathf.PI * radius)) * 60f;

    // Calculate rotation angle based on RPM
    float rotationAngle = Time.deltaTime * rpm * 360f / 60f;

    // Rotate the wheel mesh (assuming it’s a child object of the wheel transform)
    if (wheel.childCount > 0)
    {
        Transform wheelMesh = wheel.GetChild(0);
        wheelMesh.Rotate(0, rotationAngle, 0);
    }
}


}