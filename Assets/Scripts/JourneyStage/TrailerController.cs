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
    private List<Vector3> steeringForces = new List<Vector3>();
    private List<Vector3> rollingResistanceForces = new List<Vector3>();

    private void Start()
    {
        trailerRigidbody = GetComponent<Rigidbody>();

        // Set the center of mass lower for stability
        trailerRigidbody.centerOfMass = new Vector3(0, -0.5f, 0);

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
        steeringForces.Clear();
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
        bool rayDidHit = Physics.SphereCast(tireRay, 0.15f, out hit, suspensionRestDist * 1.5f);

        if (!rayDidHit) return;

        // Apply suspension force
        ApplySuspensionForce(tireTransform, hit);

        // Apply slip force for realistic trailer dynamics
        ApplySlipForce(tireTransform);

        // Apply rolling resistance to simulate friction
        ApplyRollingResistance(tireTransform);
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
        Vector3 steeringDir = tireTransform.right;
        Vector3 tireWorldVel = trailerRigidbody.GetPointVelocity(tireTransform.position);

        float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
        float gripFactor = tireGripCurve.Evaluate(Mathf.Abs(steeringVel));
        float desiredVelChange = -steeringVel * gripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

        trailerRigidbody.AddForceAtPosition(steeringDir * tireMass * desiredAccel, tireTransform.position);
        steeringForces.Add(steeringDir * tireMass * desiredAccel);
    }

    private void ApplyRollingResistance(Transform tireTransform)
    {
        Vector3 resistanceDir = -trailerRigidbody.GetPointVelocity(tireTransform.position).normalized;
        float resistanceCoefficient = 0.1f; // Fine-tune this for desired rolling resistance
        float resistanceForce = resistanceCoefficient * trailerRigidbody.mass;

        trailerRigidbody.AddForceAtPosition(resistanceDir * resistanceForce, tireTransform.position);
        rollingResistanceForces.Add(resistanceDir * resistanceForce);
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

        // Slip/steering forces
        foreach (var force in steeringForces)
        {
            Debug.DrawLine(frontLeftWheel.position, frontLeftWheel.position + force * 0.1f, Color.blue);
            Debug.DrawLine(frontRightWheel.position, frontRightWheel.position + force * 0.1f, Color.blue);
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.blue);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.blue);
        }

        // Rolling resistance
        foreach (var force in rollingResistanceForces)
        {
            Debug.DrawLine(rearLeftWheel.position, rearLeftWheel.position + force * 0.1f, Color.red);
            Debug.DrawLine(rearRightWheel.position, rearRightWheel.position + force * 0.1f, Color.red);
        }
    }

    private void UpdateWheelVisuals()
    {
        // Calculate rotation based on signed vehicle speed (forward/backward)
        float wheelRadius = 0.75f; // Assuming wheel radius of 0.75m
        float carSpeed = -Vector3.Dot(trailerRigidbody.velocity, transform.forward); // Signed speed (forward/backward)
        float rpm = (carSpeed / (2f * Mathf.PI * wheelRadius)) * 60f; // Convert speed to RPM
        float rotationAngle = Time.deltaTime * rpm * 360f / 60f;

        // Rotate each wheel mesh (assuming they are child objects of the wheel transforms)
        if (frontLeftWheel.childCount > 0) frontLeftWheel.GetChild(0).Rotate(0, rotationAngle, 0);
        if (frontRightWheel.childCount > 0) frontRightWheel.GetChild(0).Rotate(0, rotationAngle, 0);
        if (rearLeftWheel.childCount > 0) rearLeftWheel.GetChild(0).Rotate(0, rotationAngle, 0);
        if (rearRightWheel.childCount > 0) rearRightWheel.GetChild(0).Rotate(0, rotationAngle, 0);
    }
}
