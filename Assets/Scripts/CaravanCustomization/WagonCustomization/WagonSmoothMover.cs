using System.Collections.Generic;
using UnityEngine;

public class WagonSmoothMover : MonoBehaviour
{
    public float moveDuration = 1f; // Duration for the movement
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _elapsedTime;
    private bool _isMoving;

    // Event to signal the end of movement
    public System.Action OnMovementCompleted;

    /// <summary>
    /// Initializes the movement for the wagon.
    /// </summary>
    /// <param name="targetPosition">The target position for the wagon.</param>
    public void StartMoving(Vector3 targetPosition)
    {
        _startPosition = transform.position;
        _targetPosition = targetPosition;
        _elapsedTime = 0f;
        _isMoving = true;
    }

    private void Update()
    {
        if (_isMoving)
        {
            MoveSmoothly();
        }
    }

    private void MoveSmoothly()
    {
        if (_elapsedTime < moveDuration)
        {
            _elapsedTime += Time.deltaTime;

            // Calculate the normalized progress
            float progress = Mathf.Clamp01(_elapsedTime / moveDuration);

            // Smoothly interpolate between the positions
            Vector3 intermediatePosition = Vector3.Slerp(_startPosition, _targetPosition, progress);
            intermediatePosition.y = Mathf.Lerp(_startPosition.y, _targetPosition.y, progress);

            // Apply the position
            transform.position = intermediatePosition;
        }
        else
        {
            // Snap to the final position
            transform.position = _targetPosition;

            // Stop movement
            _isMoving = false;

            // Trigger any post-movement behavior
            GetComponent<GridManager>()?.InitializeGrid();

            // Invoke the completion event
            OnMovementCompleted?.Invoke();
        }
    }
}
