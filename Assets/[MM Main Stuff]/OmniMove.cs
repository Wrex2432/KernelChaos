using UnityEngine;

public class OmniMove : MonoBehaviour
{
    private Rigidbody m_rigidBody;
    private float m_moveSpeed = 2.0f; // Speed multiplier for the player

    void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody>();
    }

    // This method will be called from the BackendConnector when movement is received
    public void ApplyMovement(float vx, float vy, float speed)
    {
        // Normalize the movement vector
        Vector3 moveDirection = new Vector3(vx, 0, vy).normalized;

        // Apply the movement to the Rigidbody
        m_rigidBody.linearVelocity = moveDirection * speed * m_moveSpeed;

        // Optional: Rotate the player to face the direction of movement
        if (moveDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
