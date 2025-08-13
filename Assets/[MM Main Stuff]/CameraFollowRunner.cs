using UnityEngine;

public class CameraForwardFollow : MonoBehaviour {
    public float forwardSpeed = 2f;         // Match PlayerAvatarRunner.forwardSpeed
    public float verticalOffset = 5f;       // Optional
    public float distanceBehind = 10f;      // Optional

    //private bool hasStarted = false;

    void LateUpdate () {
        if (!PlayerTracker.Instance || !PlayerTracker.Instance.gameStarted) return;

        // Move camera forward
        transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
    }
}
