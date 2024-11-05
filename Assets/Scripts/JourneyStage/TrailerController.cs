using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrailerController : MonoBehaviour
{
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

    private Rigidbody trailerRigidbody;
    private List<Vector3> suspensionForces = new List<Vector3>();
    private List<Vector3> slipForces = new List<Vector3>();
    private List<Vector3> rollingResistanceForces = new List<Vector3>();

    private void Start()
    {
        trailerRigidbody = GetComponent<Rigidbody>();

        trailerRigidbody.centerOfMass = new Vector3(0, 3f, 0);

        // Default grip curve if not set
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
        UpdateWheelVisuals();
    }

    private void FixedUpdate()
    {
        // Clear previous frame's forces
        suspensionForces.Clear();
        slipForces.Clear();
        rollingResistanceForces.Clear();

        // Apply forces to each wheel
        ApplyWheelForces(frontLeftWheel);
        ApplyWheelForces(frontRightWheel);
        ApplyWheelForces(rearLeftWheel);
        ApplyWheelForces(rearRightWheel);

        // Visualize forces if needed
        VisualizeForcesOnTrailer();
    }

    private void ApplyWheelForces(Transform tireTransform)
    {
        // Cast ray to simulate suspension
        Ray tireRay = new Ray(tireTransform.position, -tireTransform.up);
        RaycastHit hit;
        bool rayDidHit = Physics.SphereCast(tireRay, 0.1f, out hit, suspensionRestDist * 1.5f);

        if (!rayDidHit) return;

        // Apply suspension force
        ApplySuspensionForce(tireTransform, hit);

        // Apply slip force for realistic trailer dynamics
        ApplySlipForce(tireTransform);
    }

    private void ApplySuspensionForce(Transform tireTransform, RaycastHit tireRay)
    {
        Vector3 springDir = tireTransform.up;
        Vector3 tireWorldVel = trailerRigidbody.GetPointVelocity(tireTransform.position);

        float offset = suspensionRestDist - tireRay.distance;
        float vel = Vector3.Dot(springDir, tireWorldVel);
        float force = (offset * springStrength) - (vel * springDamper);

        trailerRigidbody.AddForceAtPosition(springDir * force, tireTransform.position);
        suspensionForces.Add(springDir * force);
    }

    private void ApplySlipForce(Transform tireTransform)
    {
        Vector3 slipDir = tireTransform.right;
        Vector3 tireWorldVel = trailerRigidbody.GetPointVelocity(tireTransform.position);

        float slipVel = Vector3.Dot(slipDir, tireWorldVel);
        float gripFactor = tireGripCurve.Evaluate(Mathf.Abs(slipVel));

        // Adjust the slip force calculation to be more stable at higher speeds
        float velocityMagnitude = trailerRigidbody.velocity.magnitude;
        float clampedGripFactor = Mathf.Clamp(velocityMagnitude / 50f, 0.5f, 1f);
        float desiredVelChange = -slipVel * gripFactor * clampedGripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

        trailerRigidbody.AddForceAtPosition(slipDir * tireMass * desiredAccel, tireTransform.position);
        slipForces.Add(slipDir * tireMass * desiredAccel);
    }

    private void VisualizeForcesOnTrailer()
    {
        // Suspension forces
        foreach (var force in suspensionForces)
        {
            Debug.DrawLine(frontLeftWheel.position, frontLeftWheel.position + force * 0.1f, Color.green);
            Debug.DrawLine(frontRightWheel.position, frontRightWheel.position + force * 0.1f, Color.green);
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.green);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.green);
        }

        // Slip/slip forces
        foreach (var force in slipForces)
        {
            Debug.DrawLine(frontLeftWheel.position, frontLeftWheel.position + force * 0.1f, Color.red);
            Debug.DrawLine(frontRightWheel.position, frontRightWheel.position + force * 0.1f, Color.red);
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.red);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.red);
        }

        // Rolling resistance
        foreach (var force in rollingResistanceForces)
        {
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.blue);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.blue);
        }
    }

    private void UpdateWheelVisuals()
    {
        float wheelRadius = 1f;

        // Calculate rotation angle based on the actual forward speed at each wheel
        RotateWheel(frontLeftWheel, wheelRadius);
        RotateWheel(frontRightWheel, wheelRadius);
        RotateWheel(rearLeftWheel, wheelRadius);
        RotateWheel(rearRightWheel, wheelRadius);
    }

    private void RotateWheel(Transform wheel, float radius)
    {
        // Calculate the forward speed at the wheel position
        Vector3 wheelVelocity = trailerRigidbody.GetPointVelocity(wheel.position);
        float forwardSpeed = Vector3.Dot(wheelVelocity, wheel.forward); // Project velocity onto the wheel's forward direction

        // Convert forward speed to RPM (revolutions per minute)
        float rpm = -(forwardSpeed / (2f * Mathf.PI * radius)) * 60f;

        // Calculate rotation angle based on RPM
        float rotationAngle = Time.deltaTime * rpm * 360f / 60f;

        // Rotate the wheel mesh (assuming it’s a child object of the wheel transform)
        if (wheel.childCount > 0)
        {
            Transform wheelMesh = wheel.GetChild(0);
            wheelMesh.Rotate(0, 0, rotationAngle);
        }
    }
}
