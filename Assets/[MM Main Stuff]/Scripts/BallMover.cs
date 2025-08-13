using UnityEngine;

public class BallMover : MonoBehaviour {
    public float pullForce = 2f;         // How fast it moves from a pull
    public float returnSpeed = 1f;       // How fast it returns to center
    public float snapThreshold = 0.01f;  // When to snap to exact center

    private float direction = 0f;        // -1 for left, +1 for right
    private float pullTimer = 0f;        // How long to keep applying force

    private Vector3 targetPosition;

    void Start () {
        targetPosition = transform.position;
    }

    void Update () {
        if (pullTimer > 0f) {
            targetPosition += Vector3.right * direction * pullForce * Time.deltaTime;
            pullTimer -= Time.deltaTime;
        } else {
            // Return to center slowly
            targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);

            // Optional: snap to exact zero
            if (Vector3.Distance(targetPosition, Vector3.zero) < snapThreshold) {
                targetPosition = Vector3.zero;
            }
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
    }

    public void MoveLeft () {
        direction = -1f;
        pullTimer = 0.2f; // seconds of active pull
    }

    public void MoveRight () {
        direction = 1f;
        pullTimer = 0.2f;
    }
}
