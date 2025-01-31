using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Wheel
{
    public Transform wheelTransform;
    public bool isSteering = false;
    public bool isPowered = false;
}


[RequireComponent(typeof(Rigidbody))]
public class PrimeMoverController : MonoBehaviour
{
    [Header("DriveTrain Properties")]
    [SerializeField] private float carTopSpeed = 30f;
    [SerializeField] private EngineData engine;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float maxBrakeForce = 3000f;

    [Header("Suspension Settings")]
    [SerializeField] private float suspensionRestDist = 0.5f;
    [SerializeField] private float springStrength = 20000f;
    [SerializeField] private float springDamper = 2000f;

    [Header("Grip Settings")]
    [SerializeField] private AnimationCurve tireGripCurve;
    [SerializeField] private float tireMass = 20f;

    [Header("Wheel Transforms")]
    [SerializeField] private float wheelRadius = 1f;
    [SerializeField] private List<Wheel> wheels;

    private Rigidbody carRigidbody;
    private float accelInput;
    private float steerInput;
    private float brakeInput;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();

        carRigidbody.centerOfMass = new Vector3(0, 2f, 0);

        // Ensure we have a power curve if none is set
        if (engine.torqueCurve == null || engine.torqueCurve.length == 0)
        {
            engine.torqueCurve = new AnimationCurve(
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
        // Apply forces to each wheel
        foreach (var wheel in wheels)
        {
            ApplyWheelForces(wheel);
        }
    }

    private void ApplyWheelForces(Wheel wheel)
    {
        // Adjust wheel rotation based on steering
        if (wheel.isSteering)
        {
            wheel.wheelTransform.localRotation = Quaternion.Euler(
                wheel.wheelTransform.localRotation.eulerAngles.x,
                steerInput * maxSteerAngle,
                wheel.wheelTransform.localRotation.eulerAngles.z
            );
        }

        // Cast ray from wheel
        Ray tireRay = new Ray(wheel.wheelTransform.position, -wheel.wheelTransform.up);
        RaycastHit hit;
        bool rayDidHit = Physics.SphereCast(tireRay, 0.15f, out hit, suspensionRestDist * 1.5f);

        if (!rayDidHit) return;

        // Apply suspension force
        ApplySuspensionForce(wheel, hit);

        // Apply slip force
        ApplySlipForce(wheel);

        // Apply acceleration/brake force
        ApplyDriveForce(wheel);
    }

    private void ApplySuspensionForce(Wheel wheel, RaycastHit tireRay)
    {
        Vector3 springDir = wheel.wheelTransform.up;
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(wheel.wheelTransform.position);

        float offset = suspensionRestDist - tireRay.distance;
        float vel = Vector3.Dot(springDir, tireWorldVel);
        float force = (offset * springStrength) - (vel * springDamper);

        carRigidbody.AddForceAtPosition(springDir * force, wheel.wheelTransform.position);
    }

    private void ApplySlipForce(Wheel wheel)
    {
        Vector3 slipDir = wheel.wheelTransform.right;
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(wheel.wheelTransform.position);

        float slipVel = Vector3.Dot(slipDir, tireWorldVel);
        float gripFactor = tireGripCurve.Evaluate(Mathf.Abs(slipVel));
        float desiredVelChange = -slipVel * gripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

        carRigidbody.AddForceAtPosition(slipDir * tireMass * desiredAccel, wheel.wheelTransform.position);
    }

    private void ApplyDriveForce(Wheel wheel)
    {
        Vector3 accelDir = wheel.wheelTransform.forward;

        // Handle acceleration
        if (accelInput != 0f && wheel.isPowered)
        {
            float carSpeed = Vector3.Dot(transform.forward, carRigidbody.velocity);
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
            float availableTorque = engine.torqueCurve.Evaluate(normalizedSpeed) * engine.power * accelInput;

            carRigidbody.AddForceAtPosition(accelDir * availableTorque, wheel.wheelTransform.position);
        }

        // Handle braking
        if (brakeInput > 0f)
        {
            Vector3 brakeForce = -carRigidbody.GetPointVelocity(wheel.wheelTransform.position).normalized * maxBrakeForce * brakeInput;
            carRigidbody.AddForceAtPosition(brakeForce, wheel.wheelTransform.position);
        }
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
            wheelMesh.Rotate(0, 0, -rotationAngle);
        }
    }
}