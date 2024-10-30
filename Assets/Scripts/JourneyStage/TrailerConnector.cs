using UnityEngine;

public class TrailerConnector : MonoBehaviour
{
    [Header("Connection Points")]
    [SerializeField] private Transform primeMoverConnectionPoint; // Connection point on PrimeMover
    [SerializeField] private Transform trailerConnectionPoint;    // Connection point on Trailer

    [Header("Connection Settings")]
    [SerializeField] private float alignmentStrength = 1000f;     // Adjusts how strongly the trailer aligns
    [SerializeField] private float maxAngleDeviation = 90f;       // Maximum yaw angle between prime mover and trailer

    private Rigidbody trailerRigidbody;
    private Transform trailerTransform;

    private void Start()
    {
        trailerRigidbody = GetComponent<Rigidbody>();
        trailerTransform = transform;
    }

    private void FixedUpdate()
    {
        // Align the trailer with the prime mover's connection point
        AlignTrailerPosition();

        // Constrain rotation to simulate yaw-only hinge behavior
        ConstrainTrailerRotation();
    }

    private void AlignTrailerPosition()
    {
        // Calculate the offset to align trailer connection point with prime mover's connection point
        Vector3 offset = primeMoverConnectionPoint.position - trailerConnectionPoint.position;

        // Apply force to adjust the trailer's position
        trailerRigidbody.AddForce(offset * alignmentStrength, ForceMode.Acceleration);
    }

    private void ConstrainTrailerRotation()
    {
        // Calculate the direction vectors for alignment
        Vector3 directionToPrimeMover = (primeMoverConnectionPoint.position - trailerConnectionPoint.position).normalized;
        Vector3 trailerForward = trailerTransform.forward;

        // Calculate angle difference
        float angleDifference = Vector3.SignedAngle(trailerForward, directionToPrimeMover, Vector3.up);

        // Check if angle difference exceeds the max deviation
        if (Mathf.Abs(angleDifference) > maxAngleDeviation)
        {
            float correctionAngle = Mathf.Clamp(angleDifference, -maxAngleDeviation, maxAngleDeviation);

            // Rotate the trailer to keep within the max deviation
            Quaternion rotationCorrection = Quaternion.AngleAxis(correctionAngle, Vector3.up);
            trailerRigidbody.MoveRotation(rotationCorrection * trailerRigidbody.rotation);
        }
    }
}
