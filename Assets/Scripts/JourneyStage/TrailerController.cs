using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrailerController : MonoBehaviour
{
    [Header("Connection Settings")]
    public Transform connectionPoint;
    public float maxRotationAngle = 60f;
    public float rotationStiffness = 1000f;
    public float rotationDamping = 100f;

    [Header("Physics Settings")]
    public float mass = 500f;
    public float stabilityForce = 5000f;

    private ConfigurableJoint joint;
    private Rigidbody rb;
    private bool isDetached = false;

    private void Start()
    {
        SetupPhysics();
        SetupJoint();
    }

    private void SetupPhysics()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.centerOfMass = Vector3.down * 0.5f; // Lower center of mass for better stability
    }

    private void SetupJoint()
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = connectionPoint.GetComponentInParent<Rigidbody>();

        // Configure joint position
        joint.anchor = transform.InverseTransformPoint(connectionPoint.position);

        // Lock linear motion
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        // Configure rotation
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        // Set rotation limits
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = maxRotationAngle;
        joint.highAngularXLimit = limit;
        joint.angularYLimit = limit;
        joint.angularZLimit = limit;

        // Configure joint drive
        JointDrive drive = new JointDrive();
        drive.positionSpring = rotationStiffness;
        drive.positionDamper = rotationDamping;
        drive.maximumForce = Mathf.Infinity;

        joint.angularXDrive = drive;
        joint.angularYZDrive = drive;
    }

    private void FixedUpdate()
    {
        if (!isDetached)
        {
            CheckTrailerTipping();
            ApplyStabilityForce();
        }
    }

    private void CheckTrailerTipping()
    {
        float tippingAngle = Vector3.Angle(transform.up, Vector3.up);
        if (tippingAngle > 60f) // Adjust threshold as needed
        {
            DetachTrailer();
        }
    }

    private void ApplyStabilityForce()
    {
        // Apply upward force to prevent excessive bouncing
        rb.AddForce(Vector3.up * stabilityForce * Time.fixedDeltaTime);

        // Apply force to keep trailer level
        Vector3 rightVector = transform.right;
        rightVector.y = 0;
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
        Vector3 correctionTorque = Vector3.Cross(transform.up, Vector3.up) * (tiltAngle * stabilityForce);
        rb.AddTorque(correctionTorque * Time.fixedDeltaTime);
    }

    private void DetachTrailer()
    {
        if (joint != null)
        {
            Destroy(joint);
            isDetached = true;
            Debug.Log("Trailer detached due to tipping!");
        }
    }
}