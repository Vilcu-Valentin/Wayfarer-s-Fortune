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
    [SerializeField] private float wheelRadius = 1f;
    [SerializeField] private List<Wheel> wheels;

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
        foreach (var wheel in wheels)
        {
            ApplyWheelForces(wheel);
        }
    }

    private void ApplyWheelForces(Wheel wheel)
    {
        // Cast ray to simulate suspension
        Ray tireRay = new Ray(wheel.wheelTransform.position, -wheel.wheelTransform.up);
        RaycastHit hit;
        bool rayDidHit = Physics.SphereCast(tireRay, 0.1f, out hit, suspensionRestDist * 1.5f);

        if (!rayDidHit) return;

        // Apply suspension force
        ApplySuspensionForce(wheel, hit);

        // Apply slip force for realistic trailer dynamics
        ApplySlipForce(wheel);
    }

    private void ApplySuspensionForce(Wheel wheel, RaycastHit tireRay)
    {
        Vector3 springDir = wheel.wheelTransform.up;
        Vector3 tireWorldVel = trailerRigidbody.GetPointVelocity(wheel.wheelTransform.position);

        float offset = suspensionRestDist - tireRay.distance;
        float vel = Vector3.Dot(springDir, tireWorldVel);
        float force = (offset * springStrength) - (vel * springDamper);

        trailerRigidbody.AddForceAtPosition(springDir * force, wheel.wheelTransform.position);
        suspensionForces.Add(springDir * force);
    }

    private void ApplySlipForce(Wheel wheel)
    {
        Vector3 slipDir = wheel.wheelTransform.right;
        Vector3 tireWorldVel = trailerRigidbody.GetPointVelocity(wheel.wheelTransform.position);

        float slipVel = Vector3.Dot(slipDir, tireWorldVel);
        float gripFactor = tireGripCurve.Evaluate(Mathf.Abs(slipVel));

        // Adjust the slip force calculation to be more stable at higher speeds
        float velocityMagnitude = trailerRigidbody.velocity.magnitude;
        float clampedGripFactor = Mathf.Clamp(velocityMagnitude / 50f, 0.5f, 1f);
        float desiredVelChange = -slipVel * gripFactor * clampedGripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

        trailerRigidbody.AddForceAtPosition(slipDir * tireMass * desiredAccel, wheel.wheelTransform.position);
        slipForces.Add(slipDir * tireMass * desiredAccel);
    }



    private void UpdateWheelVisuals()
    {
        // Calculate rotation angle based on the actual forward speed at each wheel
        foreach (var wheel in wheels)
        {
            RotateWheel(wheel.wheelTransform, wheelRadius);
        }
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
